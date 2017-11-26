using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Maglev.Monorail.Scheduler;

namespace Loading_a_3D_model.Scheduler
{
    // This Scheduler is used for queing a bunch of work and then executing it
    // Might want some functionality around executing tasks straight away.

    public class JobScheduler
    {
        private Dictionary<Action, Job> m_AllTasks;
        private List<Job> m_ActiveTasks;
        private JobBatch[] m_JobBatches;
        private Int32 m_NumThreads;

        private int m_Count;        // Used for syncronization
        private int m_NextTaskId;   // Used to assign Ids to tasks

        /// <summary>
        /// Uses default number of processors
        /// </summary>
        public JobScheduler()
        {
            Initalize(Environment.ProcessorCount);
        }

        public JobScheduler(Int32 numberOfThreads)
        {
            Initalize(numberOfThreads);
        }

        private void Initalize(Int32 numberOfThreads)
        {
            if (numberOfThreads <= 0)
                throw new ArgumentException("Number of threads must be greater than 0");

            m_NumThreads = numberOfThreads;
            m_AllTasks = new Dictionary<Action, Job>();
            m_ActiveTasks = new List<Job>(42);
            m_JobBatches = new JobBatch[m_NumThreads];

            for (int i = 0; i < m_JobBatches.Length; i++)
            {
                m_JobBatches[i] = new JobBatch();
            }
        }

        public void QueueJob(String name, Action action)
        {
            lock (this) // Check if already in the task list, Otherwise create the task
            {
                if (m_AllTasks.ContainsKey(action))
                {
                    m_ActiveTasks.Add(m_AllTasks[action]);
                }
                else
                {
                    var task = new Job { Action = action, Name = name };
                    m_NextTaskId++;
                    task.Id = m_NextTaskId;
                    m_AllTasks.Add(action, task);
                    m_ActiveTasks.Add(task);
                }
            }
        }

        public void ExecuteSerial(Action callback = null)
        {
            foreach (var task in m_ActiveTasks)
            {
                task.Action.Invoke();
            }

            Reset();

            if (callback != null)
                callback.Invoke();
        }

        public void ExecuteAll(Action callback = null)
        {
           // using (Profiler.LogTime("Executing All Jobs: Threads: {0}", m_NumThreads))
            {
                if (m_NumThreads == 1)
                {
                    ExecuteSerial();
                    if (callback != null)
                        callback.Invoke();
                    return;
                }

                ExecuteParallel();

                if (callback != null)
                    callback.Invoke();
            }
        }

        private void ExecuteParallel()
        {
            // Sort Active tasks by last known execution time
            // TODO Insertion sort might be better as to minimize potential garbage collection but haven't tested or maybe use a priority queue when queuing tasks
            m_ActiveTasks.Sort((a, b) => b.LastKnownExecutionTime.CompareTo(a.LastKnownExecutionTime));

            // Assign all tasks to batches. Assign each task to a batch with the least estimated execution time.
            for (int i = 0; i < m_ActiveTasks.Count; i++)
            {
                var currentBatch = m_JobBatches[0];
                for (int j = 1; j < m_JobBatches.Length; j++)
                {
                    if (m_JobBatches[j].TotalEstimatedExecutionTime() < currentBatch.TotalEstimatedExecutionTime())
                        currentBatch = m_JobBatches[j];
                }
                currentBatch.Tasks.Add(m_ActiveTasks[i]);
            }


            var syncronization = new List<ManualResetEvent>(16);

            // Execute all batches
            for (int i = 0; i < m_JobBatches.Length; i++)
            {
                if (m_JobBatches[i].Tasks.Count > 0)
                {
                    syncronization.Add(new ManualResetEvent(false));
                    var x = i;
                    Interlocked.Increment(ref m_Count);
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                    //    using (Profiler.LogTime("Executing Job Batch: {0}", x))
                        {
                            Execute(m_JobBatches[x]);
                            syncronization[x].Set();
                        }
                    });
                }
            }

            for (int i = 0; i < syncronization.Count; i++)
            {
                syncronization[i].WaitOne();
            }

            Reset();
        }

        private void Reset()
        {
            m_ActiveTasks.Clear();
            for (int j = 0; j < m_JobBatches.Length; j++)
            {
                m_JobBatches[j].Tasks.Clear();
            }
        }

        private void Execute(Object obj)
        {
            var batch = (JobBatch)obj;
            for (int i = 0; i < batch.Tasks.Count; i++)
            {
               // using (Profiler.LogTime("Executing Job:{0}", batch.Tasks[i].Name))
                {
                    var startTime = Stopwatch.GetTimestamp();
                    batch.Tasks[i].Action.Invoke();
                    var time = Stopwatch.GetTimestamp() - startTime;
                    batch.Tasks[i].LastKnownExecutionTime = time;
                }
            }
            Interlocked.Decrement(ref m_Count);
        }
    }
}
