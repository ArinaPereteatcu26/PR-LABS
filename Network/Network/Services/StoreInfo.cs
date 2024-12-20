﻿using System.Text.Json;
using Network.Models;
using HtmlAgilityPack;
using Network.Filter;
using Network.Services;


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
            var htmlDoc = new HtmlDocument();//mainpulate nodes
            htmlDoc.LoadHtml(htmlContent);

            //see what represents products
            var productNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'book-list-v3')]//div[contains(@class, 'anyproduct-card')]");
            if (productNodes != null)
            {
                foreach (var productNode in productNodes)
                {
                    var product = new Product
                    {
                        Name = _extractProduct.ExtractName(productNode),
                        Price = _extractProduct.ExtractPrice(productNode),
                        Link = _extractProduct.ExtractLink(productNode),

                    };
                    // Console.WriteLine($"Product: {product.Name}, Price: {product.Price}, Link: {product.Link}");
                    _products.Add(product);//add created product in products list
                }
                return _products;
            }
            else
            {
                Console.WriteLine("No products found.");
                return null;
            }
        }
        public Product StoreAdditionalInfo(string htmlContent, Product product)
        {
            product.Year = _extractProduct.ExtractYear(htmlContent);

            return product;
        }

        //return json string
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
                    Year = product.Year,
                    TotalPrice = totalPrice,
                    Date = DateTime.Now
                };
                filteredProducts.Add(filteredProduct);
            }
            return filteredProducts;
        }

    }
}
