using EmDia.Application.DTOs.Itens;
using EmDia.Domain.Enums;
using FluentValidation;

namespace EmDia.Application.Validators;

public class CriarItemRequestValidator : AbstractValidator<CriarItemRequest>
{
    public CriarItemRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.AntecedenciaDias)
            .GreaterThanOrEqualTo((short)1).WithMessage("Antecedência deve ser de pelo menos 1 dia.");

        RuleFor(x => x.VeiculoId)
            .NotNull().When(x => x.TipoVencimento == TipoVencimento.UsoOuTempo)
            .WithMessage("VeiculoId é obrigatório para itens do tipo UsoOuTempo.");
    }
}

public class EditarItemRequestValidator : AbstractValidator<EditarItemRequest>
{
    public EditarItemRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.AntecedenciaDias)
            .GreaterThanOrEqualTo((short)1).WithMessage("Antecedência deve ser de pelo menos 1 dia.");
    }
}
