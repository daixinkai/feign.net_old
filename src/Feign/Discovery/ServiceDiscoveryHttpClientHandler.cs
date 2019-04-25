using Feign.Proxy;
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
        private FeignClientPipelineBuilder _feignClientPipeline;
        private IFeignClientProxy _feignClientProxy;
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDiscoveryHttpClientHandler"/> class.
        /// </summary>
        public ServiceDiscoveryHttpClientHandler(IServiceDiscovery serviceDiscovery, IFeignClientProxy feignClientProxy, IFeignClientPipelineBuilder feignClientPipeline, IDistributedCache distributedCache, ILogger logger)
        {
            _serviceResolve = new RandomServiceResolve(logger);
            _feignClientProxy = feignClientProxy;
            _feignClientPipeline = feignClientPipeline as FeignClientPipelineBuilder;
            _logger = logger;
            _serviceDiscovery = serviceDiscovery;
            _distributedCache = distributedCache;
            ShouldResolveService = true;
        }


        public bool ShouldResolveService { get; set; }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var current = request.RequestUri;
            try
            {
                var buildingArgs = _feignClientPipeline?.OnBuildingRequest(_feignClientProxy, request.Method.ToString(), request.RequestUri, new Dictionary<string, string>());
                if (buildingArgs != null)
                {
                    //request.Method = new HttpMethod(buildingArgs.Method);
                    request.RequestUri = buildingArgs.RequestUri;
                    if (buildingArgs.Headers != null && buildingArgs.Headers.Count > 0)
                    {
                        foreach (var item in buildingArgs.Headers)
                        {
                            request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                        }
                    }
                }
                request.RequestUri = LookupService(request.RequestUri);
                var sendingArgs = _feignClientPipeline?.OnSendingRequest(_feignClientProxy, request);
                var sendingRequest = request;
                if (sendingArgs != null)
                {
                    sendingRequest = sendingArgs.RequestMessage;
                }
                return await base.SendAsync(sendingRequest, cancellationToken);
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
            if (!ShouldResolveService)
            {
                return uri;
            }
            if (_serviceDiscovery == null)
            {
                return uri;
            }
            IList<IServiceInstance> services = _serviceDiscovery.GetServiceInstancesWithCache(uri.Host, _distributedCache);
            return _serviceResolve.ResolveService(uri, services);
        }

    }
}
