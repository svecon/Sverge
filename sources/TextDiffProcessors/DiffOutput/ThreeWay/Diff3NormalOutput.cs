﻿using System;
using System.Collections.Generic;
using System.IO;
using CoreLibrary.FilesystemTree.Enums;
using TextDiffAlgorithm.ThreeWay;

namespace TextDiffProcessors.DiffOutput.ThreeWay
{
    /// <summary>
    /// Prints differences between 3 files in Diff Normal Output.
    /// </summary>
    public class Diff3NormalOutput : DiffOutputAbstract<Diff3, Diff3Item>
    {
        /// <summary>
        /// Info for base file.
        /// </summary>
        private readonly FileInfo infoBase;

        /// <summary>
        /// Initializes new instance of the <see cref="Diff3NormalOutput"/>
        /// </summary>
        /// <param name="infoLocal">Info for the local file.</param>
        /// <param name="infoBase">Info for the base file.</param>
        /// <param name="infoRemote">Info for the remote file.</param>
        /// <param name="diff">Calculated 3-way text diff.</param>
        public Diff3NormalOutput(FileInfo infoLocal, FileInfo infoBase, FileInfo infoRemote, Diff3 diff)
            : base(infoLocal, infoRemote, diff)
        {
            this.infoBase = infoBase;
        }

        /// <inheritdoc />
        public override IEnumerable<string> Print()
        {
            using (StreamReader localStream = InfoLocal.OpenText())
            using (StreamReader remoteStream = InfoRemote.OpenText())
            using (StreamReader baseStream = infoBase.OpenText())
            {
                int m = 0;
                int n = 0;
                int o = 0;

                foreach (Diff3Item diffItem in Diff.Items)
                {
                    CurrentDiffItem = diffItem;

                    yield return HunkHeader(diffItem.Differeces);

                    // skip same
                    for (; o < diffItem.BaseLineStart; o++) { baseStream.ReadLine(); }
                    for (; m < diffItem.LocalLineStart; m++) { localStream.ReadLine(); }
                    for (; n < diffItem.RemoteLineStart; n++) { remoteStream.ReadLine(); }

                    // DifferencesStatusEnum.LocalRemoteSame has a different order of blocks
                    if (diffItem.Differeces == DifferencesStatusEnum.LocalRemoteSame)
                    {
                        foreach (string l in PrintSection("1", diffItem.LocalLineStart, diffItem.LocalAffectedLines,
                            m, localStream, Diff.FilesLineCount.Local, Diff.FilesEndsWithNewLine.Local, false))
                            yield return l;

                        foreach (string l in PrintSection("3", diffItem.RemoteLineStart, diffItem.RemoteAffectedLines,
                            n, remoteStream, Diff.FilesLineCount.Remote, Diff.FilesEndsWithNewLine.Remote))
                            yield return l;

                        foreach (string l in PrintSection("2", diffItem.BaseLineStart, diffItem.BaseAffectedLines,
                            o, baseStream, Diff.FilesLineCount.Base, Diff.FilesEndsWithNewLine.Base))
                            yield return l;
                    } else
                    {
                        foreach (string l in PrintSection("1", diffItem.LocalLineStart, diffItem.LocalAffectedLines,
                            m, localStream, Diff.FilesLineCount.Local, Diff.FilesEndsWithNewLine.Local,
                                diffItem.Differeces != DifferencesStatusEnum.BaseLocalSame))
                            yield return l;

                        foreach (string l in PrintSection("2", diffItem.BaseLineStart, diffItem.BaseAffectedLines,
                            o, baseStream, Diff.FilesLineCount.Base, Diff.FilesEndsWithNewLine.Base,
                                diffItem.Differeces != DifferencesStatusEnum.BaseRemoteSame))
                            yield return l;

                        foreach (string l in PrintSection("3", diffItem.RemoteLineStart, diffItem.RemoteAffectedLines,
                            n, remoteStream, Diff.FilesLineCount.Remote, Diff.FilesEndsWithNewLine.Remote))
                            yield return l;
                    }

                    m += diffItem.LocalAffectedLines;
                    n += diffItem.RemoteAffectedLines;
                    o += diffItem.BaseAffectedLines;

                    DiffHasEnded = true;
                }
            }
        }

        /// <summary>
        /// Prints one section of the diff.
        /// </summary>
        /// <param name="fileMark">String mark for the current diff.</param>
        /// <param name="lineStart">Number of line where diff starts.</param>
        /// <param name="affectedLines">Number of lines affected by diff.</param>
        /// <param name="c">Current line read by the reader.</param>
        /// <param name="stream">Stream reader for the file.</param>
        /// <param name="fileLinesCount">Total number of lines in the file.</param>
        /// <param name="endsNewLine">Does the file end with new line?</param>
        /// <param name="printLines">Print the contents of the file?</param>
        /// <returns>Enumarable over the difference.</returns>
        private static IEnumerable<string> PrintSection(string fileMark, int lineStart, int affectedLines,
            int c, TextReader stream, int fileLinesCount, bool endsNewLine, bool printLines = true)
        {
            yield return FileHeader(fileMark, lineStart, affectedLines);

            for (int p = 0; p < affectedLines; p++)
            {
                if (printLines)
                {
                    yield return "  " + stream.ReadLine();
                } else
                {
                    stream.ReadLine();
                }
                c++;
            }


            if (c == fileLinesCount && !endsNewLine)
                yield return "\\ No newline at end of file";
        }

        /// <summary>
        /// Returns diff chunk header with a number of file that is different
        /// </summary>
        /// <param name="diffStatus">Which files are same.</param>
        /// <returns>String header of the diff chunk.</returns>
        private static string HunkHeader(DifferencesStatusEnum diffStatus)
        {
            switch (diffStatus)
            {
                case DifferencesStatusEnum.BaseLocalSame:
                    return "===3";
                case DifferencesStatusEnum.BaseRemoteSame:
                    return "===1";
                case DifferencesStatusEnum.LocalRemoteSame:
                    return "===2";
                case DifferencesStatusEnum.AllDifferent:
                    return "===";
                default:
                    throw new ApplicationException("Diff chunk cannot have this DifferenceStatusEnum.");
            }
        }

        /// <summary>
        /// String header of file diff section
        /// </summary>
        /// <param name="fileMark">String representation of file.</param>
        /// <param name="lineStart">Where does the diff start.</param>
        /// <param name="linesAffected">How many lines does diff affect.</param>
        /// <returns>String header of file diff section.</returns>
        private static string FileHeader(string fileMark, int lineStart, int linesAffected)
        {
            return fileMark + ":" + CreateRange(lineStart, linesAffected);
        }

        /// <summary>
        /// Print range of how many lines were affected by the change.
        /// </summary>
        /// <param name="startingLine">Starting line in a file.</param>
        /// <param name="numberOfLines">Number of lines affected in a file.</param>
        /// <returns>String representation of diff range.</returns>
        private static string CreateRange(int startingLine, int numberOfLines)
        {
            if (numberOfLines == 0)
            {
                return startingLine + "a";
            }

            return numberOfLines > 1
                ? (startingLine + 1) + "," + ((startingLine + 1) + numberOfLines - 1) + "c"
                : (startingLine + 1) + "c";
        }
    }
}
