using EvolveDb;
using Npgsql;

namespace BooksAPI;

public static class DatabaseMigrator
{
    public static async Task EnsureDatabase(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var database = builder.Database;

        builder.Database = "postgres";
        var masterConnectionString = builder.ToString();

        await using var connection = new NpgsqlConnection(masterConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            $"SELECT 1 FROM pg_database WHERE datname = '{database}'",
            connection);

        var exists = command.ExecuteScalar() != null;

        if (!exists)
        {
            var createDbCommand = new NpgsqlCommand($"CREATE DATABASE {database}", connection);
            await createDbCommand.ExecuteNonQueryAsync();
        }
    }

    public static void RunMigrations(WebApplication app, string connectionString)
    {
        var evolveConnection = new NpgsqlConnection(connectionString);
        var evolve = new Evolve(evolveConnection, msg => app.Logger.LogInformation(msg))
        {
            Locations = ["Migrations/"],
            IsEraseDisabled = true,
            MetadataTableName = "changelog",
            MetadataTableSchema = "public",
            PlaceholderPrefix = "${",
            PlaceholderSuffix = "}",
            EnableClusterMode = true,
            CommandTimeout = 300
        };

        var retryCount = 0;
        const int maxRetries = 10;
        const int retryDelaySeconds = 5;

        while (retryCount < maxRetries)
        {
            try
            {
                evolve.Migrate();
                app.Logger.LogInformation("Database migration completed successfully");
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                app.Logger.LogError($"Database migration attempt {retryCount} of {maxRetries} failed:");
                app.Logger.LogError(ex.Message);

                if (retryCount < maxRetries)
                {
                    app.Logger.LogInformation($"Retrying in {retryDelaySeconds} seconds...");
                    Thread.Sleep(TimeSpan.FromSeconds(retryDelaySeconds));
                }
                else
                {
                    app.Logger.LogCritical("Max retry attempts reached. Exiting...");
                    throw;
                }
            }
        }
    }
}