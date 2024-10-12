using LogUtils.Helpers;
using System.IO;

namespace LogUtils.Properties
{
    /// <summary>
    /// A class for reading, or writing to the LogProperties file
    /// </summary>
    internal class LogPropertyFile
    {
        public string FilePath;

        public LogPropertyReader Reader;
        public LogPropertyWriter Writer;

        public LogPropertyFile()
        {
            FilePath = Path.Combine(Paths.StreamingAssetsPath, "logs.txt");
            Reader = new LogPropertyReader(this);
            Writer = new LogPropertyWriter(this);
        }
    }
}
