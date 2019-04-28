using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    public interface IFeignClientPipelineBuilder
    {
        bool Enabled { get; set; }
        event EventHandler<BuildingRequestEventArgs> BuildingRequest;
        event EventHandler<SendingRequestEventArgs> SendingRequest;
        event EventHandler<CancelRequestEventArgs> CancelRequest;
        event EventHandler<ErrorRequestEventArgs> ErrorRequest;
        event EventHandler<ReceivingResponseEventArgs> ReceivingResponse;
    }
}
