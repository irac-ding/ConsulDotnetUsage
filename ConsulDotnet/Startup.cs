using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsulDotnet
{
    public class Startup
    {
        public IHostEnvironment HostingEnvironment { get; }
        public IConfiguration Configuration { get; }

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
            serviceCollection.AddOptions();
            //serviceCollection.Configure<OrleansConfig>(Configuration.GetSection(nameof(OrleansConfig)));
            serviceCollection.Configure<DataOptions>(Configuration.GetSection("Data"));
            serviceCollection.Configure<ConfigOptions>(Configuration.GetSection("ConfigOptions"));
        }
    }
}
