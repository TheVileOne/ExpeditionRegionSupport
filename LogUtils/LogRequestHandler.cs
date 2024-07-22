using System.Collections.Generic;
using System.IO;

namespace LogUtils
{
    public static class LogRequestHandler
    {
        private static SharedField<LogRequest> _currentRequest;

        public static LogRequest CurrentRequest
        {
            get
            {
                if (_currentRequest == null)
                    _currentRequest = UtilityCore.DataHandler.GetField<LogRequest>(nameof(CurrentRequest));
                return _currentRequest.Value;
            }
            set => _currentRequest.Value = value;
        }
    }
}
