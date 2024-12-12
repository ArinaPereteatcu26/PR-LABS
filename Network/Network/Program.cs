using HtmlAgilityPack;
using Network.Mappers;
using Network.Models;
using Network.Services;
using RabbitMQ.Client;
using System.Text;
using System.Net;

public class FtpUploader
{
    public void UploadFile(string ftpUrl, string username, string password, string filePath)
    {
        try
        {
            ServicePointManager.Expect100Continue = false;
            string fileName = Path.GetFileName(filePath);
            string uploadUrl = $"{ftpUrl.TrimEnd('/')}/{fileName}";
            Console.WriteLine($"Uploading file to: {uploadUrl}");
            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential(username, password);
    
                client.UploadFile(uploadUrl, filePath);
    
                Console.WriteLine("Upload complete");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
    
}



public class RabbitMQSender
{
    private readonly string _hostName = "localhost";
    private readonly string _exchangeName = "logs";

    public async Task Send(string message)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange: "logs", type: ExchangeType.Fanout);

        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange: "logs", routingKey: string.Empty, body: body);
        Console.WriteLine($" [x] Sent {message}");
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var requestSite = new Request();
        var htmlContent = await requestSite.GetSiteContent("https://librarius.md/ro/books/category/literatura-artistica/gender/840");

        var storeInfoService = new StoreInfo();
        List<Product>? products = storeInfoService.StoreProduct(htmlContent);

        if (products != null && products.Count > 0)
        {
            foreach (var product in products)
            {
                var htmlContentProducts = await requestSite.GetSiteContent(product.Link);
                var htmlDocProduct = new HtmlDocument();
                htmlDocProduct.LoadHtml(htmlContentProducts);
                storeInfoService.StoreAdditionalInfo(htmlContentProducts, product);
            }
        }
        else
        {
            Console.WriteLine("No products found or product list is null.");
            return; // Exit if there are no products to process
        }

        var serializationService = new SerializationLogic();

        var json = serializationService.SerializeListToJson(products);
        var xml = serializationService.SerializeListToXML(products);

        // Show JSON and XML in the console
        Console.WriteLine("Initial Products JSON:");
        Console.WriteLine(json);
        Console.WriteLine("Initial Products XML:");
        Console.WriteLine(xml);

        File.WriteAllText("productsInicial.json", json);
        File.WriteAllText("productsInicial.xml", xml);

        // Instantiate Mappers correctly
        var priceMapper = new Mappers();

        if (products != null)
        {
            var productsInEuro = priceMapper.CurrencyConversion(products);
            var jsonEuro = serializationService.SerializeListToJson(productsInEuro);
            var xmlEuro = serializationService.SerializeListToXML(productsInEuro);

            Console.WriteLine("Products in Euro JSON:");
            Console.WriteLine(jsonEuro);
            Console.WriteLine("Products in Euro XML:");
            Console.WriteLine(xmlEuro);

            File.WriteAllText("productsInEuro.json", jsonEuro);
            File.WriteAllText("productsInEuro.xml", xmlEuro);

            var filteredProducts = priceMapper.FilterProductsByPrice(productsInEuro, 1, 9999);
            var filteredProductsTotalPrice =
                storeInfoService.StoreProductsWithTotalPrice(filteredProducts, priceMapper.SumPrices(filteredProducts));

            var jsonFilteredTotalPrice = serializationService.SerializeListToJson(filteredProductsTotalPrice);
            var xmlFilteredTotalPrice = serializationService.SerializeListToXML(filteredProductsTotalPrice);

            Console.WriteLine("Filtered Products with Total Price JSON:");
            Console.WriteLine(jsonFilteredTotalPrice);
            Console.WriteLine("Filtered Products with Total Price XML:");
            Console.WriteLine(xmlFilteredTotalPrice);

            File.WriteAllText("productsFilteredTotalPrice.json", jsonFilteredTotalPrice);
            File.WriteAllText("productsFilteredTotalPrice.xml", xmlFilteredTotalPrice);

            // Instantiate RabbitMQSender and send messages
            var rabbitMqSender = new RabbitMQSender();
            
            // Publish to RabbitMQ
            Console.WriteLine("Publishing initial product data to RabbitMQ...");
            await rabbitMqSender.Send(serializationService.SerializeToJson(products[0]));

            // Update FTP upload to use the filtered products JSON
            // var ftpPublisher = new FtpUploader();
            // ftpPublisher.UploadFile("ftp://localhost:21", "testuser", "testpass", "productsFilteredTotalPrice.json");
        }
    }
}
    

        //Console.WriteLine("Publishing products in Euro to RabbitMQ...");
        //await rabbitMqSender.Send(jsonEuro);

        //Console.WriteLine("Publishing filtered products with total price to RabbitMQ...");
        //await rabbitMqSender.Send(jsonFilteredTotalPrice);

        //Console.WriteLine("Messages published successfully!");
    

    // Additional logic to handle custom serialization
    //var customSerializationService = new CustomSerialization();

    //if (products != null)
    //{
    //    var customSerialized = customSerializationService.SerializeList(products);

    //    // Show custom serialized data in the console
    //    Console.WriteLine("Custom Serialized Data:");
    //    Console.WriteLine(customSerialized);

    //    File.WriteAllText("productsCustomSerialized.txt", customSerialized);

    //    List<Product> customDeserialized = customSerializationService.DeserializeList<Product>(customSerialized);

    //    // Show deserialized products in the console
    //    Console.WriteLine($"Deserialized {customDeserialized.Count} products:");
    //    foreach (var product in customDeserialized)
    //    {
    //        Console.WriteLine("Product");
    //        Console.WriteLine($"Name: {product.Name}");
    //        Console.WriteLine($"Price: {product.Price}");
    //        Console.WriteLine($"Link: {product.Link}");
    //        Console.WriteLine($"Year: {product.Year}\n");
    //    }
    //}
    //else
    //{
    //    Console.WriteLine("Product list is null, skipping custom serialization.");
    //}
