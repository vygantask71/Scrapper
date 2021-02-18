namespace WebScraper.Configuration
{
    public class BaseSettings
    {
        public bool ShouldScrapeStock { get; set; }

        public int RefreshIntervalInMinutes { get; set; } = 10;
    }
}