using Feign.Discovery;
using Feign.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Proxy
{
    public abstract class FeignClientProxyService : IDisposable
    {
        public FeignClientProxyService(IServiceDiscovery serviceDiscovery, IDistributedCache distributedCache, ILogger logger)
        {
            ServiceDiscoveryHttpClientHandler serviceDiscoveryHttpClientHandler = new ServiceDiscoveryHttpClientHandler(serviceDiscovery, distributedCache, logger);
            _httpClient = new HttpClient(serviceDiscoveryHttpClientHandler);
            string baseUrl = ServiceId ?? "";
            if (!baseUrl.StartsWith("http"))
            {
                baseUrl = $"http://{baseUrl}";
            }
            if (!string.IsNullOrWhiteSpace(BaseUri))
            {
                if (BaseUri.StartsWith("/"))
                {
                    baseUrl += BaseUri;
                }
                else
                {
                    baseUrl += "/" + BaseUri;
                }
            }

            if (baseUrl.EndsWith("/"))
            {
                baseUrl = baseUrl.TrimEnd('/');
            }
            _baseUrl = baseUrl;
        }

        public abstract string ServiceId { get; }

        public virtual string BaseUri { get { return null; } }

        string _baseUrl;


        private HttpClient _httpClient;

        protected HttpClient HttpClient
        {
            get
            {
                return _httpClient;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                _httpClient.Dispose();
                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        //~FeignClientServiceBase()
        //{
        //    // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //    Dispose(false);
        //}

        // 添加此代码以正确实现可处置模式。
        void IDisposable.Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            //GC.SuppressFinalize(this);
        }
        #endregion


        #region HttpMethod

        string BuildUri(string uri)
        {
            if (uri.StartsWith("/"))
            {
                return _baseUrl + uri;
            }
            return _baseUrl + "/" + uri;
        }

        #region get
        protected TResult Get<TResult>(string uri)
        {
            HttpResponseMessage response = _httpClient.GetAsync(BuildUri(uri)).GetResult();
            response.EnsureSuccessStatusCode();
            return GetResult<TResult>(response);
        }

        protected async Task<TResult> GetAsync<TResult>(string uri)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(BuildUri(uri));
            response.EnsureSuccessStatusCode();
            return await GetResultAsync<TResult>(response);
        }
        #endregion

        #region post
        protected TResult Post<TResult>(string uri, object value)
        {
            using (HttpContent httpContent = new ObjectStringContent(value))
            {
                HttpResponseMessage response = PostMessage(_httpClient, BuildUri(uri), value);
                response.EnsureSuccessStatusCode();
                return GetResult<TResult>(response);
            }
        }

        protected async Task<TResult> PostAsync<TResult>(string uri, object value)
        {
            using (HttpContent httpContent = new ObjectStringContent(value))
            {
                HttpResponseMessage response = await PostMessageAsync(_httpClient, BuildUri(uri), value);
                response.EnsureSuccessStatusCode();
                return await GetResultAsync<TResult>(response);
            }
        }

        static HttpResponseMessage PostMessage(HttpClient httpClient, string uri, object value)
        {
            if (value is HttpContent)
            {
                return httpClient.PostAsync(uri, (HttpContent)value).GetResult();
            }
            else
            {
                return httpClient.PostAsync(uri, new ObjectContent(value)).GetResult();
            }
        }

        async static Task<HttpResponseMessage> PostMessageAsync(HttpClient httpClient, string uri, object value)
        {
            if (value is HttpContent)
            {
                return await httpClient.PostAsync(uri, (HttpContent)value);
            }
            else
            {
                return await httpClient.PostAsync(uri, new ObjectContent(value));
            }
        }

        #endregion

        TResult GetResult<TResult>(HttpResponseMessage responseMessage)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TResult>(responseMessage.Content.ReadAsStringAsync().GetResult());
        }

        async Task<TResult> GetResultAsync<TResult>(HttpResponseMessage responseMessage)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TResult>(await responseMessage.Content.ReadAsStringAsync());
        }

        #endregion



        #region PathVariable
        protected static string ReplacePathVariable<T>(string uri, string name, T value)
        {
            return FeignClientUtils.ReplacePathVariable<T>(uri, name, value);
        }
        #endregion

        #region RequestParam
        protected static string ReplaceRequestParam<T>(string uri, string name, T value)
        {
            return FeignClientUtils.ReplaceRequestParam<T>(uri, name, value);
        }
        #endregion

    }
}
