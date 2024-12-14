namespace LogUtils.Helpers.FileHandling
{
    /// <summary>
    /// Simple wrapper for a path string
    /// </summary>
    internal struct PathWrapper
    {
        public string Filename;
        public string Path;

        public PathWrapper(string pathString)
        {
            pathString = pathString?.Trim('\\');

            Filename = System.IO.Path.GetFileName(pathString);

            //There is more than just the filename if this is true
            if (Filename != null && Filename.Length != pathString.Length)
                Path = System.IO.Path.GetDirectoryName(pathString);
        }
    }
}
