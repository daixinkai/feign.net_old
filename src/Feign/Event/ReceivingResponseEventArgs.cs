using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Feign
{
    public class ReceivingResponseEventArgs : FeignClientEventArgs
    {
        internal ReceivingResponseEventArgs(IFeignClient feignClient, HttpResponseMessage responseMessage, Type resultType) : base(feignClient)
        {
            ResponseMessage = responseMessage;
            ResultType = resultType;
        }
        public HttpResponseMessage ResponseMessage { get; }

        public Type ResultType { get; }

        public virtual object Result { get; set; }
    }

    public sealed class ReceivingResponseEventArgs<T> : ReceivingResponseEventArgs
    {
        internal ReceivingResponseEventArgs(IFeignClient feignClient, HttpResponseMessage responseMessage) : base(feignClient, responseMessage, typeof(T))
        {
        }

        public override object Result { get => base.Result; set => base.Result = value; }

    }
}
