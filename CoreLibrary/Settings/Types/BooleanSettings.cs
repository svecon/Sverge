﻿using CoreLibrary.Settings.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.Settings.Types
{
    public class BooleanSettings : SettingsAbstract
    {
        public static Type ForType { get { return typeof(bool); } }

        public override int NumberOfParams { get { return 0; } }

        public BooleanSettings(object instance, FieldInfo field, SettingsAttribute attribute)
            : base(instance, field, attribute)
        {
        }

        public override void SetValue(params string[] value)
        {
            Field.SetValue(Instance, !(bool)Field.GetValue(Instance));
            WasSet = true;
        }
    }
}
