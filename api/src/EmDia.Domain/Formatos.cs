using System.Globalization;

namespace EmDia.Domain;

/// <summary>Formatação em pt-BR usada nos textos devolvidos ao usuário.</summary>
public static class Formatos
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string Km(int km) => km.ToString("N0", PtBr);

    public static string Data(DateOnly data) => data.ToString("dd/MM/yyyy", PtBr);
}
