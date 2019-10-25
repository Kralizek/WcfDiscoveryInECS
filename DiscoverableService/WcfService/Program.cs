using System;
using System.Collections.Generic;
using EMG.Utilities.Hosting;
using System.ServiceModel;
using EMG.Utilities.ServiceModel;
using EMG.Utilities.ServiceModel.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Topshelf;
using EndpointAddress = EMG.Utilities.ServiceModel.Configuration.EndpointAddress;

namespace EMG.WcfService
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = HostFactory.New(configure =>
            {
                configure.HostService(ConfigureAppConfiguration, ConfigureServices, ConfigureLogging);

                configure.SetDisplayName("EMG WcfService");

                configure.SetServiceName("EMG.WcfService");

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
                ["Loggly:ApplicationName"] = "EMG.WcfService"
            };

            configuration.AddInMemoryCollection(settings);

            configuration.AddJsonFile("appsettings.json", true, true);
            configuration.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, true);

            configuration.AddECSMetadataFile();

            configuration.AddEnvironmentVariables();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IServiceEnvironment environment)
        {
            // https://github.com/emgdev/dotnet-utils/tree/master/docs/libraries/ServiceModel
            services.AddWcfService<TestService>(service =>
            {
                service.AddBasicHttpEndpoint(typeof(ITestService), configuration.GetSection("WcfService:TestService:BasicHttp").GetBasicHttpEndpointAddress().UseECS(configuration), binding => binding.WithNoSecurity()).Discoverable();

                //service.AddNamedPipeEndpoint(typeof(ITestService), configuration.GetSection("WcfService:TestService:NamedPipe").GetNamedPipeEndpointAddress(), binding => binding.WithNoSecurity());

                //service.AddNetTcpEndpoint(typeof(ITestService), configuration.GetSection("WcfService:TestService:NetTcp").GetNetTcpEndpointAddress(), binding => binding.WithNoSecurity());

                service.AddMetadataEndpoints();

                service.AddExecutionLogging();
            });

            if (environment.IsProduction())
            {
                services.AddDiscovery<NetTcpBinding>(configuration.GetSection("WCF:AnnouncementService:NetTcp").GetNetTcpEndpointAddress(), TimeSpan.FromSeconds(5), binding => binding.WithNoSecurity());
            }



        }

        private static void ConfigureLogging(ILoggingBuilder logging, IConfiguration configuration, IServiceEnvironment environment)
        {
            logging.AddConfiguration(configuration.GetSection("Logging"));

            logging.AddConsole();
        }
    }

    public static class ECSMetadataExtensions
    {
        public static IConfigurationBuilder AddECSMetadataFile(this IConfigurationBuilder builder)
        {
            var metadataFilePath = Environment.GetEnvironmentVariable("ECS_CONTAINER_METADATA_FILE");

            if (metadataFilePath != null)
            {
                builder.AddJsonFile(metadataFilePath, true);
            }

            return builder;
        }

        public static HttpEndpointAddress UseECS(this HttpEndpointAddress endpointAddress, IConfiguration configuration)
        {
            if (Environment.GetEnvironmentVariable("ECS_CONTAINER_METADATA_FILE") == null)
            {
                return endpointAddress;
            }

            var containerMetadata = configuration.Get<ECSContainerMetadata>();
            
            return EndpointAddress.ForHttp(containerMetadata.HostPrivateIPv4Address, endpointAddress.Path, containerMetadata.PortMappings[0].HostPort, endpointAddress.IsSecure);
        }
    }


    public class ECSContainerMetadata
    {
        public string Cluster { get; set; }
        public string ContainerInstanceARN { get; set; }
        public string TaskARN { get; set; }
        public string TaskDefinitionFamily { get; set; }
        public string TaskDefinitionRevision { get; set; }
        public string ContainerID { get; set; }
        public string ContainerName { get; set; }
        public string DockerContainerName { get; set; }
        public string ImageID { get; set; }
        public string ImageName { get; set; }
        public Portmapping[] PortMappings { get; set; }
        public Network[] Networks { get; set; }
        public string MetadataFileStatus { get; set; }
        public string AvailabilityZone { get; set; }
        public string HostPrivateIPv4Address { get; set; }
    }

    public class Portmapping
    {
        public int ContainerPort { get; set; }
        public int HostPort { get; set; }
        public string BindIp { get; set; }
        public string Protocol { get; set; }
    }

    public class Network
    {
        public string NetworkMode { get; set; }
        public string[] IPv4Addresses { get; set; }
    }

}
