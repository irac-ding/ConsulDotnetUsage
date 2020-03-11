using System;
using System.Collections.Generic;
using System.Text;
using Consul;

namespace ConsulDotnet
{
    public interface IConsulServicesFind
    {
        List<HealthCheck> FindHealthConsulServices(string ServiceName);
        List<CatalogService> FindConsulServices(string ServiceName);
    }
}
