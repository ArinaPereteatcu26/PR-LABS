namespace Network.Filter
{
    public class FilteredProduct
    {
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public decimal Currency { get; set; }
        public string? Link { get; set; }
        public string? Year { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime Date { get; set; }
        public object FilteredProducts { get; internal set; }

        public FilteredProduct()
        {
            // Initialize FilteredProducts to a default value (e.g., an empty collection)
            FilteredProducts = new List<object>(); // Replace List<object> with the appropriate type if known
        }
    }
}
