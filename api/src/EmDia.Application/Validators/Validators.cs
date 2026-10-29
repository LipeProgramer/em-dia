using EmDia.Application.DTOs.Auth;
using EmDia.Application.DTOs.Checkup;
using EmDia.Application.DTOs.Itens;
using EmDia.Application.DTOs.Usuarios;
using EmDia.Application.DTOs.Veiculos;
using EmDia.Application.Interfaces;
using EmDia.Domain.Entities;
using EmDia.Domain.Enums;
using FluentValidation;

namespace EmDia.Application.Validators;

// Regras de formato dos DTOs (spec §3.4 "Validações" e §6.5 "Mensagens esperadas").
// Regras que dependem de outros registros ficam no Domain e nos services.

internal static class Mensagens
{
    public const string Email = "Digite um e-mail válido.";
    public const string Senha = "Use pelo menos 8 caracteres, com letras e números.";
    public const string DataFutura = "A data não pode estar no futuro.";
    public const string Antecedencia = "Escolha entre 0 e 365 dias.";
    public const string KmNegativa = "A quilometragem deve ser maior ou igual a 0.";
    public const string Media = "Informe uma média entre 1 e 20.000 km por mês.";
}

public class RegistrarRequestValidator : AbstractValidator<RegistrarRequest>
{
    public RegistrarRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Informe seu nome.")
            .Must(n => n is null || n.Trim().Length is >= 2 and <= 100)
            .WithMessage("O nome deve ter entre 2 e 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(Mensagens.Email)
            .EmailAddress().WithMessage(Mensagens.Email)
            .MaximumLength(150).WithMessage("O e-mail deve ter até 150 caracteres.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage(Mensagens.Senha)
            .Length(8, 64).WithMessage(Mensagens.Senha)
            .Must(s => s is not null && s.Any(char.IsLetter) && s.Any(char.IsDigit)).WithMessage(Mensagens.Senha);
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Informe o e-mail.");
        RuleFor(x => x.Senha).NotEmpty().WithMessage("Informe a senha.");
    }
}

public class EsqueciSenhaRequestValidator : AbstractValidator<EsqueciSenhaRequest>
{
    public EsqueciSenhaRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage(Mensagens.Email).EmailAddress().WithMessage(Mensagens.Email);
    }
}

public class RedefinirSenhaRequestValidator : AbstractValidator<RedefinirSenhaRequest>
{
    public RedefinirSenhaRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("Abra o link que enviamos por e-mail.");
        RuleFor(x => x.NovaSenha)
            .NotEmpty().WithMessage(Mensagens.Senha)
            .Length(8, 64).WithMessage(Mensagens.Senha)
            .Must(s => s is not null && s.Any(char.IsLetter) && s.Any(char.IsDigit)).WithMessage(Mensagens.Senha);
    }
}

public class PreferenciasValidator : AbstractValidator<PreferenciasDto>
{
    public PreferenciasValidator()
    {
        RuleFor(x => x.MaxReavisos).InclusiveBetween((short)0, (short)5).WithMessage("Escolha entre 0 e 5 reavisos.");
        RuleFor(x => x.IntervaloReavisoDias).InclusiveBetween((short)1, (short)7).WithMessage("Escolha entre 1 e 7 dias.");
    }
}

public class ExcluirContaRequestValidator : AbstractValidator<ExcluirContaRequest>
{
    public ExcluirContaRequestValidator()
    {
        RuleFor(x => x.Senha).NotEmpty().WithMessage("Digite sua senha para confirmar.");
    }
}

