using EmDia.Application.DTOs.Veiculos;
using FluentValidation;

namespace EmDia.Application.Validators;

public class CriarVeiculoRequestValidator : AbstractValidator<CriarVeiculoRequest>
{
    public CriarVeiculoRequestValidator()
    {
        RuleFor(x => x.Apelido)
            .NotEmpty().WithMessage("Apelido é obrigatório.")
            .MaximumLength(100).WithMessage("Apelido deve ter no máximo 100 caracteres.");

        RuleFor(x => x.KmAtual)
            .GreaterThanOrEqualTo(0).WithMessage("Quilometragem não pode ser negativa.");

        RuleFor(x => x.MediaKmMes)
            .GreaterThanOrEqualTo(1).WithMessage("Média de km/mês deve ser pelo menos 1.");
    }
}

public class EditarVeiculoRequestValidator : AbstractValidator<EditarVeiculoRequest>
{
    public EditarVeiculoRequestValidator()
    {
        RuleFor(x => x.Apelido)
            .NotEmpty().WithMessage("Apelido é obrigatório.")
            .MaximumLength(100).WithMessage("Apelido deve ter no máximo 100 caracteres.");

        RuleFor(x => x.MediaKmMes)
            .GreaterThanOrEqualTo(1).WithMessage("Média de km/mês deve ser pelo menos 1.");
    }
}
