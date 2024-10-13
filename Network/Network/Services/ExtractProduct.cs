using HtmlAgilityPack;
namespace Network.Services
{
    public class ExtractProduct
    {
        private Validation _validation;
        public ExtractProduct() 
        {
            _validation = new Validation();
        }
        public string ExtractName(HtmlNode productNode)
        {
            var nameNode = productNode.SelectSingleNode(".//div[contains(@class, 'grid-item')]//figcaption//a[contains(@class, 'ga-item')]");
            string name = nameNode != null ? nameNode.InnerText.Trim() : string.Empty;
            return name;
        }
        public decimal ExtractPrice(HtmlNode productNode)
        {
            var priceNode = productNode.SelectSingleNode(".//div[contains(@class, 'bottom-wrap')]//div[contains(@class, 'price-wrap')]//span[contains(@class, 'price-new')]/b");
            string price = priceNode != null ? priceNode.InnerText.Trim() : string.Empty;
            return _validation.ValidProduct(price.Trim());
        }
        public string ExtractProductMemory(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            var productNode = htmlDoc.DocumentNode.SelectSingleNode("\".//div[contains(@class, 'grid-item')]//figcaption//a[contains(@class, 'ga-item')]\"\r\n");
           
            if (productNode != null)
            {
                var features = productNode.SelectNodes(".//li");
                if (features != null)
                {
                    foreach (var feature in features)
                    {
                        if (feature.InnerText.Trim().Contains("Memorie"))
                        {
                            var memoryDescription = feature.InnerText.Trim();
                            return _validation.ValidMemory(memoryDescription.Split(":")[1]);
                        }
                    }
                }
            }
            return "Memory not found";
        }

    }
}
