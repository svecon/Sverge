﻿using System.Windows.Controls;
using CoreLibrary.DiffWindow;
using CoreLibrary.Interfaces;

namespace Sverge.DiffWindows
{
    /// <summary>
    /// Interaction logic for DefaultDiffWindow.xaml
    /// </summary>
    [DiffWindow(int.MaxValue)]
    public partial class DefaultDiffWindow : UserControl, IDiffWindow<object>
    {
        public object DiffNode { get; private set; }

        public DefaultDiffWindow(object instance)
        {
            DiffNode = instance;

            InitializeComponent();
        }

        public static bool CanBeApplied(object instance)
        {
            return true;
        }
    }
}
