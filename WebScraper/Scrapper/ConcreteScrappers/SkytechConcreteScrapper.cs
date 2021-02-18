using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebScraper.Configuration;
using WebScraper.Extensions;
using WebScraper.Models;

namespace WebScraper.Scrapper.ConcreteScrappers
{
    public class SkytechConcreteScrapper : IConcreteScrapper
    {
        private readonly IOptions<AppConfig> _options;
        private readonly ILogger<SkytechConcreteScrapper> _logger;

        public SkytechConcreteScrapper(IOptions<AppConfig> options, ILogger<SkytechConcreteScrapper> logger)
        {
            _options = options;
            _logger = logger;
        }
        
        public async Task<IEnumerable<Product>> GetProducts()
        {
            try
            {
                var page = await new HtmlWeb()
                    .LoadFromWebAsync(_options.Value.ScrapePages.SkytechBaseUrl);

                var products = page
                    .DocumentNode
                    .SelectNodes("//tr[contains(@class, 'productListing')]//a")
                    .Where(node =>
                        node
                            .ParentNode
                            .OuterHtml
                            .Contains("name")
                        &&
                        !node
                            .ParentNode
                            .ParentNode
                            .OuterHtml
                            .Contains("nostock"))
                    .Select(node => new Product
                    {
                        Name = node.InnerText,
                        Url = "skytech.lt/" + node
                            .Attributes["href"]
                            .Value,
                        Price = decimal.Parse(node
                            .ParentNode
                            .ParentNode
                            .SelectSingleNode(".//strong")
                            .InnerText
                            .ParseDigitsFromText()) / 100,
                        StockCount = int.Parse(node
                            .ParentNode
                            .ParentNode
                            .SelectSingleNode(".//td[contains(@class, 'kiekis')]")
                            .InnerText
                            .ParseDigitsFromText())
                    })
                    .Where(product => _options
                        .Value
                        .ScrapeModels
                        .Any(product.Name.Contains))
                    .ToList();

                return products;
            }
            catch (Exception exception)
            {
                _logger.LogCritical("Could not scrape Varle: {ExceptionMessage}", exception.Message);
            }

            return new List<Product>();
        }
    }
}