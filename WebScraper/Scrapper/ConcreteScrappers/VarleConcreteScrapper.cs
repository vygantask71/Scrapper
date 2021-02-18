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
    public class VarleConcreteScrapper : IConcreteScrapper
    {
        private readonly IOptions<AppConfig> _options;
        private readonly ILogger<VarleConcreteScrapper> _logger;

        public VarleConcreteScrapper(IOptions<AppConfig> options, ILogger<VarleConcreteScrapper> logger)
        {
            _options = options;
            _logger = logger;
        }
        
        public async Task<IEnumerable<Product>> GetProducts()
        {
            try
            {
                var page = await new HtmlWeb()
                    .LoadFromWebAsync(_options.Value.ScrapePages.VarleBaseUrl);

                var products = page
                    .DocumentNode
                    .SelectNodes("//div[contains(@class, 'grid-item product')]")
                    .Select(node => new Product
                    {
                        Name = node
                            .SelectSingleNode(".//a[@class='title']")
                            .Attributes["title"]
                            .Value,
                        Url = "varle.lt" + node
                            .SelectSingleNode(".//a[@class='title']")
                            .Attributes["href"]
                            .Value,
                        Price = decimal.Parse(node
                            .SelectSingleNode(".//span[@class='price']")
                            .InnerText
                            .ParseDigitsFromText()) / 100,
                        StockCount = 0
                    })
                    .ToList()
                    .Where(product => !product
                                          .Url
                                          .ToLower()
                                          .Contains("ivairios-prekes") &&
                                      (product
                                          .Name
                                          .ToLower()
                                          .Contains("rx") ||
                                      product
                                          .Name
                                          .ToLower()
                                          .Contains("rtx")))
                    .Where(product => _options
                        .Value
                        .ScrapeModels
                        .Any(product.Name.Contains))
                    .ToList();

                if (!_options.Value.BaseSettings.ShouldScrapeStock)
                {
                    return products;
                }

                foreach (var product in products)
                {
                    var productPage = await new HtmlWeb()
                        .LoadFromWebAsync("https://www." + product.Url);
                    
                    product.StockCount = int.Parse(productPage
                        .DocumentNode
                        .SelectSingleNode("//table[@class='icons-content']//td[@class='warehouse']")
                        .ParentNode
                        .SelectSingleNode(".//td[@class='text']/span")
                        .InnerText
                        .ParseDigitsFromText()
                    );
                }

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