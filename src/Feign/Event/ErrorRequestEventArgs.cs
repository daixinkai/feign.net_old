using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    public sealed class ErrorRequestEventArgs : FeignClientEventArgs
    {
        internal ErrorRequestEventArgs(IFeignClient feignClient, Exception exception) : base(feignClient)
        {
            Exception = exception;
        }
        public Exception Exception { get; }
        public bool ExceptionHandled { get; set; }
    }
}
