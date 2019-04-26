using Feign;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class FeignClientPipelineBuilderExtensions
    {
        /// <summary>
        /// Gets the specified service Pipeline
        /// </summary>
        /// <param name="globalFeignClientPipelineBuilder"></param>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public static IFeignClientPipelineBuilder Service(this IGlobalFeignClientPipelineBuilder globalFeignClientPipelineBuilder, string serviceId)
        {
            if (string.IsNullOrWhiteSpace(serviceId))
            {
                throw new ArgumentException(nameof(serviceId));
            }
            GlobalFeignClientPipelineBuilder pipelineBuilder = globalFeignClientPipelineBuilder as GlobalFeignClientPipelineBuilder;
            if (pipelineBuilder == null)
            {
                throw new NotSupportedException();
            }
            return pipelineBuilder.GetOrAddServicePipeline(serviceId);
        }

        #region Authorization
        public static T Authorization<T>(this T feignClientPipeline, AuthenticationHeaderValue authenticationHeaderValue) where T : IFeignClientPipelineBuilder
        {
            if (authenticationHeaderValue == null)
            {
                throw new ArgumentNullException(nameof(authenticationHeaderValue));
            }
            feignClientPipeline.BuildingRequest += (sender, e) =>
            {
                if (!e.Headers.ContainsKey("Authorization"))
                {
                    e.Headers["Authorization"] = authenticationHeaderValue.Scheme + " " + authenticationHeaderValue.Parameter;
                }
            };
            return feignClientPipeline;
        }
        public static T Authorization<T>(this T feignClientPipeline, Func<IFeignClient, AuthenticationHeaderValue> authenticationHeaderValueAction) where T : IFeignClientPipelineBuilder
        {
            if (authenticationHeaderValueAction == null)
            {
                throw new ArgumentNullException(nameof(authenticationHeaderValueAction));
            }
            feignClientPipeline.BuildingRequest += (sender, e) =>
            {
                if (!e.Headers.ContainsKey("Authorization"))
                {
                    var authenticationHeaderValue = authenticationHeaderValueAction.Invoke(e.FeignClient);
                    e.Headers["Authorization"] = authenticationHeaderValue.Scheme + " " + authenticationHeaderValue.Parameter;
                }
            };
            return feignClientPipeline;
        }
        public static T Authorization<T>(this T feignClientPipeline, string scheme, string parameter) where T : IFeignClientPipelineBuilder
        {
            if (scheme == null)
            {
                throw new ArgumentNullException(nameof(scheme));
            }
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            feignClientPipeline.BuildingRequest += (sender, e) =>
            {
                if (!e.Headers.ContainsKey("Authorization"))
                {
                    e.Headers["Authorization"] = scheme + " " + parameter;
                }
            };
            return feignClientPipeline;
        }
        public static T Authorization<T>(this T feignClientPipeline, Func<IFeignClient, (string, string)> schemeAndParameterAction) where T : IFeignClientPipelineBuilder
        {
            if (schemeAndParameterAction == null)
            {
                throw new ArgumentNullException(nameof(schemeAndParameterAction));
            }
            feignClientPipeline.BuildingRequest += (sender, e) =>
            {
                if (!e.Headers.ContainsKey("Authorization"))
                {
                    var schemeAndParameter = schemeAndParameterAction.Invoke(e.FeignClient);
                    e.Headers["Authorization"] = schemeAndParameter.Item1 + " " + schemeAndParameter.Item2;
                }
            };
            return feignClientPipeline;
        }
        #endregion

    }
}
