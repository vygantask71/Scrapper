using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraper.Models;

namespace WebScraper.Scrapper.ConcreteScrappers
{
    public interface IConcreteScrapper
    {
        Task<IEnumerable<Product>> GetProducts();
    }
}