using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using LetsEncryptMikroTik.Core;

namespace LetsEncryptMikroTik;

class Program
{
    static int Main()
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.local.json", optional: true);

        var configuration = configurationBuilder.Build();
        var config = configuration.Get<Options>();

        var p = new Core.Program(config);

        try
        {
            p.RunAsync().GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            return 1;
        }

        if (Environment.UserInteractive)
        {
            // Press Any Key
            Console.Write("Для завершения нажмите любую клавишу...");
            Console.ReadKey(intercept: true);
        }
        return 0;
    }
}
