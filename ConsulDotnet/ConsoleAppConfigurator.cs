using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace ConsulDotnet
{
    public static class ConsoleAppConfigurator
    {
        public static Startup BootstrapApp()
        {
            var environment = GetEnvironment();
            var hostingEnvironment = GetHostingEnvironment(environment);
            var configurationBuilder = CreateConfigurationBuilder(environment);

            return new Startup(hostingEnvironment, configurationBuilder);
        }

        private static string GetEnvironment()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(environmentName))
            {
                return "Development";
                //return "Staging";
            }

            return environmentName;
        }

        private static IHostEnvironment GetHostingEnvironment(string environmentName)
        {
            return new HostingEnvironment
            {
                EnvironmentName = environmentName,
                ApplicationName = AppDomain.CurrentDomain.FriendlyName,
                ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
                ContentRootFileProvider = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory)
            };
        }

        private static IConfigurationBuilder CreateConfigurationBuilder(string environmentName)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                //.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            return config;
        }
    }
}
