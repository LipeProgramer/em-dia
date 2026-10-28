namespace EmDia.Domain.Exceptions;

/// <summary>Códigos devolvidos no campo "code" do ProblemDetails (spec §3.5).</summary>
public static class CodigosErro
{
    public const string PassosPendentes = "PASSOS_PENDENTES";
    public const string ItemEncerrado = "ITEM_ENCERRADO";
    public const string KmMenor = "KM_MENOR";
    public const string VeiculoObrigatorio = "VEICULO_OBRIGATORIO";
    public const string SenhaIncorreta = "SENHA_INCORRETA";
    public const string VeiculoComItens = "VEICULO_COM_ITENS";
    public const string EmailDuplicado = "EMAIL_DUPLICADO";
    public const string ApelidoDuplicado = "APELIDO_DUPLICADO";
}
