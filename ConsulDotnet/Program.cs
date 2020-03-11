using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Consul;
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
            DataOptions dataOptions = ServiceProvider.GetService<IOptions<DataOptions>>().Value;
            ConfigOptions configOptions = ServiceProvider.GetService<IOptions<ConfigOptions>>().Value;
            IConsulServicesFind consulServicesFind = ServiceProvider.GetRequiredService<IConsulServicesFind>();
           
            ConsulOption consulOption=new ConsulOption();
            Configuration.GetSection("ConsulOption").Bind(consulOption);
            //注册10个不健康的ServiceA节点
            consulOption.ServiceHealthCheck = "http://127.0.0.1:8088/healthCheck";
            for (int i = 0; i < 10; i++)
            {
                RegisterConsul(consulOption);
            }
            // Find the ServiceA

            using (var consulClient = new ConsulClient(a => a.Address = new Uri(dataOptions.ConsulUrl)))
            {
                CatalogService[] services = consulServicesFind.FindConsulServices("ServiceA").ToArray();

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
            //注册10个不健康的ServiceB节点
            consulOption.ServiceName = "ServiceB";
            for (int i = 0; i < 10; i++)
            {
                RegisterConsul(consulOption);
            }
            //Find the ServiceB
            using (var consulClient = new ConsulClient(a => a.Address = new Uri(dataOptions.ConsulUrl)))
            {
                CatalogService[] services = consulServicesFind.FindConsulServices("ServiceB").ToArray();
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
                string jsonString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"), Encoding.Default);
                var putPair = new KVPair("Config.json")
                {
                    Value = Encoding.UTF8.GetBytes(jsonString)
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
        public static void RegisterConsul(ConsulOption consulOption)
        {
            var consulClient = new ConsulClient(x =>
            {
                // consul 服务地址
                x.Address = new Uri(consulOption.ConsulAddress);
            });

            var registration = new AgentServiceRegistration()
            {
                ID = Guid.NewGuid().ToString(),
                Name = consulOption.ServiceName,// 服务名
                Tags = consulOption.Tags,
                Address = consulOption.ServiceIP, // 服务绑定IP
                Port = consulOption.ServicePort, // 服务绑定端口
                Check = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务启动多久后注册
                    Interval = TimeSpan.FromSeconds(10),//健康检查时间间隔
                    HTTP = consulOption.ServiceHealthCheck,//健康检查地址
                    Timeout = TimeSpan.FromSeconds(5)
                },
                Meta = consulOption.Meta
            };

            // 服务注册
            consulClient.Agent.ServiceRegister(registration).Wait();
        }
    }
}
