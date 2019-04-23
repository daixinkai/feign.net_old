using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Discovery
{
    public static class ServiceDiscoveryExtensions
    {
        public static async Task<IList<IServiceInstance>> GetInstancesWithCacheAsync(this IServiceDiscovery serviceDiscovery, string serviceId, IDistributedCache distributedCache, string serviceInstancesKeyPrefix = "ServiceDiscovery-ServiceInstances-")
        {
            // if distributed cache was provided, just make the call back to the provider
            if (distributedCache != null)
            {
                // check the cache for existing service instances
                var instanceData = await distributedCache.GetAsync(serviceInstancesKeyPrefix + serviceId);
                if (instanceData != null && instanceData.Length > 0)
                {
                    return DeserializeFromCache<List<SerializableIServiceInstance>>(instanceData).ToList<IServiceInstance>();
                }
            }

            // cache not found or instances not found, call out to the provider
            var instances = serviceDiscovery.GetInstances(serviceId);
            if (distributedCache != null)
            {
                await distributedCache.SetAsync(serviceInstancesKeyPrefix + serviceId, SerializeForCache(MapToSerializable(instances)));
            }

            return instances;
        }

        public static IList<IServiceInstance> GetInstancesWithCache(this IServiceDiscovery serviceDiscovery, string serviceId, IDistributedCache distributedCache, string serviceInstancesKeyPrefix = "ServiceDiscovery-ServiceInstances-")
        {
            // if distributed cache was provided, just make the call back to the provider
            if (distributedCache != null)
            {
                // check the cache for existing service instances
                var instanceData = distributedCache.Get(serviceInstancesKeyPrefix + serviceId);
                if (instanceData != null && instanceData.Length > 0)
                {
                    return DeserializeFromCache<List<SerializableIServiceInstance>>(instanceData).ToList<IServiceInstance>();
                }
            }

            // cache not found or instances not found, call out to the provider
            var instances = serviceDiscovery.GetInstances(serviceId);
            if (distributedCache != null)
            {
                distributedCache.Set(serviceInstancesKeyPrefix + serviceId, SerializeForCache(MapToSerializable(instances)));
            }

            return instances;
        }

        private static List<SerializableIServiceInstance> MapToSerializable(IList<IServiceInstance> instances)
        {
            var inst = instances.Select(i => new SerializableIServiceInstance(i));
            return inst.ToList();
        }

        private static byte[] SerializeForCache(object data)
        {
            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, data);
                return stream.ToArray();
            }
        }

        private static T DeserializeFromCache<T>(byte[] data)
            where T : class
        {
            using (var stream = new MemoryStream(data))
            {
                return new BinaryFormatter().Deserialize(stream) as T;
            }
        }
    }
}