public class CheckupRequestValidator : AbstractValidator<CheckupRequest>
{
    public CheckupRequestValidator(IRelogio relogio)
    {
        RuleFor(x => x.Respostas).NotNull().WithMessage("Envie as respostas do check-up.");
        RuleForEach(x => x.Respostas).ChildRules(r =>
            r.RuleFor(x => x.Codigo).NotEmpty().WithMessage("Informe o código da pergunta."));

        RuleForEach(x => x.Veiculos).ChildRules(v =>
        {
            v.RuleFor(x => x.Ref).NotEmpty().WithMessage("Informe uma referência para o veículo.");
            v.RuleFor(x => x.Apelido).NotEmpty().MaximumLength(50).WithMessage("O apelido deve ter de 1 a 50 caracteres.");
            v.RuleFor(x => x.Tipo).IsInEnum().WithMessage("Escolha Carro ou Moto.");
            v.RuleFor(x => x.KmAtual).GreaterThanOrEqualTo(0).WithMessage(Mensagens.KmNegativa);
            v.RuleFor(x => x.MediaKmMes).InclusiveBetween(1, 20_000).WithMessage(Mensagens.Media);
        });

        RuleForEach(x => x.Itens).ChildRules(i =>
        {
            i.RuleFor(x => x.ModeloId).NotEmpty().WithMessage("Escolha o modelo do item.");
            i.RuleFor(x => x.DataBase).Must(d => d is null || d <= relogio.Hoje).WithMessage(Mensagens.DataFutura);
            i.RuleFor(x => x.KmBase).GreaterThanOrEqualTo(0).When(x => x.KmBase.HasValue).WithMessage(Mensagens.KmNegativa);
        });
    }
}

public class SalvarItemRequestValidator : AbstractValidator<SalvarItemRequest>
{
    public SalvarItemRequestValidator(IRelogio relogio)
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Informe o nome do item.")
            .MaximumLength(80).WithMessage("O nome deve ter até 80 caracteres.");
        RuleFor(x => x.CategoriaId).InclusiveBetween((short)1, (short)6).WithMessage("Escolha uma categoria válida.");
        RuleFor(x => x.TipoVencimento).IsInEnum().WithMessage("Escolha o tipo de vencimento.");
        RuleFor(x => x.AntecedenciaDias).InclusiveBetween((short)0, (short)365).WithMessage(Mensagens.Antecedencia);
        RuleFor(x => x.Observacao).MaximumLength(500).WithMessage("A observação deve ter até 500 caracteres.");

        RuleFor(x => x.Passos)
            .Must(p => p is null || p.Count <= Item.MaxPassos).WithMessage("Use até 10 passos.");
        RuleForEach(x => x.Passos)
            .NotEmpty().WithMessage("O passo não pode ficar vazio.")
            .MaximumLength(Item.MaxDescricaoPasso).WithMessage("Cada passo deve ter até 120 caracteres.");

        RuleFor(x => x.DataBase).Must(d => d is null || d <= relogio.Hoje).WithMessage(Mensagens.DataFutura);
        RuleFor(x => x.IntervaloDias).GreaterThan(0).When(x => x.IntervaloDias.HasValue).WithMessage("O intervalo deve ser maior que 0.");
        RuleFor(x => x.IntervaloKm).GreaterThan(0).When(x => x.IntervaloKm.HasValue).WithMessage("O intervalo deve ser maior que 0.");
        RuleFor(x => x.KmBase).GreaterThanOrEqualTo(0).When(x => x.KmBase.HasValue).WithMessage(Mensagens.KmNegativa);
        RuleFor(x => x.GarantiaFabricanteMeses).GreaterThan((short)0).When(x => x.GarantiaFabricanteMeses.HasValue)
            .WithMessage("A garantia deve ser maior que 0.");

        // Campos exigidos por tipo de vencimento
        When(x => x.TipoVencimento == TipoVencimento.DataUnica, () =>
            RuleFor(x => x.DataVencimento).NotNull().WithMessage("Informe a data de vencimento."));
        When(x => x.TipoVencimento == TipoVencimento.RecorrentePorTempo, () =>
        {
            RuleFor(x => x.DataBase).NotNull().WithMessage("Informe a data da última vez.");
            RuleFor(x => x.IntervaloDias).NotNull().WithMessage("Informe o intervalo em dias.");
        });
        When(x => x.TipoVencimento == TipoVencimento.UsoOuTempo, () =>
        {
            RuleFor(x => x.VeiculoId).NotNull().WithMessage("Escolha o veículo.");
            RuleFor(x => x.DataBase).NotNull().WithMessage("Informe a data da última vez.");
            RuleFor(x => x.KmBase).NotNull().WithMessage("Informe a km da última vez.");
            RuleFor(x => x.IntervaloKm).NotNull().WithMessage("Informe o intervalo em km.");
            RuleFor(x => x.IntervaloDias).NotNull().WithMessage("Informe o intervalo em dias.");
        });
        When(x => x.TipoVencimento == TipoVencimento.Calculado, () =>
        {
            RuleFor(x => x.DataBase).NotNull().WithMessage("Informe a data da compra.");
            RuleFor(x => x.GarantiaFabricanteMeses).NotNull().WithMessage("Informe a garantia do fabricante em meses.");
        });
    }
}

