using HtmlAgilityPack;
using Network.Mappers;
using Network.Models;
using Network.Services;

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

// Check for null before conversion
if (products != null)
{
    var productsInEuro = priceMapper.CurrencyConversion(products); // Safe to call now
    var jsonEuro = serializationService.SerializeListToJson(productsInEuro);
    var xmlEuro = serializationService.SerializeListToXML(productsInEuro);

    // Show converted JSON and XML in the console
    Console.WriteLine("Products in Euro JSON:");
    Console.WriteLine(jsonEuro);
    Console.WriteLine("Products in Euro XML:");
    Console.WriteLine(xmlEuro);

    File.WriteAllText("productsInEuro.json", jsonEuro);
    File.WriteAllText("productsInEuro.xml", xmlEuro);

    var filteredProducts = priceMapper.FilterProductsByPrice(productsInEuro, 100, 250);
    var filteredProductsTotalPrice = storeInfoService.StoreProductsWithTotalPrice(filteredProducts, priceMapper.SumPrices(filteredProducts));

    var jsonFilteredTotalPrice = serializationService.SerializeListToJson(filteredProductsTotalPrice);
    var xmlFilteredTotalPrice = serializationService.SerializeListToXML(filteredProductsTotalPrice);

    // Show filtered JSON and XML in the console
    Console.WriteLine("Filtered Products with Total Price JSON:");
    Console.WriteLine(jsonFilteredTotalPrice);
    Console.WriteLine("Filtered Products with Total Price XML:");
    Console.WriteLine(xmlFilteredTotalPrice);

    File.WriteAllText("productsFilteredTotalPrice.json", jsonFilteredTotalPrice);
    File.WriteAllText("productsFilteredTotalPrice.xml", xmlFilteredTotalPrice);
}

var customSerializationService = new CustomSerialization();

// Check if products is not null before serialization
if (products != null)
{
    var customSerialized = customSerializationService.SerializeList(products);

    // Show custom serialized data in the console
    Console.WriteLine("Custom Serialized Data:");
    Console.WriteLine(customSerialized);

    File.WriteAllText("productsCustomSerialized.txt", customSerialized);

    List<Product> customDeserialized = customSerializationService.DeserializeList<Product>(customSerialized);

    // Show deserialized products in the console
    Console.WriteLine($"Deserialized {customDeserialized.Count} products:");
    foreach (var product in customDeserialized)
    {
        Console.WriteLine("Product");
        Console.WriteLine($"Name: {product.Name}");
        Console.WriteLine($"Price: {product.Price}");
        Console.WriteLine($"Link: {product.Link}");
        Console.WriteLine($"Year: {product.Year}\n");
    }
}
else
{
    Console.WriteLine("Product list is null, skipping custom serialization.");
}
