using HtmlAgilityPack;
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
        public string ExtractLink(HtmlNode productNode)
        {
            var linkNode = productNode.SelectSingleNode(".//a[contains(@class, 'ga-item')]");
            string link = linkNode != null ? linkNode.GetAttributeValue("href", string.Empty) : string.Empty;
            return link;
        }

        public string ExtractImage(HtmlNode productNode)
        {
            var imageNode = productNode.SelectSingleNode(".//img[contains(@class, 'product-image')]");
            string imageUrl = imageNode != null ? imageNode.GetAttributeValue("src", string.Empty) : string.Empty;
            return imageUrl;
        }

        public string ExtractBrand(HtmlNode productNode)
        {
            var brandNode = productNode.SelectSingleNode(".//div[contains(@class, 'brand-name')]");
            string brand = brandNode != null ? brandNode.InnerText.Trim() : string.Empty;
            return brand;
        }

    }
}