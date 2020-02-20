using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common;
using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ConsulDotnet
{
    class Program
    {
        
        private static Startup Startup;
        private static IServiceProvider ServiceProvider;
        public static async Task Main(string[] args)
        {
            IConfiguration Configuration;
            Startup = ConsoleAppConfigurator.BootstrapApp();
            var serviceCollection = new ServiceCollection();
            Startup.ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
            Configuration = Startup.Configuration;
            var ConfigOptionsTest = new ConfigOptions();
            Configuration.GetSection("ConfigOptions").Bind(ConfigOptionsTest);
            DataOptions dataOptions =  ServiceProvider.GetService<IOptions<DataOptions>>().Value;
            ConfigOptions configOptions = ServiceProvider.GetService<IOptions<ConfigOptions>>().Value;
            // Find the ServiceA
            using (var consulClient = new ConsulClient(a => a.Address = new Uri(dataOptions.ConsulUrl)))
            {
                var services = consulClient.Catalog.Service("ServiceA").Result.Response;
                if (services != null && services.Any())
                {
                    // 模拟随机一台进行请求，这里只是测试，可以选择合适的负载均衡工具或框架
                    Random r = new Random();
                    int index = r.Next(services.Count());
                    var service = services.ElementAt(index);

                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.GetAsync($"http://{service.ServiceAddress}:{service.ServicePort}/weatherforecast");
                        var result = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(result);
                    }
                }
            }
            //Find the ServiceB
            using (var consulClient = new ConsulClient(a => a.Address = new Uri(dataOptions.ConsulUrl)))
            {
                var services = consulClient.Catalog.Service("ServiceB").Result.Response;
                if (services != null && services.Any())
                {
                    // 模拟随机一台进行请求，这里只是测试，可以选择合适的负载均衡工具或框架
                    Random r = new Random();
                    int index = r.Next(services.Count());
                    var service = services.ElementAt(index);

                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.GetAsync($"http://{service.ServiceAddress}:{service.ServicePort}/weatherforecast");
                        var result = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(result);
                    }
                }
            }
            //Put or Replace the config, the ServiceA and ServiceB will sync the config
            using (var consulClient = new ConsulClient(a => a.Address = new Uri(dataOptions.ConsulUrl)))
            {
                var putPair = new KVPair("Config.json")
                {
                    Value = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(configOptions))
                };

                var putAttempt = await consulClient.KV.Put(putPair);

                if (putAttempt.Response)
                {
                    var getPair = await consulClient.KV.Get("Config.json");
                    string result = Encoding.UTF8.GetString(getPair.Response.Value, 0,
                        getPair.Response.Value.Length);
                    Console.WriteLine(result);
                }
            }
        }
    }
}
