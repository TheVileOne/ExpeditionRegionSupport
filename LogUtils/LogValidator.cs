using System.IO;

namespace LogUtils
{
    public class LogValidator
    {
        public FileInfo SourceFile;
        public FileInfo DestinationFile;

        internal string UnvalidatedSourcePath, UnvalidatedDestinationPath;

        public LogValidator(string sourceLogPath, string destLogPath)
        {
            UnvalidatedSourcePath = sourceLogPath;
            UnvalidatedDestinationPath = destLogPath;
        }

        public bool Validate()
        {
            string sourcePath = UnvalidatedSourcePath ?? SourceFile.FullName;
            string destPath = UnvalidatedDestinationPath ?? DestinationFile.FullName;

            UnvalidatedSourcePath = UnvalidatedDestinationPath = null;
            SourceFile = DestinationFile = null;

            if (!IsValidLogFileExt(sourcePath)) return false; //We don't want to handle random filetypes

            //A valid filetype is all we need to validate the source path
            SourceFile = new FileInfo(sourcePath);

            //Should we treat it as a directory, or a file
            if (Path.HasExtension(destPath))
            {
                string destFilename = Path.GetFileName(destPath);

                if (!ExtensionsMatch(SourceFile.Name, destFilename) && !IsValidLogFileExt(destFilename))
                    return false; //We can only replace log files

                DestinationFile = new FileInfo(destPath);
            }
            else
            {
                DestinationFile = new FileInfo(Path.Combine(destPath, SourceFile.Name));
            }

            return true;
        }

        /// <summary>
        /// Returns true if filename has either .log, or .txt as a file extension
        /// </summary>
        public static bool IsValidLogFileExt(string filename)
        {
            string fileExt = Path.GetExtension(filename); //Case-insensitive file extensions not supported

            return fileExt == ".log" || fileExt == ".txt";
        }

        public static bool ExtensionsMatch(string filename, string filename2)
        {
            string fileExt = Path.GetExtension(filename);
            string fileExt2 = Path.GetExtension(filename2);

            return fileExt == fileExt2;
        }
    }
}
