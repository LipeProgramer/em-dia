using EmDia.Application.DTOs.Itens;
using EmDia.Application.DTOs.Preferencias;
using FluentValidation;

namespace EmDia.Application.Validators;

public class ConcluirItemRequestValidator : AbstractValidator<ConcluirItemRequest>
{
    public ConcluirItemRequestValidator()
    {
        RuleFor(x => x.KmNaConclusao)
            .GreaterThanOrEqualTo(0).When(x => x.KmNaConclusao.HasValue)
            .WithMessage("Quilometragem na conclusão não pode ser negativa.");

        RuleFor(x => x.ValorGasto)
            .GreaterThanOrEqualTo(0).When(x => x.ValorGasto.HasValue)
            .WithMessage("Valor gasto não pode ser negativo.");
    }
}

public class AtualizarPreferenciasRequestValidator : AbstractValidator<AtualizarPreferenciasRequest>
{
    public AtualizarPreferenciasRequestValidator()
    {
        RuleFor(x => x.MaxReavisos)
            .InclusiveBetween((short)0, (short)5)
            .WithMessage("MaxReavisos deve ser entre 0 e 5.");

        RuleFor(x => x.IntervaloReavisoDias)
            .InclusiveBetween((short)1, (short)7)
            .WithMessage("IntervaloReavisoDias deve ser entre 1 e 7.");
    }
}
