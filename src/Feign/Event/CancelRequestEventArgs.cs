using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Feign
{
    public sealed class CancelRequestEventArgs : EventArgs
    {
        internal CancelRequestEventArgs(IFeignClient feignClient, CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            FeignClient = feignClient;
        }
        public CancellationToken CancellationToken { get; }
        public IFeignClient FeignClient { get; }
    }
}
