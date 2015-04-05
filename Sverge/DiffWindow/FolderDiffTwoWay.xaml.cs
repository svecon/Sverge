﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CoreLibrary.Enums;
using DiffIntegration.DiffFilesystemTree;
using Sverge.Control;
using Sverge.Control.LineMarkers;

namespace Sverge.DiffWindow
{
    /// <summary>
    /// Interaction logic for FolderDiffTwoWay.xaml
    /// </summary>
    public partial class FolderDiffTwoWay : UserControl, IDiffWindow
    {
        private DiffFilesystemTree node;

        public FolderDiffTwoWay(object diffNode)
        {
            node = (DiffFilesystemTree)diffNode;

            InitializeComponent();

            TreeView.ItemsSource = ((DiffDirNode)node.Root).FilesAndDirectories;
        }

        public static bool CanBeApplied(object instance)
        {
            var filesystemTree = instance as DiffFilesystemTree;

            if (filesystemTree == null)
                return false;

            return filesystemTree.DiffMode == DiffModeEnum.TwoWay;
        }

        private void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DiffDirNode.FilterIgnored = true;
            TreeView.ItemsSource = ((DiffDirNode)node.Root).FilesAndDirectories;
        }
    }
}
