using LogUtils.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace LogUtils
{
    /// <summary>
    /// Stores two versions of a log path allowing logs to be moved between two stored locations
    /// </summary>
    public class LogFileSwitcher
    {
        public PathSwitchMode SwitchMode;

        /// <summary>
        /// Determines which side of the KeyValuePair to move from. True means left.
        /// </summary>
        public bool SwitchStartPosition = true;

        public List<ValuePairToggle> PathStrings = new List<ValuePairToggle>();

        public LogFileSwitcher(PathSwitchMode mode)
        {
            SwitchMode = mode;
        }

        public void AddPaths(string path1, string path2)
        {
            ValuePairToggle valuePair = new ValuePairToggle(path1, path2);

            if (SwitchMode == PathSwitchMode.Collective)
                valuePair.ToggleFlag = SwitchStartPosition;

            PathStrings.Add(valuePair);
        }

        public void AddPaths(string path1, string path2, bool toggleValue)
        {
            if (SwitchMode == PathSwitchMode.Collective)
                throw new InvalidOperationException();

            PathStrings.Add(new ValuePairToggle(path1, path2)
            {
                ToggleFlag = toggleValue
            });
        }

        /// <summary>
        /// Moves all files to their alternate location at once
        /// </summary>
        public void SwitchPaths()
        {
            if (SwitchMode == PathSwitchMode.Singular)
                throw new InvalidOperationException();

            string path1, path2;
            foreach (ValuePairToggle valuePair in PathStrings)
            {
                //Retrieve our paths
                path1 = valuePair.ActiveValue;
                path2 = valuePair.InactiveValue;

                //If last move was unsuccessful, it is okay to do nothing here 
                if (valuePair.ToggleFlag == SwitchStartPosition)
                {
                    valuePair.Status = Helpers.LogUtils.MoveLog(path1, path2);

                    //Don't allow file position to get desynced with toggle position due to move fail
                    if (valuePair.LastMoveSuccessful)
                        valuePair.Toggle();
                }
            }

            SwitchStartPosition = !SwitchStartPosition;
        }

        /// <summary>
        /// Searches for a matching path, and attempts to move log file to that path
        /// </summary>
        public void SwitchToPath(string path)
        {
            if (SwitchMode == PathSwitchMode.Collective)
                throw new InvalidOperationException();

            //Retrieve our path
            ValuePairToggle valuePair = PathStrings.Find(vp => vp.ActiveValue == path || vp.InactiveValue == path);

            if (valuePair != null && valuePair.ActiveValue != path)
            {
                valuePair.Status = Helpers.LogUtils.MoveLog(valuePair.ActiveValue, path);

                //Don't allow file position to get desynced with toggle position due to move fail
                if (valuePair.LastMoveSuccessful)
                    valuePair.Toggle();
            }
        }

        /// <summary>
        /// Changes the directory of all paths on the right side of the value toggles
        /// </summary>
        public void UpdateTogglePath(string pathDir)
        {
            foreach (ValuePairToggle valuePair in PathStrings)
                valuePair.ValuePair = new KeyValuePair<string, string>(valuePair.ValuePair.Key, Path.Combine(pathDir, Path.GetFileName(valuePair.ValuePair.Value)));
        }

        public class ValuePairToggle
        {
            /// <summary>
            /// Determines which side of the KeyValuePair to move from. True means left.
            /// </summary>
            public bool ToggleFlag = true;

            /// <summary>
            /// Stores data for ValuePairToggle
            /// </summary>
            public KeyValuePair<string, string> ValuePair;

            public string ActiveValue => ToggleFlag ? ValuePair.Key : ValuePair.Value;
            public string InactiveValue => ToggleFlag ? ValuePair.Value : ValuePair.Key;

            /// <summary>
            /// The status of the last file move attempt. This is stored in case a failed move creates a desync
            /// </summary>
            public FileStatus Status = FileStatus.AwaitingStatus;

            public bool LastMoveSuccessful => Status != FileStatus.Error && Status != FileStatus.ValidationFailed;

            public ValuePairToggle(string value1, string value2)
            {
                ValuePair = new KeyValuePair<string, string>(value1, value2);
            }

            public void Toggle()
            {
                ToggleFlag = !ToggleFlag;
            }
        }

        public enum PathSwitchMode
        {
            Singular,
            Collective
        }
    }
}
