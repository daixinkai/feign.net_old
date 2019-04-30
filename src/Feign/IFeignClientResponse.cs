using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Feign
{
    public interface IFeignClientResponse
    {
        HttpResponseMessage ResponseMessage { get; }
    }
}
