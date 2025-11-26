using LogUtils.Console;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Formatting;
using System;
using UnityEngine;

namespace LogUtils.Properties.Formatting
{
    public abstract class LogRule : ICloneable
    {
        /// <summary>
        /// The containing collection instance of a LogRule. Only one collection allowed per instance
        /// </summary>
        public LogRuleCollection Owner;

        /// <summary>
        /// Is persistent state protected from modifications
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                //Temporary rules can always be changed, because the utility wont write this data to file, unlike a non-temporary rule
                if (IsTemporary)
                    return false;

                return Owner != null && !Owner.AllowRuleChanges;
            }
        }

        /// <summary>
        /// An unique string that identifies a particular LogRule. LogRules with the same value in this field will be treated as interchangable within the LogRuleCollection class
        /// </summary>
        public string Name = "Unknown";

        /// <summary>
        /// The default priority of a LogRule. Rules are applied in order of priority from lowest to highest
        /// </summary>
        public float Priority
        {
            get
            {
                if (TemporaryOverride != null)
                    return TemporaryOverride.GetPriority();
                return GetPriority();
            }
        }

        private LogRule _temporaryOverride;

        /// <summary>
        /// The instance stored in this field takes priority over the LogRule that contains it
        /// </summary>
        public LogRule TemporaryOverride
        {
            get => _temporaryOverride;
            set
            {
                if (value == this) return; //A reference cannot override itself

                if (Owner != null)
                {
                    Owner.ChangeAlert();

                    if (Owner.TrackChanges)
                        Owner.ChangeRecord.Push(new ChangeState(this, nameof(_temporaryOverride)));
                }

                if (_temporaryOverride != null)
                {
                    _temporaryOverride.IsTemporary = false;
                    _temporaryOverride.Owner = null;
                }

                if (value != null)
                {
                    value.IsTemporary = true;
                    value.Owner = Owner;
                    value.TemporaryOverride = null; //Temporary rules should not be allowed to have temporary rules
                }
                _temporaryOverride = value;
            }
        }

        public bool IsTemporary { get; set; }

        private bool _enabled;

        public bool IsEnabled
        {
            get
            {
                if (TemporaryOverride != null)
                    return TemporaryOverride.IsEnabled;
                return _enabled;
            }
            set
            {
                if (TemporaryOverride != null)
                {
                    TemporaryOverride.IsEnabled = value;
                    return;
                }

                if (ReadOnly || value == _enabled) return;

                if (Owner != null)
                {
                    Owner.ChangeAlert();

                    if (Owner.TrackChanges)
                        Owner.ChangeRecord.Push(new ChangeState(this, nameof(_enabled)));
                }
                _enabled = value;
            }
        }

        public void Enable()
        {
            if (IsEnabled) return;

            //When handling a non-temporary rule, temporarily enable rule, creating a temporary instance if necessary 
            if (!IsTemporary)
            {
                Owner?.ResetRecord(); //Clear any past changes before operation runs

                if (TemporaryOverride == null)
                    TemporaryOverride = (LogRule)Clone();

                TemporaryOverride.Enable();
            }
            else //Temporary rules are free to be modified directly
            {
                IsEnabled = true;
            }
        }

        public void Disable()
        {
            if (!IsEnabled) return;

            //When handling a non-temporary rule, temporarily disable rule, creating a temporary instance if necessary 
            if (!IsTemporary)
            {
                Owner?.ResetRecord(); //Clear any past changes before operation runs

                if (TemporaryOverride == null)
                    TemporaryOverride = (LogRule)Clone();

                TemporaryOverride.Disable();
            }
            else //Temporary rules are free to be modified directly
            {
                IsEnabled = false;
            }
        }

        /// <summary>
        /// Change recent rule modifications back to pre-change values
        /// </summary>
        public void Restore()
        {
            Owner?.RestoreRecord();
        }

        public string PropertyString => ToPropertyString();

        /// <summary>
        /// Constructs a new LogRule instance
        /// </summary>
        protected LogRule(string name, bool enabled)
        {
            Name = name;
            _enabled = enabled;
        }

        /// <summary>
        /// Applies format logic to a message
        /// </summary>
        /// <param name="formatter">The applicable formatter instance</param>
        /// <param name="message">The message to format</param>
        /// <param name="logEventData">Data associated with the message event</param>
        public void Apply(LogMessageFormatter formatter, ref string message, LogRequestEventArgs logEventData)
        {
            if (TemporaryOverride != null)
            {
                message = TemporaryOverride.ApplyRule(formatter, message, logEventData); //Apply temporary rule instead
                return;
            }
            message = ApplyRule(formatter, message, logEventData);
        }

        /// <summary>
        /// Applies format logic to a message
        /// </summary>
        /// <param name="formatter">The applicable formatter instance</param>
        /// <param name="message">The message to format</param>
        /// <param name="logEventData">Data associated with the message event</param>
        /// <returns>The message after rule formatting is applied</returns>
        protected virtual string ApplyRule(LogMessageFormatter formatter, string message, LogRequestEventArgs logEventData)
        {
            return message;
        }

        protected virtual float GetPriority()
        {
            return 0.7f;
        }

        public virtual string ToPropertyString()
        {
            return LogProperties.ToPropertyString(Name.ToLower(), _enabled.ToString());
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToPropertyString();
        }

        /// <inheritdoc/>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Delegate signature for applying a <see cref="LogRule"/>
        /// </summary>
        /// <param name="formatter">The applicable formatter instance</param>
        /// <param name="message">The message to format</param>
        /// <param name="logEventData">Data associated with the message event</param>
        /// <returns>The message after rule formatting is applied</returns>
        public delegate string ApplyDelegate(LogMessageFormatter formatter, string message, LogRequestEventArgs logEventData);
    }

    public class ShowCategoryRule : LogRule
    {
        public ShowCategoryRule(bool enabled) : base(UtilityConsts.DataFields.Rules.SHOW_CATEGORIES, enabled)
        {
        }

        /// <inheritdoc/>
        protected override string ApplyRule(LogMessageFormatter formatter, string message, LogRequestEventArgs logEventData)
        {
            LogCategory category = logEventData.Category;
            Color headerColor = category.ConsoleColor;

            string messageHeader = string.Format("[{0,-4}] ", category);

            messageHeader = formatter.ApplyColor(messageHeader, headerColor);
            return messageHeader + message;
        }

        /// <inheritdoc/>
        protected override float GetPriority()
        {
            return 0.995f; //This rule has the second highest defined priority
        }
    }

    public class ShowLineCountRule : LogRule
    {
        /// <summary>
        /// The color that applies to the line count rule
        /// </summary>
        public Color RuleColor = ConsoleColorMap.GetColor(ConsoleColor.White);

        public ShowLineCountRule(bool enabled) : base(UtilityConsts.DataFields.Rules.SHOW_LINE_COUNT, enabled)
        {
        }

        /// <inheritdoc/>
        protected override string ApplyRule(LogMessageFormatter formatter, string message, LogRequestEventArgs logEventData)
        {
            string messageHeader = string.Format("[{0}] ", logEventData.TotalMessagesLogged + 1); //Add one to start indexing at one, instead of zero

            messageHeader = formatter.ApplyColor(messageHeader, RuleColor);
            return messageHeader + message;
        }

        /// <inheritdoc/>
        protected override float GetPriority()
        {
            return 1.0f; //This rule has the highest defined priority
        }
    }
}
