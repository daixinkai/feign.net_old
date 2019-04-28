using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Feign
{
    class GlobalFeignClientPipelineBuilder : FeignClientPipelineBuilderBase, IGlobalFeignClientPipelineBuilder
    {
        IDictionary<string, ServiceFeignClientPipelineBuilder> _servicePipelineBuilderMap = new Dictionary<string, ServiceFeignClientPipelineBuilder>();
        public ServiceFeignClientPipelineBuilder GetServicePipeline(string serviceId)
        {
            ServiceFeignClientPipelineBuilder serviceFeignClientPipeline;
            _servicePipelineBuilderMap.TryGetValue(serviceId, out serviceFeignClientPipeline);
            return serviceFeignClientPipeline;
        }

        public ServiceFeignClientPipelineBuilder GetOrAddServicePipeline(string serviceId)
        {
            ServiceFeignClientPipelineBuilder serviceFeignClientPipeline;
            if (_servicePipelineBuilderMap.TryGetValue(serviceId, out serviceFeignClientPipeline))
            {
                return serviceFeignClientPipeline;
            }
            serviceFeignClientPipeline = new ServiceFeignClientPipelineBuilder(serviceId);
            _servicePipelineBuilderMap[serviceId] = serviceFeignClientPipeline;
            return serviceFeignClientPipeline;
        }

    }
}
