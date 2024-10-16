﻿using HtmlAgilityPack;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace Network.Services
{
    public class ExtractProduct
    {
        private Validation _validation;
        public ExtractProduct()
        {
            _validation = new Validation();
        }
        public string ExtractName(HtmlNode productNode)//takes htmlNode which represents a product
        {
            //xPath query to find name, search for <a> inside div 
            var nameNode = productNode.SelectSingleNode(".//div[contains(@class, 'grid-item')]//figcaption//a[contains(@class, 'ga-item')]");
            string name = nameNode != null ? nameNode.InnerText.Trim() : string.Empty;
            return name;
        }
        //looks for <b> element
        public decimal ExtractPrice(HtmlNode productNode)
        {
            var priceNode = productNode.SelectSingleNode(".//div[contains(@class, 'bottom-wrap')]//div[contains(@class, 'price-wrap')]//span[contains(@class, 'price-new')]/b");
            string price = priceNode != null ? priceNode.InnerText.Trim() : string.Empty;
            return _validation.ValidProduct(price.Trim());
        }
        //use get attribute to get value of href attribute
        public string ExtractLink(HtmlNode productNode)
        {
            var linkNode = productNode.SelectSingleNode(".//a[contains(@class, 'ga-item')]");
            string link = linkNode != null ? linkNode.GetAttributeValue("href", string.Empty) : string.Empty;
            return link;
        }



        public string ExtractVideoCardType(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            var productNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'main-description')]//ul[contains(@class, 'features')]");
            if (productNode != null)
            {
                var features = productNode.SelectNodes(".//li");
                if (features != null)
                {
                    foreach (var feature in features)
                    {
                        if (feature.InnerText.Trim().Contains("Tip placă video"))
                        {
                            var videoCardDescription = feature.InnerText.Trim();
                            return _validation.ValidVideoCardType(videoCardDescription.Split(":")[1]);
                        }
                    }
                }
            }
            return "Video Card Type not found";
            
        }
    }
}