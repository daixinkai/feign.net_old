using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.Discovery.Client;

namespace Feign.TestWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            //services.AddLogging();
            services.AddDiscoveryClient(Configuration);
            services.AddFeignClients(options =>
            {
                options.Lifetime = ServiceLifetime.Scoped;
                options.FeignClientPipeline.Authorization(proxy =>
                {
                    return ("global", "asdasd");
                });
                //options.FeignClientPipeline.BuildingRequest += FeignClientPipeline_BuildingRequest;
                options.FeignClientPipeline.Service("yun-platform-service-provider").BuildingRequest += (sender, e) =>
                {
                    if (!e.Headers.ContainsKey("Authorization"))
                    {
                        e.Headers["Authorization"] = "service asdasd";
                    }
                    e.Headers["Accept-Encoding"] = "gzip, deflate, br";
                };

                options.FeignClientPipeline.Service("yun-platform-service-provider").Authorization(proxy =>
                {
                    return ("service", "asdasd");
                });

                options.FeignClientPipeline.SendingRequest += FeignClientPipeline_SendingRequest;
            })
            //.AddDiscoveryClient();
            ;
        }

        private void FeignClientPipeline_SendingRequest(object sender, SendingRequestEventArgs e)
        {

        }

        static void Test()
        {
            string s = "";
            s.Replace("", "");
            return;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseDiscoveryClient();
        }
    }
}
