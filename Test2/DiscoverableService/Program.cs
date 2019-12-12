using System;
using System.Collections.Generic;
using EMG.Utilities.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Topshelf;

namespace EMG.DiscoverableService
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = HostFactory.New(configure =>
            {
                configure.HostService(ConfigureAppConfiguration, ConfigureServices, ConfigureLogging);

                configure.SetDisplayName("EMG DiscoverableService");

                configure.SetServiceName("EMG.DiscoverableService");

                configure.SetDescription("A Windows service created by EMG");

                configure.EnableServiceRecovery(rc => rc.RestartService(TimeSpan.FromMinutes(1))
                                                        .RestartService(TimeSpan.FromMinutes(5))
                                                        .RestartService(TimeSpan.FromMinutes(10))
                                                        .SetResetPeriod(1));

                configure.RunAsLocalService();

                configure.StartAutomaticallyDelayed();

                configure.SetStopTimeout(TimeSpan.FromMinutes(5));
            });

            host.Run();
        }

        private static void ConfigureAppConfiguration(IConfigurationBuilder configuration, IServiceEnvironment environment)
        {
            var settings = new Dictionary<string, string>
            {
                ["Loggly:ApiKey"] = "asd-lol",
                ["Loggly:ApplicationName"] = "EMG.DiscoverableService"
            };

            configuration.AddInMemoryCollection(settings);

            configuration.AddJsonFile("appsettings.json", true, true);
            configuration.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, true);

            configuration.AddEnvironmentVariables();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IServiceEnvironment environment)
        {



        }

        private static void ConfigureLogging(ILoggingBuilder logging, IConfiguration configuration, IServiceEnvironment environment)
        {
            logging.AddConfiguration(configuration.GetSection("Logging"));

            logging.AddConsole();

            logging.AddLoggly(configuration.GetSection("Loggly"));
        }
    }
}
