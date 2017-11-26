using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maglev.Monorail.Scheduler
{
    internal class JobBatch
    {
        public Int32 GameTime { get; set; }

        public List<Job> Tasks = new List<Job>(42);

        public long TotalEstimatedExecutionTime()
        {
            long rv = 0;
            for (var i = 0; i < Tasks.Count; i++)
            {
                rv += Tasks[i].LastKnownExecutionTime;
            }
            return rv;
        }
    }
}
