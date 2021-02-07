using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GDSync.core
{
    public class Guild
    {
        public DriveAction GetDriveAction(out bool status)
        {
            DriveAction action;
            status = ActionQueue.TryDequeue(out action);

            return action;
        }


        public void ReportDriveActions(List<DriveAction> actions, bool status = true)
        {
            foreach (var action in actions)
            {
                ActionQueue.Enqueue(action);
            }
        }


        public void AddAction(DriveAction action)
        {
            if (action.UsageLimit > 5)
            {
                DiscardActionQueue.Enqueue(action);
            }
            else
            {
                ActionQueue.Enqueue(action);
            }
        }


        public void AddAdventurer(Adventurer adventurer)
        {
            lock (adventurers_locker)
            {
                adventurers.Add(adventurer);
            }
        }


        public void RemoveAdventurer(string name)
        {
            lock (adventurers_locker)
            {
                adventurers.RemoveAll(s => (s.Name == name));
            }
        }


        public void SetExit()
        {
            lock (adventurers_locker)
            {
                foreach (var adventurer in adventurers)
                {
                    adventurer.IsExit = true;
                }
            }
        }


        public bool IsAllIdle()
        {
            bool status = true;
            if (!ActionQueue.IsEmpty)
            {
                return false;
            }
            foreach (var adventurer in adventurers)
            {
                if (adventurer.IsBusy)
                {
                    status = false;
                }
            }
            return status;
        }


        public void WaitForAllIdle(bool exit = true)
        {
            while (!IsAllIdle())
            {
                Thread.Sleep(200);
            }
            if (exit) SetExit();
        }


        public void WaitForStageOver()
        {
            while (!IsAllIdle())
            {
                Thread.Sleep(200);
            }
        }



        public int LayOff(int num)
        {
            if (num > adventurers.Count)
            {
                num = adventurers.Count;
            }
            lock (adventurers_locker)
            {
                for (int i = 0; i < num; i++)
                {
                    adventurers[0].IsExit = true;
                    adventurers.RemoveAt(0);
                }
            }
            return num;
        }


        public int LayOff(string name)
        {
            int index = adventurers.FindIndex(worker => (worker.Name == name));
            if (index == -1) return 0;
            lock (adventurers_locker)
            {
                adventurers[index].IsExit = true;
                adventurers.RemoveAt(index);
            }
            return 1;
        }


        public void Pause(bool status = true)
        {
            lock (adventurers_locker)
            {
                foreach (var adventurer in adventurers)
                {
                    adventurer.IsPause = status;
                }
            }
        }


        private List<Adventurer> adventurers = new List<Adventurer>();
        ConcurrentQueue<DriveAction> ActionQueue = new ConcurrentQueue<DriveAction>();
        ConcurrentQueue<DriveAction> DiscardActionQueue = new ConcurrentQueue<DriveAction>();
        public string Name { get; set; }
        public int Count { get { return ActionQueue.Count; } }

        // locker
        private Object adventurers_locker = new Object();
    }
}
