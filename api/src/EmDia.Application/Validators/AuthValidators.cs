using EmDia.Application.DTOs.Auth;
using FluentValidation;

namespace EmDia.Application.Validators;

public class RegistrarRequestValidator : AbstractValidator<RegistrarRequest>
{
    public RegistrarRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(150).WithMessage("E-mail deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter pelo menos 8 caracteres.")
            .MaximumLength(100).WithMessage("Senha deve ter no máximo 100 caracteres.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.");
    }
}

public class AlterarNomeRequestValidator : AbstractValidator<AlterarNomeRequest>
{
    public AlterarNomeRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");
    }
}
