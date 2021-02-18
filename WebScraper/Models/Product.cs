namespace WebScraper.Models
{
    public class Product
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public decimal Price { get; set; }
        public int StockCount { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}\nUrl: {Url}\nStock: {StockCount}\nPrice: {Price:0.00}€";
        }
    }
}