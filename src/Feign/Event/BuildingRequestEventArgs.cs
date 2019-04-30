using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    public sealed class BuildingRequestEventArgs : FeignClientEventArgs
    {
        internal BuildingRequestEventArgs(IFeignClient feignClient, string method, Uri requestUri, IDictionary<string, string> headers) : base(feignClient)
        {
            Method = method;
            RequestUri = requestUri;
            Headers = headers;
        }
        public string Method { get; }
        public Uri RequestUri { get; set; }
        public IDictionary<string, string> Headers { get; }
    }
}
