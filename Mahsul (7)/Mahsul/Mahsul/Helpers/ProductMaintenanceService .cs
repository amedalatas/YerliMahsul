using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mahsul.Data;
using Microsoft.EntityFrameworkCore;

namespace Mahsul.Helpers
{
    public class ProductMaintenanceService : IHostedService, IDisposable
    {
        private readonly ILogger<ProductMaintenanceService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer _timer;

        public ProductMaintenanceService(ILogger<ProductMaintenanceService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Product maintenance service is starting.");

            // Şu an ile bugünün 00:00'ı arasındaki süreyi hesapla
            var now = DateTime.Now;
            var nextRun = now.Date.AddDays(1); // Bugünün 00:00'ı
            var initialDelay = nextRun - now;

            // Timer'ı ayarla
            _timer = new Timer(DoWork, null, initialDelay, TimeSpan.FromHours(24)); // Her 24 saatte bir çalıştır

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            _logger.LogInformation("Product maintenance service is working.");

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // StartDate kontrolü
                    var productsToPublish = await context.Product
                        .Where(p => p.StartDate.Date == DateTime.Today && !p.isActive)
                        .ToListAsync();

                    foreach (var product in productsToPublish)
                    {
                        product.isActive = true;
                    }

                    // EndDate kontrolü
                    var productsToUnpublish = await context.Product
                        .Where(p => p.EndDate.Date == DateTime.Today && p.isActive)
                        .ToListAsync();

                    foreach (var product in productsToUnpublish)
                    {
                        product.isActive = false;
                    }

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating product statuses.");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Product maintenance service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
