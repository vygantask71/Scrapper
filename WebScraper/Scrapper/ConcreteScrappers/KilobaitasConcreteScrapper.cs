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
    public class KilobaitasConcreteScrapper : IConcreteScrapper
    {
        private readonly IOptions<AppConfig> _options;
        private readonly ILogger<KilobaitasConcreteScrapper> _logger;

        public KilobaitasConcreteScrapper(IOptions<AppConfig> options, ILogger<KilobaitasConcreteScrapper> logger)
        {
            _options = options;
            _logger = logger;
        }
        
        public async Task<IEnumerable<Product>> GetProducts()
        {
            try
            {
                var mainPage = await new HtmlWeb()
                    .LoadFromWebAsync(_options.Value.ScrapePages.KilobaitasBaseUrl);

                var products = mainPage
                    .DocumentNode
                    .SelectNodes("//div[@class='itemNormal']")
                    .Select(node => new Product
                    {
                        Name = node
                            .SelectSingleNode(".//span/a")
                            .Attributes["title"]
                            .Value,
                        Url = "kilobaitas.lt" + node
                            .SelectSingleNode(".//span/a")
                            .Attributes["href"]
                            .Value,
                        Price = decimal.Parse(node
                            .SelectSingleNode(".//span/a")
                            .ParentNode
                            .ParentNode
                            .ParentNode
                            .ParentNode
                            .SelectSingleNode(".//span[@class='itemPriceTop']")
                            .ParentNode
                            .InnerText
                            .ParseDigitsFromText()) / 100,
                        StockCount = 0
                    })
                    .ToList()
                    .Where(product => product
                                          .Name
                                          .ToLower()
                                          .Contains("rx") ||
                                      product
                                          .Name
                                          .ToLower()
                                          .Contains("rtx"))
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
            
                    var htmlScriptText = productPage
                        .DocumentNode
                        .SelectSingleNode("//div[@class='deliveryMore']")
                        .ChildNodes[3]
                        .InnerHtml;
            
                    product.StockCount = int.Parse(htmlScriptText
                        .Substring(htmlScriptText.IndexOf(':'), 10)
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