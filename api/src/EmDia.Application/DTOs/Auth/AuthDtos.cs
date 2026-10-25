namespace EmDia.Application.DTOs.Auth;

public record RegistrarRequest(
    string Nome,
    string Email,
    string Senha
);

public record LoginRequest(
    string Email,
    string Senha
);

public record LoginResponse(
    Guid Id,
    string Nome,
    string Email,
    string AccessToken,
    bool PrecisaCheckup
);

public record RefreshResponse(
    string AccessToken
);

public record AlterarNomeRequest(
    string Nome
);

public record UsuarioResponse(
    Guid Id,
    string Nome,
    string Email,
    DateTimeOffset CriadoEm,
    bool PrecisaCheckup
);
