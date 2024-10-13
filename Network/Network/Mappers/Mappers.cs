using Network.Filter;
using Network.Models;
using Network.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Mappers
{
    public class Mappers
    {
        public List<Product> CurrencyConversion(List<Product> products)
        {
            decimal CurrencyConversion = 0.051m;

            var productsInEuro = products.Select(product =>
            {
                product.Price = Math.Round(product.Price * CurrencyConversion, 2);
                return product;
            }).ToList();
            return productsInEuro;
        }

        public List<Product> FilterProductsByPrice(List<Product> products, decimal minPrice, decimal maxPrice)
        {
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