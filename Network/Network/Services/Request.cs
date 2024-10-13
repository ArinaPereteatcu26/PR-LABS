using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network.Services
{
    public class Request
    {
        public async Task<string> GetSiteContent(string siteName)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(siteName);

                    if (response.IsSuccessStatusCode)
                    {
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
                    Console.WriteLine($"Error: {ex.Message}");
                    return string.Empty;
                }
            }
        }
        private async Task SaveContent(string content, string filePath)
        {
            try
            {
                await File.WriteAllTextAsync(filePath, content);
                Console.WriteLine($"Content saved to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public async Task<string> GetSiteContentTcp(string siteName)
        {
            try 
            {
                Uri uri = new Uri(siteName);
                string host = uri.Host;
                string path = uri.PathAndQuery;

                using (TcpClient client = new TcpClient(host, 80))
                using (NetworkStream networkStream = client.GetStream())
                using (SslStream sslStream = new SslStream(networkStream, false,
                    new RemoteCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true), // Accept any certificate
                    null))

                {
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

                    return httpBody;
                }
            }

            catch (Exception ex)
            {
                // Handle any exceptions that occur during the request
                Console.WriteLine($"Error: {ex.Message}");
                return string.Empty;
            }
        }

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

    }
}


