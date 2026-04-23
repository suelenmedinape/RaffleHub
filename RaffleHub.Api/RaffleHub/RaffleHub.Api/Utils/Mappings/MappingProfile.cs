using AutoMapper;
using RaffleHub.Api.DTOs.Booking;
using RaffleHub.Api.DTOs.CategoriesGallery;
using RaffleHub.Api.DTOs.Gallery;
using RaffleHub.Api.DTOs.Participant;
using RaffleHub.Api.DTOs.Raffle;
using RaffleHub.Api.DTOs.Ticket;
using RaffleHub.Api.Entities;

namespace RaffleHub.Api.Utils.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Raffle Mappings
        CreateMap<Raffle, ListAllRaffleDto>()
            .ForMember(dest => dest.SoldTicketsCount, 
                opt => opt.MapFrom(src => src.Tickets.Count(t => t.ParticipantId != null)));

        CreateMap<Raffle, ListNamesRafflesDto>();

        CreateMap<CreateRaffleDto, Raffle>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.Tickets, opt => opt.Ignore());
        
        CreateMap<UpdateRaffleDto, Raffle>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Tickets, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        CreateMap<UpdateStatusDto, Raffle>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // CategoriesGallery Mappings
        CreateMap<CategoriesGallery, ListAllCategoriesDto>();
        CreateMap<CreateCategoriesGalleryDto, CategoriesGallery>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()));
        CreateMap<UpdateCategoriesGalleryDto, CategoriesGallery>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Gallery Mappings
        CreateMap<Gallery, ListAllGalleryDto>();
        CreateMap<CreateGalleryDto, Gallery>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()));
        CreateMap<UpdateGalleryDto, Gallery>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.FolderName, opt => opt.Ignore());

        // Participant Mappings
        CreateMap<Participant, ListAllParticipantsDto>()
            .ForMember(dest => dest.Tickets, 
                opt => opt.MapFrom(src => src.Tickets.Select(t => t.TicketNumber).ToList()));
        
        CreateMap<Participant, ParticipantDetailDto>();

        CreateMap<CreateParticipantDto, Participant>()
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.Document));
        
        // Mapeamento do Booking para a Resposta da Reserva
        CreateMap<Booking, ParticipantPurchaseResponseDto>()
            .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ParticipantId, opt => opt.MapFrom(src => src.ParticipantId))
            // Mapeia propriedades filhas para a raiz do DTO
            .ForMember(dest => dest.ParticipantName, opt => opt.MapFrom(src => src.Participant.ParticipantName))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Participant.Phone))
            .ForMember(dest => dest.RaffleName, opt => opt.MapFrom(src => src.Raffle.RaffleName))
            .ForMember(dest => dest.TicketNumbers, opt => opt.MapFrom(src => src.Tickets.Select(t => t.TicketNumber).ToList()));

        CreateMap<Booking, ListBookingPendingDto>()
            .ForMember(dest => dest.RaffleName, opt => opt.MapFrom(src => src.Raffle != null ? src.Raffle.RaffleName : string.Empty))
            .ForMember(dest => dest.ParticipantName, opt => opt.MapFrom(src => src.Participant != null ? src.Participant.ParticipantName : string.Empty));
        
        // Ticket Mappings
        CreateMap<Ticket, ListTicketsByParticipant>();
    }
}