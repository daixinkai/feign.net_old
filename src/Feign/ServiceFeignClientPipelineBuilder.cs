using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    class ServiceFeignClientPipelineBuilder : FeignClientPipelineBuilderBase, IFeignClientPipelineBuilder
    {
        public ServiceFeignClientPipelineBuilder(string serviceId)
        {
            _serviceId = serviceId;
        }
        string _serviceId;
    }
}
