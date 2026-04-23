using Microsoft.EntityFrameworkCore;
using RaffleHub.Api.Data;
using RaffleHub.Api.Enums;
using RaffleHub.Api.Services.MercadoPago;
using RaffleHub.Api.Services;

namespace RaffleHub.Api.Utils.Job;

public class BookingJob
{
    private readonly AppDbContext _db;
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly BookingService _bookingService;
    private readonly ILogger<BookingJob> _logger;

    public BookingJob(
        AppDbContext db,
        IMercadoPagoService mercadoPagoService,
        BookingService bookingService,
        ILogger<BookingJob> logger)
    {
        _db = db;
        _mercadoPagoService = mercadoPagoService;
        _bookingService = bookingService;
        _logger = logger;
    }

    public async Task ExpirePixAsync()
    {
        var expiredBookings = await _db.Bookings
            .Include(b => b.Tickets)
            .Where(b => b.Status == BookingStatus.PENDING &&
                        b.CreatedAt < DateTime.UtcNow.AddMinutes(-30))
            .ToListAsync();

        if (!expiredBookings.Any()) return;

        foreach (var booking in expiredBookings)
        {
            booking.Status = BookingStatus.EXPIRED;
            foreach (var ticket in booking.Tickets)
            {
                ticket.BookingId = null;
                ticket.ParticipantId = null;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task ExpireBookingAsync(Guid bookingId)
    {
        // Use the repository from bookingService to ensure consistent transaction/locking logic
        // We need to make sure we use a consistent way to handle transactions.
        // Actually, we can use the injected _db here but the repository already has the locking logic.
        // Since BookingRepository also uses the same db context (if registered as scoped), it should work.
        
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Use pessimistic locking to prevent race conditions with payment processing
            // We'll use a local instance of the logic since we have access to _db here
            var booking = await _db.Bookings
                .FromSqlRaw("SELECT * FROM \"Bookings\" WHERE \"Id\" = {0} FOR UPDATE", bookingId)
                .Include(b => b.Tickets)
                .FirstOrDefaultAsync();

            if (booking == null || booking.Status != BookingStatus.PENDING)
            {
                await transaction.RollbackAsync();
                return;
            }

            if (!string.IsNullOrEmpty(booking.TransactionId))
            {
                var statusResult = await _mercadoPagoService.CheckTransactionStatusAsync(booking.TransactionId);
                if (statusResult.IsSuccess && statusResult.Value.Status == "PAID")
                {
                    // If it's paid, we confirm it instead of expiring
                    // ConfirmPaymentByTransactionIdAsync already manages its own transaction
                    await _bookingService.ConfirmPaymentByTransactionIdAsync(booking.TransactionId);
                    return;
                }
            }

            _logger.LogWarning("Expirando reserva {BookingId} e liberando tickets", booking.Id);

            booking.Status = BookingStatus.EXPIRED;

            foreach (var ticket in booking.Tickets)
            {
                ticket.BookingId = null;
                ticket.ParticipantId = null;
            }

            _db.Bookings.Update(booking);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Erro ao expirar reserva {BookingId}", bookingId);
            throw;
        }
    }
}