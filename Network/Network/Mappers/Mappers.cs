using Network.Models;

namespace Network.Mappers
{
    public class Mappers
    {
        public List<Product> CurrencyConversion(List<Product> products)
        {
            decimal CurrencyConversionRate = 0.051m;

            var productsInEuro = products.Select(product =>
            {
                product.Price = Math.Round(product.Price * CurrencyConversionRate, 2);
                return product;
            }).ToList();

            return productsInEuro;
        }

        public List<Product> FilterProductsByPrice(List<Product> products, decimal minPrice, decimal maxPrice)
        {
            return products.Where(product => product.Price >= minPrice && product.Price <= maxPrice).ToList();
        }

        public decimal SumPrices(List<Product> products)
        {
            return products.Sum(product => product.Price);
        }
    }
}
