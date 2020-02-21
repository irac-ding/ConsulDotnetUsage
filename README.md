# Consul in .net core 3
##Consul 介绍

在分布式架构中，服务治理是必须面对的问题，如果缺乏简单有效治理方案，各服务之间只能通过人肉配置的方式进行服务关系管理，当遇到服务关系变化时，就会变得极其麻烦且容易出错。

Consul 是一个用来实现分布式系统服务发现与配置的开源工具。它内置了服务注册与发现框架、分布一致性协议实现、健康检查、Key/Value存储、多数据中心方案，不再需要依赖其他工具（比如 ZooKeeper 等），使用起来也较为简单。
![Consul 架构图](https://github.com/irac-ding/ConsulDotnetUsage/blob/master/picture/5378831-1b41fc061123189b.png "Consul 架构图")
Consul 集群支持多数据中心，在上图中有两个 DataCenter，他们通过 Internet 互联，为了提高通信效率，只有 Server 节点才加入跨数据中心的通信。在单个数据中心中，Consul 分为 Client 和 Server 两种节点（所有的节点也被称为 Agent），Server 节点保存数据，Client 负责健康检查及转发数据请求到 Server，本身不保存注册信息；Server 节点有一个 Leader 和多个 Follower，Leader 节点会将数据同步到 Follower，Server 节点的数量推荐是3个或者5个，在 Leader 挂掉的时候会启动选举机制产生一个新 Leader。

##主要参数：
具体启动文档见 [configuration](https://www.consul.io/docs/agent/options.html#configuration_files "configuration")。
如:
consul agent -server -config-dir /etc/consul.d -bind=192.168.1.100
    -config-dir /etc/consul.d
config-dir
需要加载的配置文件目录，consul将加载目录下所有后缀为“.json”的文件，加载顺序为字母顺序，文件中配置选项合并方式如config-file。该参数可以多次配置。目录中的子目录是不会加载的。

data-dir
此目录是为Agent存放state数据的。是所有Agent需要的，该目录应该存放在持久存储中（reboot不会丢失），对于server角色的Agent是很关键的，需要记录集群状态。并且该目录是支持文件锁。

server
设置Agent是server模式还是client模式。Consul agent有两种运行模式：Server和Client。这里的Server和Client只是Consul集群层面的区分，与搭建在Cluster之上 的应用服务无关。Consule Server模式agent节点用于采用raft算法维护Consul集群的状态，官方建议每个Consul Cluster至少有3个或以上的运行在Server mode的Agent，Client节点不限。

其他常用的还有：

client
将绑定到client接口的地址，可以是HTTP、DNS、RPC服务器。默认为“127.0.0.1”,只允许回路连接。RPC地址会被其他的consul命令使用，比如consul members,查询agent列表

node
节点在集群的名字，在集群中必须是唯一的。默认为节点的Hostname。

bootstrap
设置服务是否为“bootstrap”模式。如果数据中心只有1个server agent，那么需要设置该参数。从技术上来讲，处于bootstrap模式的服务器是可以选择自己作为Raft Leader的。在consul集群中，只有一个节点可以配置该参数，如果有多个参数配置该参数，那么难以保证一致性。

bind
用于集群内部通信的IP地址，与集群中其他节点互连可通。默认为“0.0.0.0”，consul将使用第一个有效的私有IPv4地址。如果指定“[::]”，consul将使用第一个有效的公共IPv6地址。使用TCP和UDP通信。注意防火墙，避免无法通信。

##Windows

 Goto https://www.consul.io/downloads.html download Consul Zip file, Extract to C:/Consul，
 build And Run: cmd run the buildAndRun.bat
 
##实战：

![项目图](https://github.com/irac-ding/ConsulDotnetUsage/blob/master/picture/5378831-36333b210141eef9.png "项目图")

###1.创建 .NET Core WebAPI 服务 ServiceA（2个实例） 和 ServiceB

###2.NuGet 安装 Consul

###3.注册到 Consul 的核心代码如下（源码下载）：
```csharp
public static class ConsulBuilderExtensions
{
  public static IApplicationBuilder RegisterConsul(this IApplicationBuilder app, IApplicationLifetime lifetime, ConsulOption consulOption)
  {
    var consulClient = new ConsulClient(x =>
    {
      // consul 服务地址
      x.Address = new Uri(consulOption.Address);
    });

    var registration = new AgentServiceRegistration()
    {
      ID = Guid.NewGuid().ToString(),
      Name = consulOption.ServiceName,// 服务名
      Address = consulOption.ServiceIP, // 服务绑定IP
      Port = consulOption.ServicePort, // 服务绑定端口
      Check = new AgentServiceCheck()
      {
        DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务启动多久后注册
        Interval = TimeSpan.FromSeconds(10),//健康检查时间间隔
        HTTP = consulOption.ServiceHealthCheck,//健康检查地址
        Timeout = TimeSpan.FromSeconds(5)
      }
    };

    // 服务注册
    consulClient.Agent.ServiceRegister(registration).Wait();

    // 应用程序终止时，服务取消注册
    lifetime.ApplicationStopping.Register(() =>
    {
      consulClient.Agent.ServiceDeregister(registration.ID).Wait();
    });
    return app;
  }
}
```
###4.添加配置如下：
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "ConsulOption": {
    "AllowedHosts": "*",
    "ServiceName": "ServiceA",
    "ServiceIP": "192.168.100.12",
    "ServicePort": 8010,
    "ServiceHealthCheck": "http://192.168.100.12:8010/healthCheck",
    "ConsulAddress": "http://192.168.100.12:8500"
  }
}

```
###5.注册成功结果如下：
![注册成功结果](https://github.com/irac-ding/ConsulDotnetUsage/blob/master/picture/image%20(1).png "注册成功结果")

###6.服务发现
```csharp
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
```
###7.Key/Value存储 同步配置文件"Config.json"

####A.ServiceA and ServiceB every 5s sync the config.json
```csharp
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
               .ConfigureAppConfiguration(
                    builder =>
                    {
                        builder
                            .AddConsul(
                                "Config.json",
                                options =>
                                {
                                    options.ConsulConfigurationOptions =
                                        cco => { cco.Address = new Uri("http://192.168.100.12:8500"); };
                                    options.Optional = true;
                                    options.PollWaitTime = TimeSpan.FromSeconds(5);
                                    options.ReloadOnChange = true;
                                })
                            .AddEnvironmentVariables();
                    });
    }
```
########B.ServiceA and ServiceB add ConfigController(remember _configOptions will not change after startup. In fact Only startup it's will update the lately config file)
```csharp
namespace ServiceA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private ConfigOptions _configOptions;
        public ConfigController(IConfiguration configuration, IOptions<ConfigOptions> configOptions)
        {
            _configuration = configuration;
            _configOptions = configOptions.Value;
        }

        /// <summary>
        /// ConsulOption:ConsulAddress
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet("{key}")]
        public IActionResult GetValueForKey(string key)
        {
            return Ok(_configuration.GetSection(key));
        }
        [HttpGet("GetConfigOptions")]
        public ConfigOptions GetConfigOptions()
        {
            var ConfigOptions = new ConfigOptions();
            // read the latest config from memory,remember
            _configuration.GetSection("ConfigOptions").Bind(ConfigOptions);
            return ConfigOptions;
        }
    }
}
```

####C.ConsulDotnet Console Put or Replace or update the config.json
```csharp
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
```
####D.Open with browser http://localhost:8010/swagger/index.html or  http://localhost:8011/swagger/index.html

![结果](https://github.com/irac-ding/ConsulDotnetUsage/blob/master/picture/image.png "结果")

####E.you can open _http://localhost:8500/ui/dc1/kv/Config.json/edit to edit the config.json 

![编辑Config.json](https://github.com/irac-ding/ConsulDotnetUsage/blob/master/picture/2.png "编辑Config.json")
