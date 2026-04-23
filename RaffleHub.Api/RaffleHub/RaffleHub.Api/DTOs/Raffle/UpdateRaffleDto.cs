using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using RaffleHub.Api.Enums;

namespace RaffleHub.Api.DTOs.Raffle;

public class UpdateRaffleDto
{
    [NotNull]
    [Required(ErrorMessage = "O Campo nome não pode estar em branco")]
    public required string RaffleName { get; set; }
    
    [NotNull]
    [Required(ErrorMessage = "O Campo descrição não pode estar em branco")]
    public required string Description { get; set; }
    
    [NotNull]
    [Required(ErrorMessage = "O Campo total de números não pode estar em branco")]
    [Range(0, int.MaxValue, ErrorMessage = "O valor de tickets deve ser um número válido")]
    public int TotalTickets { get; set; }
    
    [NotNull]
    [Required(ErrorMessage = "O Campo preço não pode estar em branco")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser positivo")]
    public decimal TicketPrice { get; set; }
    
    [NotNull]
    [Required(ErrorMessage = "O Campo data do sorteio não pode estar em branco")]
    public DateTime DrawDate { get; set; }
}