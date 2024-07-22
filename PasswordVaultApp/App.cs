using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PasswordVaultApp
{
    public class App
    {
        using IHost host = CreateHostBuilder(args).Build();
    }

static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        })
        .ConfigureServices((_, services) =>
        {
            services.AddSingleton<IHashService, HashService>();
            services.AddSingleton<IDynamodbRepository>(sp =>
            {
                ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                try
                {
                    Env.TraversePath().Load();
                    string? accessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
                    string? secretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
                    string? region = Environment.GetEnvironmentVariable("AWS_REGION");
                    string? usersTableName = Environment.GetEnvironmentVariable("AWS_DYNAMODB_TABLE_NAME");

                    return new DynamodbRepository(accessKeyId, secretAccessKey, region, usersTableName, loggerFactory);
                } catch (Exception ex)
                {
                    throw new ApplicationException($"Error: {ex.Message}");
                }
            });
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IXORCipher, XORCipherService>();
            services.AddSingleton<App>();
        });
}
}
