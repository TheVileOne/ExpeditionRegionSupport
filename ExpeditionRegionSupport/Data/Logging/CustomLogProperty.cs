﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Data.Logging
{
    public class CustomLogProperty
    {
        /// <summary>
        /// A field that indicates that this property is convertible into a LogRule instance
        /// </summary>
        public bool IsLogRule;

        /// <summary>
        /// A field that indicates whether property functionality should be applied
        /// </summary>
        public bool IsEnabled => CheckEnabled(Value);

        /// <summary>
        /// The string that will be used as header information
        /// </summary>
        public string Name;

        /// <summary>
        /// The value of the property converted to a string
        /// </summary>
        public string Value;

        public string PropertyString => Name + ':' + Value;

        /// <summary>
        /// An overridable method that allows custom parsing of the value to determine the enable state for this property
        /// </summary>
        protected internal virtual bool CheckEnabled(string value)
        {
            return true;
        }

        /// <summary>
        /// Constructs a CustomLogProperty
        /// </summary>
        /// <param name="name">A string to be used as header information</param>
        /// <param name="value">The default, or current value assigned to this property converted to a string</param>
        /// <param name="isRule">Whether or not this data is associated with a custom LogRule object</param>
        public CustomLogProperty(string name, string value, bool isRule)
        {
            Name = name;
            Value = value;
            IsLogRule = isRule;
        }

        /// <summary>
        /// An overridable method used in conjunction with IsLogRule for constructing custom LogRule implementations 
        /// </summary>
        public virtual LogRule CreateRule()
        {
            return null;
        }
    }
}
