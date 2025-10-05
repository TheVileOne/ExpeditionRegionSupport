using LogUtils.Helpers.FileHandling;
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

            if (!FileExtension.IsSupported(sourcePath)) return false; //We don't want to handle random filetypes

            //A valid filetype is all we need to validate the source path
            SourceFile = new FileInfo(sourcePath);

            //Should we treat it as a directory, or a file
            if (Path.HasExtension(destPath))
            {
                string destFilename = Path.GetFileName(destPath);

                if (!FileExtension.Match(SourceFile.Name, destFilename) && !FileExtension.IsSupported(destFilename))
                    return false; //We can only replace log files

                DestinationFile = new FileInfo(destPath);
            }
            else
            {
                DestinationFile = new FileInfo(Path.Combine(destPath, SourceFile.Name));
            }

            return true;
        }
    }
}
