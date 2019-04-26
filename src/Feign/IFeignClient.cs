using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    public interface IFeignClient
    {
        string ServiceId { get; }
    }
}
