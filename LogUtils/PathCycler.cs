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

            if (basePath == null)
            {
                Result = accessToken.AllowedPaths[0];
                return;
            }

            string result = null;
            for (int i = 0; i < accessToken.AllowedPaths.Length; i++)
            {
                //Find the index of the base path in AllowedPaths array
                if (PathUtils.PathsAreEqual(basePath, accessToken.AllowedPaths[i]))
                {
                    //Wrap to start of array if we are at the end of it
                    result = i < accessToken.AllowedPaths.Length - 1 ? accessToken.AllowedPaths[i + 1] : accessToken.AllowedPaths[0];
                    break;
                }
            }

            if (result == null)
                throw new InvalidOperationException("PathCycler failed to find a suitable path");

            Result = result;
        }

        public virtual void CyclePrev(LogsFolderAccessToken accessToken)
        {
            string basePath = LogsFolder.Path;

            if (basePath == null)
            {
                Result = accessToken.AllowedPaths[0];
                return;
            }

            string result = null;
            for (int i = 0; i < accessToken.AllowedPaths.Length; i++)
            {
                //Find the index of the base path in AllowedPaths array
                if (PathUtils.PathsAreEqual(basePath, accessToken.AllowedPaths[i]))
                {
                    //Wrap to end of array if we are at the start of it
                    result = i > 0 ? accessToken.AllowedPaths[i - 1] : accessToken.AllowedPaths[accessToken.AllowedPaths.Length - 1];
                    break;
                }
            }

            if (result == null)
                throw new InvalidOperationException("PathCycler failed to find a suitable path");

            Result = result;
        }
    }
}
