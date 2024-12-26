using System;

namespace LogUtils.Diagnostics
{
    public struct ConditionResults
    {
        public static ConditionResults Fail => new ConditionResults(false);
        public static ConditionResults Pass => new ConditionResults(true);

        private Message _response;

        /// <summary>
        /// The response message object created from processing the assert conditions 
        /// </summary>
        public Message Response
        {
            get
            {
                if (_response == null)
                    _response = Message.Empty;
                return _response;
            }
            set => _response = value;
        }

        public readonly ConditionStatus Status;

        public readonly bool Failed => Status == ConditionStatus.Fail;
        public readonly bool Passed => Status == ConditionStatus.Pass;

        public ConditionResults(bool conditionPassed)
        {
            Status = conditionPassed ? ConditionStatus.Pass : ConditionStatus.Fail;
        }

        public override string ToString()
        {
            return Response.ToString();
        }


    }

    public enum ConditionStatus
    {
        Pass,
        Fail
    }
}
