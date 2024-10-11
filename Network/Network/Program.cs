using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

class Program
{
    static async Task Main(string[] args)
    {
        using (HttpClient client = new HttpClient())
        {
            // Set a User-Agent header to mimic a browser
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            try
            {
                // Send the GET request
                HttpResponseMessage response = await client.GetAsync("https://enter.online/laptopuri");

                // Output the status code
                Console.WriteLine($"Status Code: {response.StatusCode}");

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Request was successful.");

                    // Read the response content as a string
                    string html = await response.Content.ReadAsStringAsync();

                    // Load the HTML document
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(html);

                    // Extract product elements (adjust XPath as necessary)
                    var products = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'product')]");

                    if (products != null)
                    {
                        Console.WriteLine("Found products:");
                        foreach (var product in products)
                        {
                            // Adjust these XPaths based on actual HTML structure
                            var nameNode = product.SelectSingleNode(".//h2[contains(@class, 'product-name')]"); // Product name
                            var priceNode = product.SelectSingleNode(".//span[contains(@class, 'price')]"); // Price
                            var imageNode = product.SelectSingleNode(".//img[contains(@class, 'product-image')]"); // Image
                            var linkNode = product.SelectSingleNode(".//a[contains(@class, 'product-link')]"); // Product link

                            // Extract and trim text values
                            var nameText = nameNode?.InnerText.Trim();
                            var priceText = priceNode?.InnerText.Trim();
                            var imageUrl = imageNode?.GetAttributeValue("src", "No image found");
                            var productLink = linkNode?.GetAttributeValue("href", "No link found");

                            // Validate product data before storing
                            if (IsValidProduct(nameText, priceText))
                            {
                                // Print product details
                                Console.WriteLine($"Product Name: {nameText}, Price: {priceText}, Image URL: {imageUrl}, Product Link: {productLink}");

                                // Extract additional data from the product link
                                await ExtractAdditionalData(client, productLink);
                            }
                            else
                            {
                                Console.WriteLine("Invalid product data. Skipping.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No products found.");
                    }
                }
                else
                {
                    Console.WriteLine("Request failed.");
                }
            }
            catch (HttpRequestException e)
            {
                // Handle exceptions
                Console.WriteLine($"Request error: {e.Message}");
            }
        }
    }

    static async Task ExtractAdditionalData(HttpClient client, string productLink)
    {
        try
        {
            // Send the GET request to the product link
            HttpResponseMessage productResponse = await client.GetAsync(productLink);

            if (productResponse.IsSuccessStatusCode)
            {
                // Read the response content as a string
                string productHtml = await productResponse.Content.ReadAsStringAsync();

                // Load the HTML document
                var productDoc = new HtmlDocument();
                productDoc.LoadHtml(productHtml);

                // Extract additional data (e.g., product description)
                var descriptionNode = productDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'product-description')]"); // Adjust XPath as necessary
                var descriptionText = descriptionNode?.InnerText.Trim() ?? "No description found";

                // Print additional product details
                Console.WriteLine($"Product Description: {descriptionText}");
            }
            else
            {
                Console.WriteLine("Failed to fetch product details.");
            }
        }
        catch (HttpRequestException e)
        {
            // Handle exceptions
            Console.WriteLine($"Error fetching product details: {e.Message}");
        }
    }

    static bool IsValidProduct(string name, string price)
    {
        // Check for empty values
        if (string.IsNullOrEmpty(name))
        {
            Console.WriteLine("Product name is empty.");
            return false;
        }

        // Check if price is a valid number and greater than zero
        if (!decimal.TryParse(price, out decimal parsedPrice) || parsedPrice <= 0)
        {
            Console.WriteLine("Invalid price value.");
            return false;
        }

        return true;
    }
}
