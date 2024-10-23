namespace Network.Models
{
    public class Product
    {
        public string? Url { get; set; }
        // public string? Image { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public string Year { get; set; }
        public string Link { get; set; }

        public Product()
        {
            Currency = string.Empty; // Initialize with a default value
            Year = string.Empty;     // Initialize with a default value
            Link = string.Empty;     // Initialize with a default value
        }
    }
}
