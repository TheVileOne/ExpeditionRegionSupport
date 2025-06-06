﻿using LogUtils.Events;
using System;

namespace LogUtils.Properties.Formatting
{
    public abstract class LogRule
    {
        /// <summary>
        /// The containing collection instance of a LogRule. Only one collection allowed per instance
        /// </summary>
        public LogRuleCollection Owner;

        public bool ReadOnly
        {
            get
            {
                //Temporary rules can always be changed, because the utility wont write this data to file, unlike a non-temporary rule
                if (IsTemporary)
                    return false;
                return Owner?.ReadOnly == true;
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
                    TemporaryOverride = (LogRule)Activator.CreateInstance(GetType(), new object[] { true });
                else
                {
                    TemporaryOverride.Enable();
                }
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
                    TemporaryOverride = (LogRule)Activator.CreateInstance(GetType(), new object[] { false });
                else
                {
                    TemporaryOverride.Disable();
                }
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

        public LogRule(bool enabled)
        {
            _enabled = enabled;
        }

        public LogRule(string name, bool enabled) : this(enabled)
        {
            Name = name;
        }

        public void Apply(ref string message, LogMessageEventArgs logEventData)
        {
            if (TemporaryOverride != null)
            {
                message = TemporaryOverride.ApplyRule(message, logEventData); //Apply temporary rule instead
                return;
            }
            message = ApplyRule(message, logEventData);
        }

        protected virtual string ApplyRule(string message, LogMessageEventArgs logEventData)
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

        public override string ToString()
        {
            return ToPropertyString();
        }

        public delegate string ApplyDelegate(string message, LogMessageEventArgs logEventData);
    }

    public class ShowCategoryRule : LogRule
    {
        public ShowCategoryRule(bool enabled) : base(UtilityConsts.DataFields.Rules.SHOW_CATEGORIES, enabled)
        {
        }

        protected override string ApplyRule(string message, LogMessageEventArgs logEventData)
        {
            //TODO: Padding doesn't work
            return string.Format("[{0,-4}] {1}", logEventData.Category, message);
        }

        protected override float GetPriority()
        {
            return 0.995f; //This rule has the second highest defined priority
        }
    }

    public class ShowLineCountRule : LogRule
    {
        public ShowLineCountRule(bool enabled) : base(UtilityConsts.DataFields.Rules.SHOW_LINE_COUNT, enabled)
        {
        }

        protected override string ApplyRule(string message, LogMessageEventArgs logEventData)
        {
            return string.Format("[{0}] {1}", logEventData.TotalMessagesLogged + 1, message); //Add one to start indexing at one, instead of zero
        }

        protected override float GetPriority()
        {
            return 1.0f; //This rule has the highest defined priority
        }
    }
}