public class ConcluirRequestValidator : AbstractValidator<ConcluirRequest>
{
    public ConcluirRequestValidator(IRelogio relogio)
    {
        RuleFor(x => x.DataConclusao)
            .NotNull().WithMessage("Informe a data da conclusão.")
            .Must(d => d is null || d <= relogio.Hoje).WithMessage(Mensagens.DataFutura);
        RuleFor(x => x.Km).GreaterThanOrEqualTo(0).When(x => x.Km.HasValue).WithMessage(Mensagens.KmNegativa);
        RuleFor(x => x.ValorGasto).GreaterThanOrEqualTo(0).When(x => x.ValorGasto.HasValue)
            .WithMessage("O valor deve ser maior ou igual a 0.");
        RuleFor(x => x.Observacao).MaximumLength(500).WithMessage("A observação deve ter até 500 caracteres.");
        RuleFor(x => x.NovaDataVencimento).Must(d => d is null || d > relogio.Hoje)
            .WithMessage("A nova validade precisa ser uma data futura.");
    }
}

public class MarcarPassoRequestValidator : AbstractValidator<MarcarPassoRequest>
{
    public MarcarPassoRequestValidator()
    {
        RuleFor(x => x.Concluido).NotNull().WithMessage("Informe se o passo está concluído.");
    }
}

public class CriarVeiculoRequestValidator : AbstractValidator<CriarVeiculoRequest>
{
    public CriarVeiculoRequestValidator()
    {
        RuleFor(x => x.Apelido).NotEmpty().MaximumLength(50).WithMessage("O apelido deve ter de 1 a 50 caracteres.");
        RuleFor(x => x.Tipo).IsInEnum().WithMessage("Escolha Carro ou Moto.");
        RuleFor(x => x.KmAtual).GreaterThanOrEqualTo(0).WithMessage(Mensagens.KmNegativa);
        RuleFor(x => x.MediaKmMes).InclusiveBetween(1, 20_000).WithMessage(Mensagens.Media);
    }
}

public class EditarVeiculoRequestValidator : AbstractValidator<EditarVeiculoRequest>
{
    public EditarVeiculoRequestValidator()
    {
        RuleFor(x => x.Apelido).NotEmpty().MaximumLength(50).WithMessage("O apelido deve ter de 1 a 50 caracteres.");
        RuleFor(x => x.Tipo).IsInEnum().WithMessage("Escolha Carro ou Moto.");
        RuleFor(x => x.MediaKmMes).InclusiveBetween(1, 20_000).WithMessage(Mensagens.Media);
    }
}

public class AtualizarKmRequestValidator : AbstractValidator<AtualizarKmRequest>
{
    public AtualizarKmRequestValidator()
    {
        RuleFor(x => x.KmAtual).GreaterThanOrEqualTo(0).WithMessage(Mensagens.KmNegativa);
    }
}

/// <summary>
/// Inscrição de notificações: o endereço precisa ser HTTPS de um serviço de push conhecido (Google, Mozilla,
/// Microsoft ou Apple). Assim a API nunca envia requisições para endereços escolhidos pelo usuário (SSRF).
/// </summary>
public class InscricaoPushRequestValidator : AbstractValidator<DTOs.Notificacoes.InscricaoPushRequest>
{
    private static readonly string[] ServicosPush =
        ["fcm.googleapis.com", "push.services.mozilla.com", "notify.windows.com", "push.apple.com"];

    public InscricaoPushRequestValidator()
    {
        RuleFor(x => x.Endpoint).NotEmpty().MaximumLength(1000)
            .Must(EnderecoDeServicoPush).WithMessage("Endereço de notificação inválido.");
        RuleFor(x => x.P256dh).NotEmpty().MaximumLength(200).WithMessage("Chave de notificação inválida.");
        RuleFor(x => x.Auth).NotEmpty().MaximumLength(100).WithMessage("Chave de notificação inválida.");
    }

    public static bool EnderecoDeServicoPush(string? endpoint) =>
        Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && ServicosPush.Any(s => uri.Host == s || uri.Host.EndsWith("." + s, StringComparison.OrdinalIgnoreCase));
}
