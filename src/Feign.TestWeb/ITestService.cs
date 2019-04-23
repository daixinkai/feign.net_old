using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Feign.TestWeb
{
    //[FeignClient("yun-platform-service-provider")]
    [FeignClient("http://localhost:8802/")]
    [RequestMapping("/organizations")]
    public interface ITestService
    {
        [RequestMapping("/{id}", Method = "GET")]
        Task<JObject> GetValueAsync([PathVariable("id")]string id);
        [RequestMapping("/{id}?test={test}", Method = "GET")]
        Task<JObject> GetValueAsync([PathVariable]int id, [RequestParam] string test);
    }
}
