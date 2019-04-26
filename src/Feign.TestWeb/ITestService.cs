using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Feign.TestWeb
{
    [FeignClient("yun-platform-service-provider", Url = "http://localhost:8802/")]
    [RequestMapping("/organizations")]
    public interface ITestService
    {
        [RequestMapping("/{id}", Method = "GET")]
        Task<JObject> GetValueAsync([PathVariable("id")]string id);
        [RequestMapping("/{id}", Method = "GET")]
        Task<JObject> GetValueAsync([PathVariable]int id, [RequestParam] string test);
        [GetMapping("/{id}")]
        Task<JObject> GetValueAsync([PathVariable]int id, [RequestQuery] TestServiceParam param);
        [GetMapping("/{id}")]
        void GetValueAsync([PathVariable]int id, [RequestParam] string test, [RequestQuery] TestServiceParam param);
    }



}
