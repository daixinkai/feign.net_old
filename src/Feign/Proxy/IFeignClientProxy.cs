using System;
using System.Collections.Generic;
using System.Text;

namespace Feign.Proxy
{
    public interface IFeignClientProxy
    {
        string ServiceId { get; }
    }
}
