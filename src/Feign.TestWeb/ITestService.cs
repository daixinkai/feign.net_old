using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Feign.TestWeb
{
    [FeignClient("yun-platform-service-provider")]
    [RequestMapping("/organizations")]
    public interface ITestService
    {
        [RequestMapping("/{id}", Method = "GET")]
        Task<JObject> GetValueAsync([PathVariable("id")]string id);
    }
}
