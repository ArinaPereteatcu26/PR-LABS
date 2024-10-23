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
            // XPath query to find name, searching for <div class="card-title"> inside the productNode
            var nameNode = productNode.SelectSingleNode(".//div[@class='card-title']");
            return nameNode != null ? nameNode.InnerText.Trim() : string.Empty;
        }

        public decimal ExtractPrice(HtmlNode productNode)
        {
            // XPath query to find the price, searching for <div class="card-price">
            var priceNode = productNode.SelectSingleNode(".//div[@class='card-price']");
            if (priceNode != null)
            {
                // Extract the text, remove " lei", commas, spaces, and other unwanted characters
                var priceText = priceNode.InnerText.Trim();

                // Remove " lei" and any non-numeric characters (except decimal point)
                priceText = priceText.Replace(" lei", "").Trim();

                // Replace commas (if present in numbers like "1,234") with empty string
                priceText = priceText.Replace(",", "").Trim();

                // Now try to parse the cleaned-up price text
                if (decimal.TryParse(priceText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var price))
                {
                    return price;
                }
                else
                {
                    Console.WriteLine($"Failed to parse price: {priceText}");
                    return 0; // Return 0 if parsing fails
                }
            }

            return 0; // Return 0 if the priceNode is not found
        }


        public string ExtractLink(HtmlNode productNode)
        {
            var linkNode = productNode.SelectSingleNode(".//a[@href]");
            return linkNode != null ? linkNode.GetAttributeValue("href", string.Empty) : string.Empty;
        }

        public string ExtractYear(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            // XPath query to find the year directly within the relevant section
            var yearNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(text(), 'Anul')]/following-sibling::div[@class='book-prop-value']");

            if (yearNode != null)
            {
                string year = yearNode.InnerText.Trim();
                return _validation.ValidYear(year);
            }
            return "Year not found";
        }
    }
}
