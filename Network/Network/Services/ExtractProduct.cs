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

        public string ExtractProductMemory(HtmlNode productNode)
        {
            var ga4Data = productNode.GetAttributeValue("data-ga4", string.Empty);
            if (!string.IsNullOrEmpty(ga4Data))
            {
                // Extract JSON part
                var jsonStartIndex = ga4Data.IndexOf("{");
                if (jsonStartIndex >= 0)
                {
                    var json = ga4Data.Substring(jsonStartIndex);
                    try
                    {
                        JsonNode jsonNode = JsonNode.Parse(json);

                        // Check if the JSON structure contains "items"
                        if (jsonNode != null && jsonNode["ecommerce"]?["items"] is JsonArray items)
                        {
                            // Ensure items is not empty
                            if (items.Count > 0)
                            {
                                // Access the first item
                                var item = items[0];
                                // Check if the item contains "item_variant"
                                if (item["item_variant"] != null)
                                {
                                    string itemVariant = item["item_variant"].ToString();
                                    // Split the variant string to get the memory
                                    string[] variants = itemVariant.Split("|");

                                    // Check if the second part (index 1) contains memory info
                                    if (variants.Length > 1)
                                    {
                                        return variants[1].Trim(); // This should be "32 GB"
                                    }
                                }
                            }
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        Console.WriteLine($"JSON Parsing Error: {jsonEx.Message}");
                        return "Memory not found";
                    }
                }
            }
            return "Memory not found";
        }
    }
}