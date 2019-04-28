using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    class FeignClientPipelineBuilderBase : IFeignClientPipelineBuilder
    {
        public virtual bool Enabled { get; set; } = true;
        public event EventHandler<BuildingRequestEventArgs> BuildingRequest;
        public event EventHandler<SendingRequestEventArgs> SendingRequest;
        public event EventHandler<CancelRequestEventArgs> CancelRequest;
        public event EventHandler<ErrorRequestEventArgs> ErrorRequest;
        public event EventHandler<ReceivingResponseEventArgs> ReceivingResponse;

        protected internal virtual void OnBuildingRequest(object sender, BuildingRequestEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }
            BuildingRequest?.Invoke(sender, e);
        }
        protected internal virtual void OnSendingRequest(object sender, SendingRequestEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }
            SendingRequest?.Invoke(sender, e);
        }
        protected internal virtual void OnCancelRequest(object sender, CancelRequestEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }
            CancelRequest?.Invoke(sender, e);
        }
        protected internal virtual void OnErrorRequest(object sender, ErrorRequestEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }
            ErrorRequest?.Invoke(sender, e);
        }
        protected internal virtual void OnReceivingResponse(object sender, ReceivingResponseEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }
            ReceivingResponse?.Invoke(sender, e);
        }

    }
}
