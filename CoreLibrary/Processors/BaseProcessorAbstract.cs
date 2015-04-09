﻿using CoreLibrary.Enums;
using CoreLibrary.Interfaces;

namespace CoreLibrary.Processors
{
    /// <summary>
    /// ProcessorAbstract is a base class for all types of processors.
    /// 
    /// Contain some helper methods that are shared for all processors.
    /// </summary>
    public abstract class BaseProcessorAbstract : IProcessorBase
    {
        /// <summary>
        /// Mode is a mask for all possible DiffModeEnum values.
        /// </summary>
        public abstract DiffModeEnum Mode { get; }

        public abstract int Priority { get; }

        /// <summary>
        /// Checks whether the node still should be processed or not.
        /// </summary>
        /// <param name="node">FilesystemTreeAbstractNode</param>
        /// <returns>True if the node should be processed.</returns>
        protected virtual bool CheckStatus(IFilesystemTreeAbstractNode node)
        {
            switch (node.Status)
            {
                case NodeStatusEnum.HasError:
                    return false;
                case NodeStatusEnum.IsIgnored:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether current Processor is compatible with node's mode.
        /// </summary>
        /// <param name="node">FilesystemTreeAbstractNode</param>
        /// <returns>True if the processor is compatible.</returns>
        protected virtual bool CheckMode(IFilesystemTreeAbstractNode node)
        {
            return (node.Mode & Mode) != 0;
        }

        /// <summary>
        /// Check node's mode and status for compatibility.
        /// </summary>
        /// <param name="node">FilesystemTreeAbstractNode</param>
        /// <returns>True if the node should be processed.</returns>
        protected bool CheckModeAndStatus(IFilesystemTreeAbstractNode node)
        {
            return CheckMode(node) && CheckStatus(node);
        }

        public abstract void Process(IFilesystemTreeDirNode node);

        public abstract void Process(IFilesystemTreeFileNode node);
    }
}
