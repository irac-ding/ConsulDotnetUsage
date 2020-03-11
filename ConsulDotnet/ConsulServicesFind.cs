using Consul;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsulDotnet
{
    /*
     * 服务发现
     * (服务和健康信息)http://localhost:8500/v1/health/service/[ServiceName]
     * (健康信息)http://localhost:8500/v1/health/checks/[ServiceName]
     */
    public class ConsulServicesFind: IConsulServicesFind
    {
        DataOptions _dataOptions;
        ConsulClient consulClient;
        public ConsulServicesFind(IOptions<DataOptions> dataOptions)
        {
            _dataOptions = dataOptions.Value;
            consulClient = new ConsulClient(a => a.Address = new Uri(_dataOptions.ConsulUrl));
        }
        
        public List<HealthCheck> FindHealthConsulServices(string ServiceName)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string findUrl = $"{_dataOptions.ConsulUrl}/v1/health/checks/{ServiceName}";
            string findResult = HttpHelper.HttpGet(findUrl, headers, 5);
            var findCheck = JsonConvert.DeserializeObject<List<HealthCheck>>(findResult);
            return findCheck.Where(g => g.Status== HealthStatus.Passing).ToList();
        }
        public List<CatalogService> FindConsulServices(string ServiceName)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string findUrl = $"{_dataOptions.ConsulUrl}/v1/health/checks/{ServiceName}";
            string findResult = HttpHelper.HttpGet(findUrl, headers, 5);
            var findCheck = JsonConvert.DeserializeObject<List<HealthCheck>>(findResult);
            CatalogService[] services = consulClient.Catalog.Service(ServiceName).Result.Response;
            List<HealthCheck> healthChecks = FindHealthConsulServices(ServiceName);
            return services.ToList().Where(x => healthChecks.Select(n => n.ServiceID).ToArray().Contains(x.ServiceID)).ToList();
        }
    }
}
