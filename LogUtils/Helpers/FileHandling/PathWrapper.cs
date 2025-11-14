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
            if (pathString == null) return;

            Filename = System.IO.Path.GetFileName(pathString); //No filename will return an empty string. No file extension will be treated as a filename not a directory

            bool containsPathInfo = Filename.Length != pathString.Length;
            if (containsPathInfo)
                Path = System.IO.Path.GetDirectoryName(pathString);
        }
    }
}
