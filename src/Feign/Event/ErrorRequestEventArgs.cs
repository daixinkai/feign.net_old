using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    public sealed class ErrorRequestEventArgs : EventArgs
    {
        internal ErrorRequestEventArgs(IFeignClient feignClient, Exception exception)
        {
            Exception = exception;
            FeignClient = feignClient;
        }
        public Exception Exception { get; }
        public bool ExceptionHandled { get; set; }
        public IFeignClient FeignClient { get; }
    }
}
