using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

namespace Zyowo
{
    public class ThreadManager : DontDestroyMonoSingleton<ThreadManager>
    {
        public class DelayedAction
        {
            public float timeMarker = 0;
            public Action delayedAction = null;
            public DelayedAction(Action delayedAction, float timeMarker)
            {
                this.delayedAction = delayedAction;
                this.timeMarker = timeMarker;
            }
        }

        public static int maxThreads = 10;
        public static int threadCounter = 0;
        public static bool exist = false;
        public static ThreadManager threadManagerHolder;
        private List<Action> tempActions = new List<Action>();
        private List<Action> currentActions = new List<Action>();
        private List<DelayedAction> delayedActions = new List<DelayedAction>();
        private List<DelayedAction> currentDelayedActions = new List<DelayedAction>();

        /// <summary>
        /// 执行 Unity 主线程任务
        /// </summary>
        /// <param name="inputAction">主线程任务</param>
        /// <param name="delayedTime">延迟时间</param>
        public void RunUnityAction(Action inputAction, float delayedTime = 0)
        {
            if (delayedTime != 0)
            {
                lock (delayedActions)
                {
                    delayedActions.Add(new DelayedAction(
                            inputAction, delayedTime += Time.time
                    ));
                }
            }
            else
            {
                lock (tempActions)
                {
                    tempActions.Add(inputAction);
                }
            }
        }

        /// <summary>
        /// 线程池中执行后台任务
        /// </summary>
        /// <param name="inputAction">需要执行的任务</param>
        /// <returns></returns>
        public Thread RunOrginalAction(Action inputAction)
        {
            while (threadCounter >= maxThreads)
                Thread.Sleep(1);
            Interlocked.Increment(ref threadCounter);
            ThreadPool.QueueUserWorkItem(RunAction, inputAction);
            return null;
        }

        private void RunAction(object action)
        {
            try
            {
                ((Action)action)();
            }
            catch
            {
            }
            Interlocked.Decrement(ref threadCounter);
        }

        // Update is called once per frame
        void Update()
        {
            ThreadManagerLoop();
        }

        public void ThreadManagerLoop()
        {
            lock (tempActions)
            {
                currentActions.Clear();
                currentActions.AddRange(tempActions);
                tempActions.Clear();
            }
            for (int i = 0; i < currentActions.Count; i++)
                currentActions[i]();
            lock (delayedActions)
            {
                currentDelayedActions.Clear();
                for (int i = 0; i < delayedActions.Count; i++)
                {
                    if (delayedActions[i].timeMarker <= Time.time)
                        currentDelayedActions.Add(delayedActions[i]);
                }
                for (int i = 0; i < currentDelayedActions.Count; i++)
                    delayedActions.Remove(currentDelayedActions[i]);
            }
            for (int i = 0; i < currentDelayedActions.Count; i++)
                currentDelayedActions[i].delayedAction();

        }
    }
}
