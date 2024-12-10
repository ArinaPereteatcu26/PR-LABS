using HtmlAgilityPack;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.IO;
using System.Net;


public class FtpDownloader
{
    public async Task<string> DownloadFile(string ftpUrl, string username, string password, string filePath)
    {
        try
        {
            ServicePointManager.Expect100Continue = false;

            // Step 1: Download the JSON file from FTP
            string fileName = Path.GetFileName(filePath);
            string localFilePath = Path.Combine(Path.GetTempPath(), fileName);

            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential(username, password);
                await client.DownloadFileTaskAsync(new Uri(ftpUrl), localFilePath);
            }

            Console.WriteLine($"File downloaded to: {localFilePath}");

            // Step 2: Read the downloaded file as a string
            string fileContent = File.ReadAllText(localFilePath);
            Console.WriteLine($"File content: {fileContent}");

            // Step 3: Delete the file from the FTP server
            DeleteFileFromFtp(ftpUrl, username, password);
            Console.WriteLine($"File deleted from FTP: {ftpUrl}");

            return localFilePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return string.Empty;
        }
    }

    private void DeleteFileFromFtp(string ftpUrl, string username, string password)
    {
        try
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = new NetworkCredential(username, password);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                Console.WriteLine($"Delete status: {response.StatusDescription}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while deleting the file: {ex.Message}");
        }
    }

}

class RabbitMqConsumer
{
    public async Task Consume()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange: "logs", type: ExchangeType.Fanout);

        // Declare a server-named queue
        QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync();
        string queueName = queueDeclareResult.QueueName;
        await channel.QueueBindAsync(queue: queueName, exchange: "logs", routingKey: string.Empty);

        Console.WriteLine(" [*] Waiting for logs.");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [x] {message}");
            return Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }

    public static async Task Main(string[] args)
    {
        //    var consumer = new RabbitMqConsumer();
        //    await consumer.Consume();



        // Download a file using FtpDownloader
        var ftpDownloader = new FtpDownloader();
        string downloadedFilePath = await ftpDownloader.DownloadFile("ftp://localhost:21/miau.json", "testuser", "testpass", "miau.json");

        // Check if file was successfully downloaded
        if (!string.IsNullOrEmpty(downloadedFilePath))
        {
            Console.WriteLine($"File downloaded and saved to: {downloadedFilePath}");
        }
    }
}