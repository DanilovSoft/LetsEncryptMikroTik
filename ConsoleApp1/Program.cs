using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using LetsEncryptMikroTik.Core;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace LetsEncryptMikroTik;

class Program
{
    static int Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        var options = configuration.Get<Options>();

        using var loggerFactory = LoggerFactory.Create(builder => 
        {
            builder.ClearProviders();
            builder.AddConfiguration(configuration.GetSection("Logging"));
        });
        var logger = loggerFactory.CreateLogger<Program>();

        var certUpdater = new Core.CertUpdater(options, logger);

        try
        {
            certUpdater.RunAsync().GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            return 1;
        }

        if (Environment.UserInteractive)
        {
            Console.Write("Press Any Key To Exit");
            Console.ReadKey(intercept: true); // Press Any Key
        }
        return 0;
    }
}
