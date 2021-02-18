using System.Collections.Generic;

namespace WebScraper.Configuration
{
    public class AppConfig
    {
        public BaseSettings BaseSettings { get; set; }

        public ScrapePages ScrapePages { get; set; }
        
        public ICollection<string> ScrapeModels { get; set; }
    }
}