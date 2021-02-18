using System.Collections.Generic;
using WebScraper.Models;

namespace WebScraper.ScrapperLogger
{
    public interface IProductLogger
    {
        void LogProducts(IEnumerable<Product> products);
    }
}