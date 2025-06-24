using LogUtils.Threading;
using System;
using System.Collections.Generic;

namespace LogUtils.Diagnostics.Tools
{
    public class DeadlockTester
    {
        internal static DeadlockTester Instance;

        internal static void Run()
        {
            if (Instance == null)
            {
                UtilityLogger.Log("Deadlock checking enabled");
                Instance = new DeadlockTester();
            }
        }

        internal Dictionary<Lock, LockRecord> EventRecord = new Dictionary<Lock, LockRecord>();

        public DeadlockTester()
        {
            Lock.OnEvent += onLockEvent;
        }

        private void onLockEvent(Lock source, Lock.EventID eventID)
        {
            try
            {
                switch (eventID)
                {
                    case Lock.EventID.LockCreated:
                        createRecord(source);
                        break;
                    case Lock.EventID.WaitingToAcquire:
                        if (willDeadlockWhenLockIsTaken(source))
                        {
                            const string message = "!!!DEADLOCK CONDITIONS PRESENT!!!";

                            //Log to both in case one fails
                            UtilityLogger.DebugLog(message);
                            UtilityLogger.LogWarning(message);
                        }

                        EventRecord[source].RecordWaitEvent(); //Waiting on another thread
                        break;
                    case Lock.EventID.LockAcquired:
                        EventRecord[source].RecordAcquireEvent(); //Acquired a lock
                        break;
                    case Lock.EventID.LockReleased:
                        EventRecord[source].RecordReleaseEvent(); //Released a lock
                        break;
                }
            }
            catch
            {
                UtilityLogger.LogError("Record not available");
            }
        }

        private void createRecord(Lock key)
        {
            EventRecord[key] = new LockRecord();
        }

        private bool willDeadlockWhenLockIsTaken(Lock pendingLock)
        {
            LockRecord record = EventRecord[pendingLock];

            int lockHolder = record.Holder;

            //Determine if lock holder is waiting on another lock
            LockRecord waitRecord = findWaitRecord(lockHolder);

            if (waitRecord != null)
            {
                //Check that waiting thread is waiting on a lock taken by the current thread
                int currentThreadID = Environment.CurrentManagedThreadId;

                bool currentThreadHoldsLock = currentThreadID == waitRecord.Holder;
                return currentThreadHoldsLock;
            }

            //The thread holding the pending lock isn't waiting on any other relevant locks
            return false;
        }

        private LockRecord findWaitRecord(int threadID)
        {
            LockRecord foundEntry = null;
            foreach (var entry in EventRecord)
            {
                if (entry.Value.WaitingOnRelease.Contains(threadID))
                {
                    foundEntry = entry.Value;
                    break;
                }
            }
            return foundEntry;
        }

        internal record LockRecord
        {
            public int Holder;
            public List<int> WaitingOnRelease = new List<int>();

            public void RecordWaitEvent()
            {
                WaitingOnRelease.Add(Environment.CurrentManagedThreadId);
            }

            public void RecordAcquireEvent()
            {
                Holder = Environment.CurrentManagedThreadId;
                WaitingOnRelease.Remove(Holder);
            }

            public void RecordReleaseEvent()
            {
                Holder = -1;
            }
        }
    }
}
