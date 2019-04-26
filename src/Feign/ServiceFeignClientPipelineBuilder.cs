using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    class ServiceFeignClientPipelineBuilder : IFeignClientPipelineBuilder
    {
        public ServiceFeignClientPipelineBuilder(string serviceId)
        {
            _serviceId = serviceId;
        }
        public event EventHandler<BuildingRequestEventArgs> BuildingRequest;
        public event EventHandler<SendingRequestEventArgs> SendingRequest;

        string _serviceId;

        internal void OnBuildingRequest(object sender, BuildingRequestEventArgs e)
        {
            BuildingRequest?.Invoke(sender, e);
        }
        internal void OnSendingRequest(object sender, SendingRequestEventArgs e)
        {
            SendingRequest?.Invoke(sender, e);
        }

    }
}
