using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Feign.Proxy
{
    class FeignClientPipelineBuilder : IFeignClientPipelineBuilder
    {
        public event EventHandler<BuildingRequestEventArgs> BuildingRequest;
        public event EventHandler<SendingRequestEventArgs> SendingRequest;

        internal BuildingRequestEventArgs OnBuildingRequest(IFeignClientProxy feignClientProxy, string method, Uri requestUri, IDictionary<string, string> headers)
        {
            BuildingRequestEventArgs args = new BuildingRequestEventArgs(method, requestUri, headers)
            {
                FeignClientProxy = feignClientProxy
            };
            BuildingRequest?.Invoke(feignClientProxy, args);
            return args;
        }

        internal SendingRequestEventArgs OnSendingRequest(IFeignClientProxy feignClientProxy, HttpRequestMessage requestMessage)
        {
            SendingRequestEventArgs args = new SendingRequestEventArgs(requestMessage)
            {
                FeignClientProxy = feignClientProxy
            };
            SendingRequest?.Invoke(feignClientProxy, args);
            return args;
        }

    }
}
