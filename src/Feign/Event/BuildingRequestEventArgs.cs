using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    public sealed class BuildingRequestEventArgs : EventArgs
    {
        internal BuildingRequestEventArgs(IFeignClient feignClient, string method, Uri requestUri, IDictionary<string, string> headers)
        {
            Method = method;
            RequestUri = requestUri;
            Headers = headers;
            FeignClient = feignClient;
        }
        public string Method { get; }
        public Uri RequestUri { get; set; }
        public IDictionary<string, string> Headers { get; }
        public IFeignClient FeignClient { get; }
    }
}
