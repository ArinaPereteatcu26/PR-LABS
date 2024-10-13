using HtmlAgilityPack;
using Network.Mappers;
using Network.Models;
using Network.Services;

var requestSite = new Request();
var htmlContent = await requestSite.GetSiteContent("https://darwin.md/laptopuri");

var storeInfoService = new StoreInfo();
List<Product>? products = storeInfoService.StoreProduct(htmlContent);

if (products != null && products.Count > 0)
{
    foreach (var product in products)
    {
        var htmlContentProducts = await requestSite.GetSiteContent(product.Link);
        var htmlDocProduct = new HtmlDocument();
        htmlDocProduct.LoadHtml(htmlContentProducts);
        storeInfoService.StoreAdditionalInfo(htmlDocProduct.DocumentNode, product);
    }
}

var serializationService = new SerializationLogic();
var json = serializationService.SerializeListToJson(products);
var xml = serializationService.SerializeListToXML(products);

var customSerializationService = new CustomSerialization();
var customSerialized = customSerializationService.SerializeList(products);

var priceMapper = new Mappers();
var productsInEuro = priceMapper.CurrencyConversion(products);
var filteredProducts = priceMapper.FilterProductsByPrice(productsInEuro, 100, 250);

var filteredProductsTotalPrice = storeInfoService.StoreProductsWithTotalPrice(filteredProducts, priceMapper.SumPrices(filteredProducts));
