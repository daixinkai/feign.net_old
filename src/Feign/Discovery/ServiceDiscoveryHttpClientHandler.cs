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
        private GlobalFeignClientPipelineBuilder _globalFeignClientPipeline;
        private IFeignClient _feignClient;
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDiscoveryHttpClientHandler"/> class.
        /// </summary>
        public ServiceDiscoveryHttpClientHandler(IServiceDiscovery serviceDiscovery, IFeignClient feignClient, IGlobalFeignClientPipelineBuilder globalFeignClientPipeline, IDistributedCache distributedCache, ILogger logger)
        {
            _serviceResolve = new RandomServiceResolve(logger);
            _feignClient = feignClient;
            _globalFeignClientPipeline = globalFeignClientPipeline as GlobalFeignClientPipelineBuilder;
            _logger = logger;
            _serviceDiscovery = serviceDiscovery;
            _distributedCache = distributedCache;
            ShouldResolveService = true;
        }


        public bool ShouldResolveService { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var current = request.RequestUri;
            ServiceFeignClientPipelineBuilder serviceFeignClientPipeline = _globalFeignClientPipeline?.GetServicePipeline(_feignClient.ServiceId);
            try
            {

                #region BuildingRequest
                BuildingRequestEventArgs buildingArgs = new BuildingRequestEventArgs(_feignClient, request.Method.ToString(), request.RequestUri, new Dictionary<string, string>());

                serviceFeignClientPipeline?.OnBuildingRequest(_feignClient, buildingArgs);
                _globalFeignClientPipeline?.OnBuildingRequest(_feignClient, buildingArgs);
                //request.Method = new HttpMethod(buildingArgs.Method);
                request.RequestUri = buildingArgs.RequestUri;
                if (buildingArgs.Headers != null && buildingArgs.Headers.Count > 0)
                {
                    foreach (var item in buildingArgs.Headers)
                    {
                        request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                    }
                }
                #endregion
                request.RequestUri = LookupService(request.RequestUri);
                #region SendingRequest
                SendingRequestEventArgs sendingArgs = new SendingRequestEventArgs(_feignClient, request);
                serviceFeignClientPipeline?.OnSendingRequest(_feignClient, sendingArgs);
                _globalFeignClientPipeline?.OnSendingRequest(_feignClient, sendingArgs);
                request = sendingArgs.RequestMessage;
                #endregion

                #region CannelRequest
                CancelRequestEventArgs cancelArgs = new CancelRequestEventArgs(_feignClient, cancellationToken);
                serviceFeignClientPipeline?.OnCancelRequest(_feignClient, cancelArgs);
                _globalFeignClientPipeline?.OnCancelRequest(_feignClient, cancelArgs);
                #endregion

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
