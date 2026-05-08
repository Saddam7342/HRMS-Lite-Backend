using System.Threading.Channels;
using HRMS.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HRMS.Infrastructure.Services;

public interface IBackgroundJobService
{
    void Enqueue(Func<IServiceProvider, CancellationToken, Task> job);
}

public class BackgroundJobService : BackgroundService, IBackgroundJobService
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundJobService> _logger;

    public BackgroundJobService(IServiceProvider serviceProvider, ILogger<BackgroundJobService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _queue = Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, Task>>();
    }

    public void Enqueue(Func<IServiceProvider, CancellationToken, Task> job)
    {
        if (!_queue.Writer.TryWrite(job))
        {
            _logger.LogError("Failed to enqueue background job.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Job Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _queue.Reader.ReadAsync(stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                await job(scope.ServiceProvider, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing background job.");
            }
        }

        _logger.LogInformation("Background Job Service is stopping.");
    }
}
