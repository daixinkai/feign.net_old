using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Feign
{
    public sealed class SendingRequestEventArgs : EventArgs
    {
        internal SendingRequestEventArgs(IFeignClient feignClient, HttpRequestMessage requestMessage)
        {
            RequestMessage = requestMessage;
            FeignClient = feignClient;
        }
        public HttpRequestMessage RequestMessage { get; }
        public IFeignClient FeignClient { get; }
    }
}
