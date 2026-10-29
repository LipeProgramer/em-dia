using EmDia.Application.Interfaces;
using EmDia.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmDia.Infrastructure.Jobs;

/// <summary>
/// Worker em segundo plano que executa a rotina de avisos diária às 8h (America/Sao_Paulo) (spec §1.2, §4.6).
/// </summary>
public class AvisosWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AvisosWorker> _logger;
    private readonly IRelogio _relogio;

    private static readonly TimeZoneInfo FusoSP =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public AvisosWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<AvisosWorker> logger,
        IRelogio relogio)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _relogio = relogio;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AvisosWorker iniciado.");

        // Num servidor que "dorme" (plano gratuito), a API pode não estar no ar às 8h. Ao acordar depois das 8h,
        // a rotina do dia roda logo; os avisos já enviados ficam em `avisos`, então nada é duplicado.
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
            var agoraSp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, FusoSP);
            if (agoraSp.Hour >= 8)
                await ExecutarAgoraAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na rotina de avisos ao iniciar.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalcularProximaExecucaoAs8h();
            _logger.LogInformation("Próxima execução da rotina de avisos agendada para daqui a {Delay}", delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
                await ExecutarAgoraAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no loop de execução do AvisosWorker.");
                // Aguarda 1 minuto antes de tentar novamente para evitar loop rápido em falha
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        _logger.LogInformation("AvisosWorker finalizado.");
    }

    /// <summary>
    /// Executa a rotina imediatamente (usado pelo worker e pelos testes automatizados).
    /// </summary>
    public async Task<ResultadoRotinaAvisos> ExecutarAgoraAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Disparando execução imediata da rotina de avisos...");

        using var scope = _scopeFactory.CreateScope();
        var rotinaService = scope.ServiceProvider.GetRequiredService<RotinaAvisosService>();
        return await rotinaService.ExecutarRotinaAsync(ct);
    }

    private TimeSpan CalcularProximaExecucaoAs8h()
    {
        var agoraSp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, FusoSP);
        var proximaExecucao = agoraSp.Date.AddHours(8);

        // Se já passou das 8h hoje, agenda para amanhã às 8h
        if (agoraSp >= proximaExecucao)
        {
            proximaExecucao = proximaExecucao.AddDays(1);
        }

        var delay = proximaExecucao - agoraSp;
        return delay > TimeSpan.Zero ? delay : TimeSpan.FromHours(24);
    }
}
