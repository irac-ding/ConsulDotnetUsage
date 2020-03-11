using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsulDotnet
{
    public class Startup
    {
        public IHostEnvironment HostingEnvironment { get; }
        public IConfiguration Configuration { get; set; }

        public Startup(
            IHostEnvironment hostingEnvironment,
            IConfigurationBuilder configurationBuilder
        )
        {
            HostingEnvironment = hostingEnvironment;
            Configuration = configurationBuilder.Build();
        }

        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            // HostingEnvironment.EnvironmentName 通过环境变量设置
            Configuration = new ConfigurationBuilder()
              .SetBasePath(HostingEnvironment.ContentRootPath)
              .AddJsonFile("appsettings.json", true, true)
               .AddJsonFile("Config.json", true, true)
              .AddJsonFile($"appsettings.{HostingEnvironment.EnvironmentName}.json", true, true)
              .AddEnvironmentVariables()
              .Build();
            serviceCollection.AddSingleton(Configuration);
            serviceCollection.AddSingleton(HostingEnvironment);
            serviceCollection.AddOptions();
            serviceCollection.Configure<DataOptions>(Configuration.GetSection("Data"));
            serviceCollection.Configure<ConfigOptions>(Configuration.GetSection("ConfigOptions"));
            serviceCollection.AddSingleton<IConsulServicesFind, ConsulServicesFind>();
        }
    }
}
