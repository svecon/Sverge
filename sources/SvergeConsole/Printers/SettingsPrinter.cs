﻿using System;
using System.Collections.Generic;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;
using CoreLibrary.Plugins.Processors.Settings.Types;

namespace SvergeConsole.Printers
{
    /// <summary>
    /// Prints all available settings on a Console into a well-arranged column layout.
    /// </summary>
    public class SettingsPrinter : IPrinter
    {
        /// <summary>
        /// A list of settings to be printed out.
        /// </summary>
        readonly List<ISettings> settings;

        /// <summary>
        /// Length of the longest setting option.
        /// </summary>
        readonly int longestOption = 0;

        /// <summary>
        /// Number of spaces to to the left used as an indentation.
        /// </summary>
        private readonly int paddingLeft = 0;

        /// <summary>
        /// Constructor for SettingsPrinter.
        /// </summary>
        /// <param name="settings">Setting to be printed.</param>
        /// <param name="paddingLeft">Number of spaces to add from left.</param>
        public SettingsPrinter(IEnumerable<ISettings> settings, int paddingLeft = 0)
        {
            this.settings = new List<ISettings>();
            this.paddingLeft = paddingLeft;

            foreach (ISettings option in settings)
            {
                this.settings.Add(option);

                if (option.Argument != null && longestOption < option.Argument.Length)
                    longestOption = option.Argument.Length;
            }
        }

        /// <summary>
        /// Prints the settings ordered alphabetically.
        /// </summary>
        public void Print()
        {
            SortByOptionParameters();

            //Console.WriteLine("Listing of all possible options:");

            foreach (ISettings option in settings)
            {
                if (option.ArgumentShortcut != null)
                {
                    Console.CursorLeft = paddingLeft + 2;
                    Console.Write("-" + option.ArgumentShortcut);
                }

                if (option.Argument != null)
                {
                    Console.CursorLeft = paddingLeft + 6;
                    Console.Write("--" + option.Argument);
                }

                Console.CursorLeft = paddingLeft + 9 + longestOption;
                Console.Write(" [" + option.NumberOfParams + "]");

                Console.CursorLeft = paddingLeft + 15 + longestOption;
                Console.Write(option.Info);

                if (option is EnumSettings)
                {
                    Console.WriteLine();
                    Console.CursorLeft = paddingLeft + 15 + longestOption;
                    Console.Write("Possible options: ");

                    Console.Write(String.Join(", ", Enum.GetNames(option.Field.FieldType)));
                }

                Console.WriteLine();
            }
        }

        /// <summary>
        /// Sorts settings alphabetically.
        /// </summary>
        private void SortByOptionParameters()
        {
            settings.Sort(new SettingsComparer());
        }

        /// <summary>
        /// Compares the two settings by their ArgumentShortcuts alphabetically if available.
        /// Otherwise use Switch.
        /// </summary>
        private class SettingsComparer : IComparer<ISettings>
        {
            /// <summary>
            /// Comapres two <see cref="ISettings"/>
            /// </summary>
            /// <param name="x">First setting to be compared.</param>
            /// <param name="y">Second setting to be compared.</param>
            /// <returns>Comparison integer</returns>
            public int Compare(ISettings x, ISettings y)
            {
                string left = x.ArgumentShortcut ?? x.Argument;
                string right = y.ArgumentShortcut ?? y.Argument;

                return String.Compare(left, right, StringComparison.InvariantCulture);
            }
        }

    }
}
