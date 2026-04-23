using FluentValidation;
using RaffleHub.Api.DTOs.Participant;

namespace RaffleHub.Api.Utils.Validators;

public class CreateParticipantValidator : AbstractValidator<CreateParticipantDto>
{
    public CreateParticipantValidator()
    {
        RuleFor(x => x.ParticipantName)
            .NotEmpty().WithMessage("O nome do participante é obrigatório.")
            .MinimumLength(3).WithMessage("O nome deve ter no mínimo 3 caracteres.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("O telefone é obrigatório.")
            .Matches(@"^\d{10,11}$").WithMessage("Telefone inválido. Use apenas números (10 ou 11 dígitos).");

        RuleFor(x => x.Document)
            .NotEmpty().WithMessage("O CPF é obrigatório.")
            .Matches(@"^\d{11}$").WithMessage("O CPF deve conter exatos 11 dígitos numéricos.");

        RuleFor(x => x.RaffleId)
            .NotEmpty().WithMessage("O identificador da rifa é obrigatório.");

        RuleFor(x => x.TicketNumbers)
            .NotEmpty().WithMessage("É necessário escolher pelo menos um número.")
            .Must(x => x != null && x.Count > 0).WithMessage("Lista de números não pode estar vazia.");
    }
}
