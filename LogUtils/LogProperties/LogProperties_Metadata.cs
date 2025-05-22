using LogUtils.Helpers.FileHandling;
using System;
using System.IO;

namespace LogUtils.Properties
{
    public partial class LogProperties
    {
        private bool _fileExists;
        private string _filename = string.Empty;
        private string _altFilename = string.Empty;
        private string _folderPath = string.Empty;
        private string _originalFolderPath = string.Empty;

        public bool FileExists
        {
            get => _fileExists;
            set
            {
                if (_fileExists == value) return;

                _fileExists = value;
                LogSessionActive = false; //A new session needs to apply when file is created or removed
            }
        }

        /// <summary>
        /// The active filename of the log file (without file extension)
        /// </summary>
        public string CurrentFilename { get; private set; }

        /// <summary>
        /// The active filename of the log file (with file extension)
        /// </summary>
        public string CurrentFilenameWithExtension => CurrentFilename + PreferredFileExt;

        /// <summary>
        /// The active filepath of the log file (with filename)
        /// </summary>
        public string CurrentFilePath => Path.Combine(CurrentFolderPath, CurrentFilenameWithExtension);

        /// <summary>
        /// The path to the log file when it has been slated to be replaced or removed
        /// </summary>
        public string ReplacementFilePath { get; private set; }

        /// <summary>
        /// The active full path of the directory containing the log file
        /// </summary>
        public string CurrentFolderPath { get; private set; }

        /// <summary>
        /// The full path to the directory containing the log file as recorded from the properties file
        /// </summary>
        public string FolderPath
        {
            get => _folderPath;
            set
            {
                if (_folderPath == value || ReadOnly) return;

                if (value == null)
                    throw new ArgumentNullException(nameof(FolderPath), "Property is not allowed to be null. Use root, or customroot as a value instead.");
                _folderPath = value;
            }
        }

        /// <summary>
        /// The path that was first assigned when the log file was first registered
        /// </summary>
        public string OriginalFolderPath
        {
            get => _originalFolderPath;
            set
            {
                if (_originalFolderPath == value || ReadOnly) return;

                if (value == null)
                    throw new ArgumentNullException(nameof(OriginalFolderPath), "Property is not allowed to be null. Use root, or customroot as a value instead.");
                _originalFolderPath = value;
            }
        }

        /// <summary>
        /// The path of the last known location of the log file
        /// </summary>
        public string LastKnownFilePath { get; internal set; }

        /// <summary>
        /// The filename that will be used in the typical write path for the log file
        /// </summary>
        public string Filename
        {
            get => _filename;
            set
            {
                if (_filename == value || ReadOnly) return;

                if (value == null)
                    throw new ArgumentNullException(nameof(Filename));
                _filename = value;
            }
        }

        /// <summary>
        /// The filename that will be used if the write path is the Logs directory. May be null, or empty if same as Filename
        /// </summary>
        public string AltFilename
        {
            get => _altFilename;
            set
            {
                if (_altFilename == value || ReadOnly) return;

                if (value == null)
                    throw new ArgumentNullException(nameof(AltFilename));
                _altFilename = value;
            }
        }

        public string PreferredFileExt = FileExt.DEFAULT;
    }
}
