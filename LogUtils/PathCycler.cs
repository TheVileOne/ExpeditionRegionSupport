using LogUtils.Helpers.FileHandling;
using System;

namespace LogUtils
{
    public class PathCycler
    {
        public string Result;

        public virtual void CycleNext(LogsFolderAccessToken accessToken)
        {
            string basePath = LogsFolder.Path;

            string[] allowedPaths = accessToken.AllowedPaths;
            string result = null;

            try
            {
                if (basePath == null)
                {
                    result = allowedPaths.Length > 0 ? allowedPaths[0] : null;
                    return;
                }

                for (int i = 0; i < allowedPaths.Length; i++)
                {
                    //Find the index of the base path in AllowedPaths array
                    if (PathUtils.PathsAreEqual(basePath, allowedPaths[i]))
                    {
                        //Wrap to start of array if we are at the end of it
                        result = i < allowedPaths.Length - 1 ? allowedPaths[i + 1] : allowedPaths[0];
                        break;
                    }
                }
            }
            finally
            {
                if (result == null)
                    throw new InvalidOperationException("PathCycler failed to find a suitable path");

                Result = result;
            }
        }

        public virtual void CyclePrev(LogsFolderAccessToken accessToken)
        {
            string basePath = LogsFolder.Path;

            string[] allowedPaths = accessToken.AllowedPaths;
            string result = null;

            try
            {
                if (basePath == null)
                {
                    result = allowedPaths.Length > 0 ? allowedPaths[0] : null;
                    return;
                }

                for (int i = 0; i < allowedPaths.Length; i++)
                {
                    //Find the index of the base path in AllowedPaths array
                    if (PathUtils.PathsAreEqual(basePath, allowedPaths[i]))
                    {
                        //Wrap to end of array if we are at the start of it
                        result = i > 0 ? allowedPaths[i - 1] : allowedPaths[allowedPaths.Length - 1];
                        break;
                    }
                }
            }
            finally
            {
                if (result == null)
                    throw new InvalidOperationException("PathCycler failed to find a suitable path");

                Result = result;
            }
        }
    }
}
