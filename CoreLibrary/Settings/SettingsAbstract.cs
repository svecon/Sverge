﻿using System.Reflection;
using System.Text;
using CoreLibrary.Interfaces;
using CoreLibrary.Settings.Attributes;

namespace CoreLibrary.Settings
{
    /// <summary>
    /// Abstract class for Processor's settings.
    /// 
    /// This class needs to be extended for needed field types.
    /// 
    /// All Settings classes MUST HAVE a static ForType attribute
    /// which defines what field type the class is for.
    /// </summary>
    public abstract class SettingsAbstract : ISettings
    {
        // MUST HAVE for children
        //public abstract Type ForType { get; }

        public string Info { get; protected set; }

        public string Argument { get; protected set; }

        public string ArgumentShortcut { get; protected set; }

        public bool WasSet { get; protected set; }

        public abstract int NumberOfParams { get; }

        public object Instance { get; protected set; }

        public FieldInfo Field { get; protected set; }

        protected SettingsAbstract(object instance, FieldInfo field, SettingsAttribute attribute)
        {
            Instance = instance;
            Field = field;

            Info = attribute.Info;
            Argument = attribute.Argument;
            ArgumentShortcut = attribute.Shortcut;

            WasSet = false;
        }

        public abstract void SetValue(params string[] value);

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (ArgumentShortcut != null)
            {
                sb.AppendFormat("  -{0, -2}", ArgumentShortcut);
            }
            else
            {
                sb.AppendFormat("{0,5}", "");
            }

            if (Argument != null)
            {
                sb.AppendFormat(" --{0}", Argument);
            }

            sb.AppendFormat(" [{0}] ", NumberOfParams);

            sb.Append(Info);

            return sb.ToString();
        }
    }
}
