using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebScraper.Configuration;
using WebScraper.Scrapper.ConcreteScrappers;
using WebScraper.ScrapperLogger;

namespace WebScraper.Scrapper
{
    public class Scrapper : IScrapper
    {
        private readonly IEnumerable<IConcreteScrapper> _scrappers;
        private readonly IProductLogger _productLogger;
        private readonly ILogger<Scrapper> _logger;
        private readonly IOptions<BaseSettings> _options;

        public Scrapper(IEnumerable<IConcreteScrapper> scrappers, IProductLogger productLogger, ILogger<Scrapper> logger, IOptions<BaseSettings> options)
        {
            _scrappers = scrappers;
            _productLogger = productLogger;
            _logger = logger;
            _options = options;
        }
        
        public async Task Scrape()
        {
            _logger.LogInformation("Last refresh time: {CurrentTime}", DateTime.Now);
            _logger.LogInformation("Next refresh after {RefreshInterval} minute(s)\n", _options.Value.RefreshIntervalInMinutes);

            if (!_options.Value.ShouldScrapeStock)
            {
                _logger.LogInformation("Stock scrape is currently disabled");
            }

            var scrapersResult = await Task.WhenAll(_scrappers
                .Select(s => s
                    .GetProducts()
                )
                .ToArray()
            );

            var products = scrapersResult
                .SelectMany(r => r);
            
            _productLogger.LogProducts(products);
        }
    }
}