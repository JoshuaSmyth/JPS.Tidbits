using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Maglev.Monorail.Scheduler
{
    class Job
    {
        public Int32 Id;
        public Action Action;
        public long LastKnownExecutionTime = 2;

        public string Name = "";
    }
}
