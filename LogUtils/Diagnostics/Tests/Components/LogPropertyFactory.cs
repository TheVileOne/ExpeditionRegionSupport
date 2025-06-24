using LogUtils.Diagnostics.Tools;
using LogUtils.Properties;
using System;
using System.Collections.Generic;

namespace LogUtils.Diagnostics.Tests.Components
{
    public class LogPropertyFactory : IDisposable
    {
        private StringProvider _provider;

        public StringProvider Provider
        {
            get => _provider;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _provider = value;

                NameEnumerator?.Dispose();

                NameEnumerator = _provider.GetEnumerator(); //Each time an enumerator is 
            }
        }

        protected IEnumerator<string> NameEnumerator;

        public LogPropertyFactory(string templateName = null)
        {
            Provider = new TestStringProvider(templateName);
        }

        /// <summary>
        /// Create an instance of a LogProperties for testing purposes
        /// </summary>
        /// <param name="path">The path to use for the instance</param>
        /// <param name="register">Whether or not instance should be added to the PropertyDataController</param>
        public LogProperties Create(string path, bool register)
        {
            return Create(path, true, register);
        }

        /// <summary>
        /// Create an instance of a LogProperties for testing purposes
        /// </summary>
        /// <param name="path">The path to use for the instance</param>
        /// <param name="advanceEnumeration">Affects whether a new string is provided, or the latest one</param>
        /// <param name="register">Whether or not instance should be added to the PropertyDataController</param>
        public LogProperties Create(string path, bool advanceEnumeration, bool register)
        {
            if (advanceEnumeration)
                NameEnumerator.MoveNext();

            LogProperties instance = new LogProperties(NameEnumerator.Current, path);

            if (register)
                LogProperties.PropertyManager.SetProperties(instance);
            return instance;
        }

        public void Dispose()
        {
            NameEnumerator.Dispose();
        }
    }
}
