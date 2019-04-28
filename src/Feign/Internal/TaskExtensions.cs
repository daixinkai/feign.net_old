using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Internal
{
    static class TaskExtensions
    {
        public static TResult GetResult<TResult>(this Task<TResult> task)
        {
            if (task.IsCompleted)
            {
                return task.Result;
            }
            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
