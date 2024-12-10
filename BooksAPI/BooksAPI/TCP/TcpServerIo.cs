using System.Net;
using System.Net.Sockets;

namespace BooksAPI.TCP;

//object for logging purposes,helps with log errors
public class TcpServerIo(ILogger<TcpServerIo> logger) : IHostedService
{
    private const string Filepath = "/tmp/sharedtext.txt";
    private const int Port = 6000;
    //control access and prevent race conditions
    private readonly SemaphoreSlim _fileLock = new(1);
    private readonly Queue<(string Content, TaskCompletionSource<bool> Completion)> _writeQueue = new();
    private readonly object _queueLock = new();
    private int _pendingWrites;
    private TcpListener _listener = new(IPAddress.Any, Port);//incoming TCP connections
    private bool _isRunning;
    private readonly Random _random = new();
    private Task? _writeProcessorTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(Filepath))
            {
                File.WriteAllText(Filepath, string.Empty);
            }

            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();
            _isRunning = true;

            _writeProcessorTask = ProcessWriteQueueAsync(cancellationToken);

            _ = AcceptClientsAsync(cancellationToken);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start TCP server");
            throw;
        }
    }

    private async Task ProcessWriteQueueAsync(CancellationToken cancellationToken)
    {
        while (_isRunning && !cancellationToken.IsCancellationRequested)
        {
            (string Content, TaskCompletionSource<bool> Completion)? writeOperation = null;

            lock (_queueLock)
            {
                if (_writeQueue.Count > 0)
                {
                    writeOperation = _writeQueue.Dequeue();
                }
            }

            if (writeOperation != null)
            {
                try
                {
                    await _fileLock.WaitAsync(cancellationToken);
                    try
                    {
                        await Task.Delay(_random.Next(500, 2000), cancellationToken);
                        await File.AppendAllTextAsync(Filepath, writeOperation.Value.Content + Environment.NewLine, cancellationToken);
                        writeOperation.Value.Completion.SetResult(true);
                    }
                    finally
                    {
                        _fileLock.Release();
                        Interlocked.Decrement(ref _pendingWrites);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing write operation");
                    writeOperation.Value.Completion.SetException(ex);
                }
            }
            else
            {
                await Task.Delay(100, cancellationToken);
            }
        }
    }

    private async Task AcceptClientsAsync(CancellationToken cancellationToken)
    {
        while (_isRunning && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                _ = HandleClientAsync(client, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Error accepting client connection");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            using var _ = client;
            await using var stream = client.GetStream();
            using var reader = new StreamReader(stream);
            await using var writer = new StreamWriter(stream) { AutoFlush = true };

            await writer.WriteLineAsync("Welcome! Available commands: 'read', 'write <content>'");

            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrEmpty(line)) break;

                var parts = line.Split(' ', 2);
                var command = parts[0].ToLower();
                var content = parts.Length > 1 ? parts[1] : string.Empty;

                switch (command)
                {
                    case "write" when !string.IsNullOrWhiteSpace(content):
                        await WriteToFileAsync(content, writer, cancellationToken);
                        break;

                    case "write":
                        await writer.WriteLineAsync("Write command requires content. Usage: write <content>");
                        break;

                    case "read":
                        await ReadFromFileAsync(writer, cancellationToken);
                        break;

                    default:
                        await writer.WriteLineAsync("Unknown command. Available commands: 'read', 'write <content>'");
                        break;
                }
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Error handling client connection");
        }
    }

    private async Task WriteToFileAsync(string content, TextWriter clientWriter, CancellationToken cancellationToken)
    {
        var completionSource = new TaskCompletionSource<bool>();

        try
        {
            lock (_queueLock)
            {
                Interlocked.Increment(ref _pendingWrites);
                _writeQueue.Enqueue((content, completionSource));
            }

            await completionSource.Task;
            await clientWriter.WriteLineAsync("Write operation successful.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during write operation");
            await clientWriter.WriteLineAsync("Write operation failed.");
            throw;
        }
    }

    private async Task ReadFromFileAsync(TextWriter clientWriter, CancellationToken cancellationToken)
    {
        try
        {
            while (Interlocked.CompareExchange(ref _pendingWrites, 0, 0) > 0)
            {
                await Task.Delay(100, cancellationToken);
            }

            await _fileLock.WaitAsync(cancellationToken);
            try
            {
                if (File.Exists(Filepath))
                {
                    var content = await File.ReadAllTextAsync(Filepath, cancellationToken);
                    await clientWriter.WriteLineAsync($"File content:\n{content}");
                }
                else
                {
                    await clientWriter.WriteLineAsync("File does not exist.");
                }
            }
            finally
            {
                _fileLock.Release();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during read operation");
            await clientWriter.WriteLineAsync("Read operation failed.");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _isRunning = false;
        _listener.Stop();

        if (_writeProcessorTask != null)
        {
            try
            {
                await _writeProcessorTask;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while stopping write processor");
            }
        }

        _fileLock.Dispose();
    }
}