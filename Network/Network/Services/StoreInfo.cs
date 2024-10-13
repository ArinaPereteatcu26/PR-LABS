using System.Text.Json;
using Network.Models;
using HtmlAgilityPack;
using Network.Filter;


namespace Network.Services
{
    public class StoreInfo
    {
        private ExtractProduct _extractProduct;
        private List<Product> _products;
        public StoreInfo()
        {
            _extractProduct = new ExtractProduct();
            _products = new List<Product>();
        }

        public List<Product>? StoreProduct(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var productNodes = htmlDoc.DocumentNode.SelectNodes("//figure[contains(@class, 'card-product')]");
            if (productNodes != null)
            {
                foreach (var productNode in productNodes)
                {
                    var product = new Product
                    {
                        Name = _extractProduct.ExtractName(productNode),
                        Price = _extractProduct.ExtractPrice(productNode),
                        Link = _extractProduct.ExtractLink(productNode)
                    };
                    // Console.WriteLine($"Product: {product.Name}, Price: {product.Price}, Link: {product.Link}");
                    _products.Add(product);
                }
                return _products;
            }
            else
            {
                Console.WriteLine("No products found.");
                return null;
            }
        }
        public Product StoreAdditionalInfo(HtmlNode productNode, Product product)
        {
            // Check if the productNode is null
            if (productNode == null)
            {
                Console.WriteLine("Product node is null.");
                return product; // You can handle this case differently if needed
            }

            // Extract memory information
            product.Memory = _extractProduct.ExtractProductMemory(productNode);
            return product;
        }


        public string StoreAsJson(List<Product> products)
        {
            var json = JsonSerializer.Serialize(products);
            return json;
        }

        public List<FilteredProduct> StoreProductsWithTotalPrice(List<Product> products, decimal totalPrice)
        {
            var filteredProducts = new List<FilteredProduct>();
            foreach (var product in products)
            {
                var filteredProduct = new FilteredProduct
                {
                    Name = product.Name,
                    Price = product.Price,
                    Link = product.Link,
                    Resolution = product.Memory,
                    TotalPrice = totalPrice,
                    Date = DateTime.Now
                };
                filteredProducts.Add(filteredProduct);
            }
            return filteredProducts;
        }

    }
}