using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace Lab1.Services
{
    public class RequestSiteService
    {
        public async Task<string> GetSiteContent(string siteName)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Send an HTTP GET request to the specified URL
                    HttpResponseMessage response = await client.GetAsync(siteName);

                    // Check if the response was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the HTML content of the response
                        string htmlContent = await response.Content.ReadAsStringAsync();
                        return htmlContent;
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        return string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during the request
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return string.Empty;
                }
            }
        }

        public async Task<string> GetSiteContentTCP(string siteName)
        {
            try
            {
                // Extract host and resource path from the siteName (URL)
                Uri uri = new Uri(siteName);
                string host = uri.Host;
                string path = uri.PathAndQuery;

                // Connect to the server using a TCP socket
                using (TcpClient client = new TcpClient(host, 443)) // Using port 443 for HTTPS
                using (NetworkStream networkStream = client.GetStream())
                using (SslStream sslStream = new SslStream(networkStream, false,
                    new RemoteCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true), // Accept any certificate
                    null))
                {
                    // Authenticate the server certificate
                    await sslStream.AuthenticateAsClientAsync(host);

                    // Build and send the HTTP GET request
                    string httpRequest = $"GET {path} HTTP/1.1\r\n" +
                                         $"Host: {host}\r\n" +
                                         "Connection: close\r\n" + // Close the connection after the response
                                         "\r\n"; // End of headers

                    byte[] requestBytes = Encoding.ASCII.GetBytes(httpRequest);
                    await sslStream.WriteAsync(requestBytes, 0, requestBytes.Length);
                    await sslStream.FlushAsync();

                    // Read the response from the server
                    StringBuilder responseBuilder = new StringBuilder();
                    char[] buffer = new char[1024];
                    int bytesRead;

                    using (StreamReader reader = new StreamReader(sslStream, Encoding.UTF8))
                    {
                        while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            responseBuilder.Append(buffer, 0, bytesRead);
                        }
                    }

                    string fullResponse = responseBuilder.ToString();

                    // Extract the HTTP body (after the header)
                    string httpBody = ExtractHttpResponseBody(fullResponse);

                    // Now we can extract the title and price from the body
                    string extractedData = ExtractTitleAndPrice(httpBody);

                    return extractedData;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the request
                Console.WriteLine($"An error occurred: {ex.Message}");
                return string.Empty;
            }
        }

        // Method to extract the HTTP response body from the full response
        private string ExtractHttpResponseBody(string httpResponse)
        {
            // Find the end of the HTTP headers
            int headerEndIndex = httpResponse.IndexOf("\r\n\r\n");

            if (headerEndIndex != -1)
            {
                // The body starts right after the headers
                return httpResponse.Substring(headerEndIndex + 4);
            }
            else
            {
                return string.Empty;
            }
        }

        // Method to extract the product title and price
        private string ExtractTitleAndPrice(string html)
        {
            // Extract the product title from the "title" attribute of <a> or <img>
            var titleMatch = Regex.Match(html, "title=\"([^\"]+)\"");
            string productTitle = titleMatch.Success ? titleMatch.Groups[1].Value : "No title found";

            // Extract the product price from the "data-ga4" JSON-like attribute
            var priceMatch = Regex.Match(html, "\"price\":(\\d+)");
            string productPrice = priceMatch.Success ? priceMatch.Groups[1].Value : "No price found";

            // Return the extracted data in the desired format
            return $"Product: {productTitle}\nPrice: {productPrice}";
        }
    }
}

