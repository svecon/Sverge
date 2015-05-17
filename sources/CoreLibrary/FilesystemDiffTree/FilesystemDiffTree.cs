﻿using System.IO;
using CoreLibrary.FilesystemTree;
using CoreLibrary.FilesystemTree.Enums;

namespace CoreLibrary.FilesystemDiffTree
{
    /// <summary>
    /// FilesystemDiffTree is enhanced for some features needed in diffing between files.
    /// </summary>
    public class FilesystemDiffTree : FilesystemTree.FilesystemTree
    {
        /// <summary>
        /// Initializes new instance of the <see cref="FilesystemDiffTree"/>
        /// </summary>
        /// <param name="mode">Default Diff mode for the tree.</param>
        public FilesystemDiffTree(DiffModeEnum mode)
            : base(mode)
        {
        }

        /// <inheritdoc />
        protected override INodeDirNode CreateDirNode(DirectoryInfo info, LocationEnum location)
        {
            return new DirDiffNode(null, null, info, location, DiffMode);
        }
    }
}
