using System.Threading.Tasks;

namespace WebScraper.Scrapper
{
    public interface IScrapper
    {
        Task Scrape();
    }
}