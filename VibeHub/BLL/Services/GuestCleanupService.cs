using DAL.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace BLL.Services;

public class GuestCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GuestCleanupService> _logger;

    public GuestCleanupService(IServiceProvider serviceProvider, ILogger<GuestCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupInactiveGuests(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // 1 minute for dev
        }
    }

    private async Task CleanupInactiveGuests(CancellationToken stoppingToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var guestRepository = scope.ServiceProvider.GetRequiredService<IGuestRepository>();

            var inactiveGuests = await guestRepository.GetInactiveGuests();

            foreach (var guest in inactiveGuests)
            {
                await guestRepository.Delete(guest);
                _logger.LogInformation($"Deleted inactive guest: {guest.Name} (ID: {guest.Id})");
            }
        }
    }
}