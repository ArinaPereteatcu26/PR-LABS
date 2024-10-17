using Network.Models;

namespace Network.Mappers
{
    public class Mappers
    {
        public List<Product> CurrencyConversion(List<Product> products)
        {
            //rate to euros
            decimal CurrencyConversion = 0.051m;

            //transfom each product in collection into a new form
            var productsInEuro = products.Select(product =>
            {
                product.Price = Math.Round(product.Price * CurrencyConversion, 2);
                return product;
            }).ToList(); //store to productsInEuro
            return productsInEuro;
        }

        public List<Product> FilterProductsByPrice(List<Product> products, decimal minPrice, decimal maxPrice)
        {
          //filter elements based on condition
            var filteredProducts = products.Where(product => product.Price >= minPrice && product.Price <= maxPrice).ToList();
            return filteredProducts;
        }

        public decimal SumPrices(List<Product> products)
        {
            decimal sum = products.Sum(product => product.Price);
            return sum;
        }
    } 
}