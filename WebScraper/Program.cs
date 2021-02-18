using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebScraper.Configuration;
using WebScraper.Loggers;
using WebScraper.Scrapper;
using WebScraper.Scrapper.ConcreteScrappers;
using WebScraper.ScrapperLogger;

namespace WebScraper
{
    public class Program
    {
        public static async Task Main()
        {
            var serviceProvider = GetServiceProvider();

            var scrapper = serviceProvider
                .GetService<IScrapper>();

            var baseSettings = serviceProvider
                .GetService<IOptions<BaseSettings>>();

            var refreshIntervalTime = baseSettings?.Value.RefreshIntervalInMinutes ?? new BaseSettings().RefreshIntervalInMinutes;

            if (scrapper != null)
            {
                while (true)
                {
                    await scrapper.Scrape();
                    await Task.Delay(refreshIntervalTime * 60000);
                }
            }
        }

        private static IServiceProvider GetServiceProvider()
        {
            var configuration = GetConfiguration();
            
            var serviceProvider = new ServiceCollection()
                .AddSingleton(configuration)
                .AddLogging(o => o.AddProvider(new ConsoleLoggerProvider()))
                .AddSingleton<IConcreteScrapper, SkytechConcreteScrapper>()
                .AddSingleton<IConcreteScrapper, VarleConcreteScrapper>()
                .AddSingleton<IConcreteScrapper, KilobaitasConcreteScrapper>()
                .AddSingleton<IScrapper, Scrapper.Scrapper>()
                .AddSingleton<IProductLogger, ProductLogger>()
                .AddOptions()
                .Configure<AppConfig>(configuration)
                .Configure<BaseSettings>(configuration.GetSection(nameof(BaseSettings)))
                .Configure<ScrapePages>(configuration.GetSection(nameof(ScrapePages)))
                .BuildServiceProvider();

            return serviceProvider;
        }

        private static IConfigurationRoot GetConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            return configuration;
        }
    }
}