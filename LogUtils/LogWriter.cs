using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    public class LogWriter : ILogWriter
    {
        private static SharedField<ILogWriter> _writer;

        public static ILogWriter Writer
        {
            get
            {
                if (_writer == null)
                {
                    _writer = UtilityCore.DataHandler.GetField<ILogWriter>("logwriter");

                    if (_writer.Value == null)
                        _writer.Value = new LogWriter();
                }
                return _writer.Value;
            }
        }

        public void CreateFile(LogID logFile)
        {
            try
            {
                File.Create(logFile.Properties.CurrentFilePath);
                logFile.Properties.LogStartProcess();
            }
            catch (IOException e)
            {
                UtilityCore.BaseLogger.LogError($"Unable to create file {logFile.Properties.CurrentFilename}.log");
                UtilityCore.BaseLogger.LogError(e);
            }
        }

        public void WriteToFile(LogID logFile, string message)
        {
            if (!logFile.Properties.FileExists)
                CreateFile(logFile);

            OnLogMessageReceived(new LogEvents.LogMessageEventArgs(logFile, message));

            string writePath = logFile.Properties.CurrentFilePath;

            message = ApplyRules(logFile, message);
            File.AppendAllText(writePath, message);
        }

        public string ApplyRules(LogID logFile, string message)
        {
            message = message ?? string.Empty;

            foreach (LogRule rule in logFile.Properties.Rules.Where(r => r.IsEnabled))
                rule.Apply(ref message);
            return message;
        }

        protected virtual void OnLogMessageReceived(LogEvents.LogMessageEventArgs e)
        {
            LogEvents.OnMessageReceived?.Invoke(e);
        }
    }

    public interface ILogWriter
    {
        public void CreateFile(LogID logFile);
        public void WriteToFile(LogID logFile, string message);
        public string ApplyRules(LogID logFile, string message);
    }
}
