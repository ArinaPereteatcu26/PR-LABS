////using RabbitMQ.Client;
////using System;
////using System.Text;
////using System.Threading.Tasks;

////class EmitLog
////{
////    static async Task Main(string[] args)
////    {
////        var factory = new ConnectionFactory { HostName = "localhost" };

////        // Create connection and channel
////        using var connection = await factory.CreateConnectionAsync();
////        using var channel = await connection.CreateChannelAsync();

////        // Declare the exchange of type Fanout (matching the receiver)
////        await channel.ExchangeDeclareAsync(exchange: "logs", type: ExchangeType.Fanout);

////        // Allow publishing messages from command line
////        while (true)
////        {
////            Console.WriteLine(" [*] Enter a log message (or 'exit' to quit):");
////            string message = Console.ReadLine();

////            // Exit condition
////            if (string.IsNullOrWhiteSpace(message) || message.ToLower() == "exit")
////                break;

////            // Convert message to bytes
////            var body = Encoding.UTF8.GetBytes(message);

////            // Publish the message to the exchange
////            await channel.BasicPublishAsync(
////                exchange: "logs",
////                routingKey: string.Empty,
////                body: body
////            );

////            Console.WriteLine($" [x] Sent '{message}'");
////        }
////    }
////}

//using RabbitMQ.Client;
//using System;
//using System.Text;

//public class RabbitMQPublisher
//{
//    public static void PublishMessage(
//        string message,
//        string rabbitMqHost = "localhost",
//        string queueName = "queue")
//    {
//        // Create connection factory
//        var factory = new ConnectionFactory()
//        {
//            HostName = rabbitMqHost
//        };

//        // Create connection and channel
//        using (var connection = factory.CreateConnectionAsync())
//        using (var channel = connection.CreateModel())
//        {
//            // Declare the queue (durable and not auto-deleted)
//            channel.QueueDeclare(
//                queue: queueName,
//                durable: true,
//                exclusive: false,
//                autoDelete: false,
//                arguments: null);

//            // Convert message to byte array
//            var body = Encoding.UTF8.GetBytes(message);

//            // Set message properties (persistent delivery mode)
//            var properties = channel.CreateBasicProperties();
//            properties.Persistent = true;

//            // Publish the message
//            channel.BasicPublish(
//                exchange: "",
//                routingKey: queueName,
//                basicProperties: properties,
//                body: body);

//            Console.WriteLine($"Sent message: {message}");
//        }
//    }

//    // Example usage
//    public static void Main(string[] args)
//    {
//        PublishMessage("Hello");
//    }
//}
using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(exchange: "logs", type: ExchangeType.Fanout);

var message = GetMessage(args);
var body = Encoding.UTF8.GetBytes(message);
await channel.BasicPublishAsync(exchange: "logs", routingKey: string.Empty, body: body);
Console.WriteLine($" [x] Sent {message}");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

static string GetMessage(string[] args)
{
    return ((args.Length > 0) ? string.Join(" ", args) : "info: Hello World!");
}