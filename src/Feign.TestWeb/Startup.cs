﻿using System;
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
                options.FeignClientPipeline.BuildingRequest += FeignClientPipeline_BuildingRequest;
                options.FeignClientPipeline.SendingRequest += FeignClientPipeline_SendingRequest;
            })
            //.AddDiscoveryClient();
            ;
        }

        private void FeignClientPipeline_BuildingRequest(object sender, Proxy.BuildingRequestEventArgs e)
        {
            e.Headers["Authorization"] = "test asdasd";
            e.Headers["Accept-Encoding"] = "gzip, deflate, br";
        }

        private void FeignClientPipeline_SendingRequest(object sender, Proxy.SendingRequestEventArgs e)
        {

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
