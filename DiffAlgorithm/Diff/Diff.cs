﻿using System;
using System.IO;

namespace DiffAlgorithm.Diff
{
    /// <summary>
    /// Container for holding all information about a two-way diff.
    /// </summary>
    public class Diff
    {
        /// <summary>
        /// Number of lines for two diffed files.
        /// </summary>
        public struct FilesLineCountStruct
        {
            public int Local;
            public int Remote;
        }

        /// <summary>
        /// Do the files have an empty line at the end?
        /// (Standard in Unix files)
        /// </summary>
        public struct FilesEndsWithNewLineStruct
        {
            public bool Local;
            public bool Remote;
        }

        /// <summary>
        /// All DiffItems.
        /// </summary>
        public DiffItem[] Items { get; protected set; }

        private DateTime diffedTime;

        private FileInfo oldFile;
        private FileInfo newFile;

        public FilesLineCountStruct FilesLineCount;
        public FilesEndsWithNewLineStruct FilesEndsWithNewLine;

        /// <summary>
        /// Constructor for Diff.
        /// </summary>
        /// <param name="oldFile">Local file diffed.</param>
        /// <param name="newFile">Remote file diffed.</param>
        public Diff(FileInfo oldFile, FileInfo newFile)
        {
            this.oldFile = oldFile;
            this.newFile = newFile;

            diffedTime = DateTime.Now;
            FilesLineCount = new FilesLineCountStruct();
            FilesEndsWithNewLine = new FilesEndsWithNewLineStruct();
        }

        /// <summary>
        /// Insert calculated diff items into the container.
        /// </summary>
        /// <param name="diffItems">Diff item changes between the files.</param>
        public void SetDiffItems(DiffItem[] diffItems)
        {
            Items = diffItems;
        }

        /// <summary>
        /// Sets statistics useful during diffing.
        /// 
        /// Number of lines.
        /// Does file end with new line?
        /// </summary>
        /// <param name="oldLineCount">Number of lines in the old file.</param>
        /// <param name="newLineCount">Number of files in the new file.</param>
        /// <param name="oldEndsNewLine">Does old file end with new line?</param>
        /// <param name="newEndsNewLine">Does new file end with new line?</param>
        public void SetStatistics(int oldLineCount, int newLineCount, bool oldEndsNewLine, bool newEndsNewLine)
        {
            FilesLineCount.Local = oldLineCount;
            FilesLineCount.Remote = newLineCount;

            FilesEndsWithNewLine.Local = oldEndsNewLine;
            FilesEndsWithNewLine.Remote = newEndsNewLine;
        }

    }
}
