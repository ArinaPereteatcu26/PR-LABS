﻿using HtmlAgilityPack;
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
        storeInfoService.StoreAdditionalInfo(htmlContentProducts, product);
    }
}

var serializationService = new SerializationLogic();
var json = serializationService.SerializeListToJson(products);
var xml = serializationService.SerializeListToXML(products);

File.WriteAllText("productsInicial.json", json);
File.WriteAllText("productsInicial.xml", xml);

var priceMapper = new Mappers();
var productsInEuro = priceMapper.CurrencyConversion(products);
var jsonEuro = serializationService.SerializeListToJson(productsInEuro);
var xmlEuro = serializationService.SerializeListToXML(productsInEuro);

File.WriteAllText("productsInEuro.json", jsonEuro);
File.WriteAllText("productsInEuro.xml", xmlEuro);


var filteredProducts = priceMapper.FilterProductsByPrice(productsInEuro, 100, 250);
var filteredProductsTotalPrice = storeInfoService.StoreProductsWithTotalPrice(filteredProducts, priceMapper.SumPrices(filteredProducts));

var jsonFilteredTotalPrice = serializationService.SerializeListToJson(filteredProductsTotalPrice);
var xmlFilteredTotalPrice = serializationService.SerializeListToXML(filteredProductsTotalPrice);

File.WriteAllText("productsFilteredTotalPrice.json", jsonFilteredTotalPrice);
File.WriteAllText("productsFilteredTotalPrice.xml", xmlFilteredTotalPrice);

var customSerializationService = new CustomSerialization();
var customSerialized = customSerializationService.SerializeList(products);
File.WriteAllText("productsCustomSerialized.txt", customSerialized);

List<Product> customDeserialized = customSerializationService.DeserializeList<Product>(customSerialized);
Console.WriteLine(customDeserialized.Count);
foreach (var product in customDeserialized)
{
    Console.WriteLine("Product");
    Console.WriteLine(product.Name);
    Console.WriteLine(product.Price);
    Console.WriteLine(product.Link);
    Console.WriteLine(product.VideoCardType + "\n");
}