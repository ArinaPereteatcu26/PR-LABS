using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net;

public class BookProcessor
{
    private static readonly HttpClient httpClient = new HttpClient();

    public async Task SendBookToApi(string content)
    {
        try
        {
            // Set up the content as StringContent with proper encoding and headers
            var jsonContent = new StringContent(content, Encoding.UTF8, "application/json");

            // Send the request
            var response = await httpClient.PostAsync("http://localhost:8080/api/books", jsonContent);

            // Ensure the response was successful
            response.EnsureSuccessStatusCode();
            Console.WriteLine($"Successfully sent book data to API. Status: {response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending book data to API: {ex.Message}");
        }
    }
}


public class FtpDownloader
{
    private static readonly HttpClient httpClient = new HttpClient();

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

            // Step 3: Send file content to API-------------------------
            var bookProcessor = new BookProcessor();
            await bookProcessor.SendBookToApi(fileContent);

            // Step 4: Delete the file from the FTP server
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

    public void DeleteFileFromFtp(string ftpUrl, string username, string password)
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

    public List<string> ListFilesInDirectory(string ftpDirectoryUrl, string username, string password)
    {
        List<string> fileUrls = new List<string>();

        try
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpDirectoryUrl);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(username, password);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line != "." && line != "..") // Skip current and parent directory
                    {
                        fileUrls.Add($"{ftpDirectoryUrl}/{line}");
                    }
                }
            }

            Console.WriteLine($"Listed files in directory {ftpDirectoryUrl}: {fileUrls.Count} files found.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while listing files: {ex.Message}");
        }

        return fileUrls;
    }
}

class RabbitMqConsumer
{
    private readonly BookProcessor _bookProcessor = new BookProcessor();

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
        consumer.ReceivedAsync += async (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [x] {message}");
            
            // Send message to API
            await _bookProcessor.SendBookToApi(message);
        };

        await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }

    public static async Task Main(string[] args)
    {
        // Start RabbitMqConsumer in a separate thread
        var consumerTask = Task.Run(async () =>
        {
            var consumer = new RabbitMqConsumer();
            await consumer.Consume();
        });

        // Start FTP download task every 30 seconds in a separate thread
        var ftpTask = Task.Run(async () =>
        {
            var ftpDownloader = new FtpDownloader();
            var username = "testuser";
            var password = "testpass";

            while (true)
            {
                try
                {
                    var fileUrls = ftpDownloader.ListFilesInDirectory("ftp://localhost:21", username, password);

                    // Step 2: Iterate through the list, download each file, and delete it
                    foreach (var fileUrl in fileUrls)
                    {
                        // Download the file content
                        string downloadedFilePath = await ftpDownloader.DownloadFile(fileUrl, username, password, Path.GetFileName(fileUrl));

                        if (!string.IsNullOrEmpty(downloadedFilePath))
                        {
                            // Step 3: Delete the file from the FTP server
                            ftpDownloader.DeleteFileFromFtp(fileUrl, username, password);
                            Console.WriteLine($"File {fileUrl} deleted from FTP server.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error downloading file: {ex.Message}");
                }

                // Wait for 30 seconds before downloading again
                await Task.Delay(30000); // 30 seconds
            }
        });

        // Wait for both tasks to complete
        await Task.WhenAny(consumerTask, ftpTask);
    }
}