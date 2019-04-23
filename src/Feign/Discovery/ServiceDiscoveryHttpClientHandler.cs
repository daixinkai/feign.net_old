using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Feign.Discovery
{
    public class ServiceDiscoveryHttpClientHandler : HttpClientHandler
    {
        private readonly ILogger _logger;
        private IServiceResolve _serviceResolve;
        private IServiceDiscovery _serviceDiscovery;
        private IDistributedCache _distributedCache;
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryHttpClientHandler"/> class.
        /// </summary>
        /// <param name="discoveryClient">Service discovery client to use - provided by calling services.AddDiscoveryClient(Configuration)</param>
        /// <param name="logger">ILogger for capturing logs from Discovery operations</param>
        public ServiceDiscoveryHttpClientHandler(IServiceDiscovery serviceDiscovery, IDistributedCache distributedCache, ILogger logger)
        {
            _serviceResolve = new RandomServiceResolve(logger);
            _logger = logger;
            _serviceDiscovery = serviceDiscovery;
            _distributedCache = distributedCache;
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var current = request.RequestUri;
            try
            {
                request.RequestUri = LookupService(request.RequestUri);
                return await base.SendAsync(request, cancellationToken);
            }
            catch (Exception e)
            {
                _logger?.LogDebug(e, "Exception during SendAsync()");
                throw;
            }
            finally
            {
                request.RequestUri = current;
            }
        }


        Uri LookupService(Uri uri)
        {
            IList<IServiceInstance> services = _serviceDiscovery?.GetInstancesWithCache(uri.Host, _distributedCache);
            return _serviceResolve.ResolveService(uri, services);
        }

    }
}
