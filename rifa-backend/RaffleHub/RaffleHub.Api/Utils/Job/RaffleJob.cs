using Microsoft.EntityFrameworkCore;
using RaffleHub.Api.Data;
using RaffleHub.Api.Enums;

namespace RaffleHub.Api.Utils.Job;

public class RaffleJob
{
    private readonly AppDbContext _db;

    public RaffleJob(AppDbContext db)
    {
        _db = db;
    }

    public async Task ExpireRaffles()
    {
        var today = DateTime.UtcNow.Date;

        var total = await _db.Raffle
            .Where(r => r.Status == RaffleStatus.ACTIVE && r.DrawDate < today)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, RaffleStatus.EXPIRED));
        
        Console.WriteLine($"{total} rifa(s) expirada(s) com sucesso.");
    }
}