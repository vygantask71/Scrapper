using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using WebScraper.Models;

namespace WebScraper.ScrapperLogger
{
    public class ProductLogger : IProductLogger
    {
        private readonly ILogger<ProductLogger> _logger;

        public ProductLogger(ILogger<ProductLogger> logger)
        {
            _logger = logger;
        }
        
        public void LogProducts(IEnumerable<Product> products)
        {
            var productsList = products
                .ToList();
            
            var radeonGpuList = productsList
                .Where(p => p
                    .Name
                    .ToLower()
                    .Contains("rx")
                )
                .ToList();
            
            var nvidiaGpuList = productsList
                .Where(p => p
                    .Name
                    .ToLower()
                    .Contains("rtx")
                )
                .ToList();
            
            var unknownGpuList = productsList
                .Where(p => !radeonGpuList.Contains(p) && !nvidiaGpuList.Contains(p))
                .ToList();

            if (productsList.Any())
            {
                _logger.LogInformation("Products found: {ProductsCount}", productsList.Count);
                _logger.LogInformation("Sorting by Price Low to High");

                productsList.Sort((x, y) => x.Price.CompareTo(y.Price));

                if (nvidiaGpuList.Any())
                {
                    PrintManufacturer("Nvidia");

                    for (var i = 3070; i < 3100; i += 10)
                    {
                        if (!nvidiaGpuList.Any(product => product.Name.Contains(i.ToString())))
                        {
                            continue;
                        }

                        PrintDivider(i);

                        foreach (var gpu in nvidiaGpuList.Where(product => product.Name.Contains(i.ToString())))
                        {
                            PrintGpu(gpu.ToString());
                        }
                    }
                }

                if (radeonGpuList.Any())
                {
                    PrintManufacturer("AMD Radeon");

                    for (var i = 6800; i < 7000; i += 100)
                    {
                        if (!radeonGpuList.Any(product => product.Name.Contains(i.ToString())))
                        {
                            continue;
                        }

                        PrintDivider(i);

                        foreach (var gpu in radeonGpuList.Where(product => product.Name.Contains(i.ToString())))
                        {
                            PrintGpu(gpu.ToString());
                        }
                    }
                }

                if (!unknownGpuList.Any())
                {
                    return;
                }
                
                PrintManufacturer("Unknown");
            
                foreach (var gpu in unknownGpuList)
                {
                    PrintGpu(gpu.ToString());
                }
            }
            else
            {
                _logger.LogInformation("Unfortunately, there is nothing in stock...");
            }
        }

        private void PrintDivider(int seriesNumber)
        {
            _logger.LogInformation("");
            _logger.LogInformation("{SeriesNumber}", seriesNumber);
            _logger.LogInformation("{Divider}", new string('-', 120));
        }

        private void PrintManufacturer(string manufacturerName)
        {
            _logger.LogInformation("");
            _logger.LogInformation("{Manufacturer}", manufacturerName);
            _logger.LogInformation("");
        }

        private void PrintGpu(string gpu)
        {
            _logger.LogInformation("{GpuName}", gpu);
            _logger.LogInformation("");
        }
    }
}