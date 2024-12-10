//using RabbitMQ.Client;
//using RabbitMQ.Client.Events;
//using System;
//using System.Text;
//using System.Threading.Tasks;

//class RabbitMqConsumer
//{
//    public async Task Consume()
//    {
//        var factory = new ConnectionFactory { HostName = "localhost" };
//        using var connection = await factory.CreateConnectionAsync();
//        using var channel = await connection.CreateChannelAsync();

//        await channel.ExchangeDeclareAsync(exchange: "logs", type: ExchangeType.Fanout);

//        // Declare a server-named queue
//        QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync();
//        string queueName = queueDeclareResult.QueueName;
//        await channel.QueueBindAsync(queue: queueName, exchange: "logs", routingKey: string.Empty);

//        Console.WriteLine(" [*] Waiting for logs.");

//        var consumer = new AsyncEventingBasicConsumer(channel);
//        consumer.ReceivedAsync += (model, ea) =>
//        {
//            byte[] body = ea.Body.ToArray();
//            var message = Encoding.UTF8.GetString(body);
//            Console.WriteLine($" [x] {message}");
//            return Task.CompletedTask;
//        };

//        await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);

//        Console.WriteLine(" Press [enter] to exit.");
//        Console.ReadLine();
//    }

//    public static async Task Main(string[] args)
//    {
//        var consumer = new RabbitMqConsumer();
//        await consumer.Consume();
//    }
//}
