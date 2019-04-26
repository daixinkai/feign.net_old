using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Feign
{
    class GlobalFeignClientPipelineBuilder : IGlobalFeignClientPipelineBuilder
    {
        public event EventHandler<BuildingRequestEventArgs> BuildingRequest;
        public event EventHandler<SendingRequestEventArgs> SendingRequest;
        IDictionary<string, ServiceFeignClientPipelineBuilder> _servicePipelineBuilderMap = new Dictionary<string, ServiceFeignClientPipelineBuilder>();

        internal BuildingRequestEventArgs OnBuildingRequest(IFeignClient feignClient, string method, Uri requestUri, IDictionary<string, string> headers)
        {
            BuildingRequestEventArgs args = new BuildingRequestEventArgs(method, requestUri, headers)
            {
                FeignClient = feignClient
            };
            GetServicePipeline(feignClient.ServiceId)?.OnBuildingRequest(feignClient, args);
            BuildingRequest?.Invoke(feignClient, args);
            return args;
        }

        internal SendingRequestEventArgs OnSendingRequest(IFeignClient feignClient, HttpRequestMessage requestMessage)
        {
            SendingRequestEventArgs args = new SendingRequestEventArgs(requestMessage)
            {
                FeignClient = feignClient
            };
            GetServicePipeline(feignClient.ServiceId)?.OnSendingRequest(feignClient, args);
            SendingRequest?.Invoke(feignClient, args);
            return args;
        }

        internal void OnBuildingRequest(object sender, BuildingRequestEventArgs e)
        {
            BuildingRequest?.Invoke(sender, e);
        }
        internal void OnSendingRequest(object sender, SendingRequestEventArgs e)
        {
            SendingRequest?.Invoke(sender, e);
        }

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
