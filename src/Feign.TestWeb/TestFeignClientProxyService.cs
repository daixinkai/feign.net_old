using Feign.Discovery;
using Feign.Proxy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Feign.TestWeb
{
    public class TestFeignClientProxyService : FeignClientProxyService
    {
        public TestFeignClientProxyService(IServiceDiscovery serviceDiscovery, IGlobalFeignClientPipelineBuilder globalFeignClientPipeline, IDistributedCache distributedCache, ILoggerFactory loggerFactory) : base(serviceDiscovery, globalFeignClientPipeline, distributedCache, loggerFactory)
        {

        }
        public override string ServiceId => throw new NotImplementedException();

        public void Test(string s)
        {
            Get<JObject>(s);

        }

    }
}
