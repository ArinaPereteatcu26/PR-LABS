//using RabbitMQ.Client;
//using System;
//using System.Text;
//using System.Threading.Tasks;

//public class RabbitMQSender
//{
//    private readonly string _hostName = "localhost";
//    private readonly string _exchangeName = "logs";

//    public async Task Send(string message)
//    {
//        var factory = new ConnectionFactory { HostName = _hostName };
//        using var connection = await factory.CreateConnectionAsync();
//        using var channel = await connection.CreateChannelAsync();

//        await channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Fanout);

//        var body = Encoding.UTF8.GetBytes(message);
//        await channel.BasicPublishAsync(exchange: _exchangeName, routingKey: string.Empty, body: body);
//        Console.WriteLine($" [x] Sent {message}");
//    }
//    static async Task Main(string[] args)
//    {
//        var sender = new RabbitMQSender();
//        await sender.Send("ihai was here");

//        Console.WriteLine(" Press [enter] to exit.");
//        Console.ReadLine();
//    }
//}