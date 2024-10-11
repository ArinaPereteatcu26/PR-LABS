using System;
using System.Net.Http;
using System.Threading.Tasks;

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

                // Optionally, check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Request was successful.");
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
}
