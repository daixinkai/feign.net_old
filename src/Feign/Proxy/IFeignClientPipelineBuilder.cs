﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Feign.Proxy
{
    public interface IFeignClientPipelineBuilder
    {
        event EventHandler<BuildingRequestEventArgs> BuildingRequest;
        event EventHandler<SendingRequestEventArgs> SendingRequest;
    }
}
