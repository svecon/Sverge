﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreLibrary.Enums;
using DiffAlgorithm.Diff;

namespace DiffAlgorithm.Diff3
{
    /// <summary>
    /// This class implements Diff3 algorithm.
    /// 
    /// Takes two 2-way diffs and merges them into 3-way chunks.
    /// Checks overlapping and conflicts.
    /// </summary>
    class Diff3Algorithm
    {
        /// <summary>
        /// 2-way diff between new and old files.
        /// </summary>
        private readonly DiffItem[] diffBaseLocal;

        /// <summary>
        /// 2-way diff between new and his files.
        /// </summary>
        private readonly DiffItem[] diffBaseRemote;

        /// <summary>
        /// Hashed new file.
        /// 
        /// Used in checking conflicts.
        /// </summary>
        private readonly int[] localFile;

        /// <summary>
        /// Hashed his file.
        /// 
        /// Used in checking conflicts.
        /// </summary>
        private readonly int[] remoteFile;

        /// <summary>
        /// Temporary container for Diff3Items
        /// </summary>
        private List<Diff3Item> diff3Items;

        /// <summary>
        /// How many lines ahead/back is old file compared to new file.
        /// 
        /// Always in regard to start of the diff chunk.
        /// </summary>
        private int deltaToNew;

        /// <summary>
        /// How many lines ahead/back is old file compared to his file.
        /// 
        /// Always in regard to start of the diff chunk.
        /// </summary>
        private int deltaToHis;

        #region very simple iterators over the two-way diffs
        int newIterator = 0;
        int hisIterator = 0;
        private DiffItem CurrentNew
        {
            get
            {
                return newIterator < diffBaseLocal.Length ? diffBaseLocal[newIterator] : null;
            }
        }
        private DiffItem CurrentHis
        {
            get
            {
                return hisIterator < diffBaseRemote.Length ? diffBaseRemote[hisIterator] : null;
            }
        }
        #endregion

        /// <summary>
        /// Constructor for Diff3Algorithm.
        /// </summary>
        /// <param name="diffBaseLocal">Two-way diff between old and new file.</param>
        /// <param name="diffBaseRemote">Two-way diff between old and his file.</param>
        /// <param name="localFile">Hashed new file.</param>
        /// <param name="remoteFile">Hashed his file.</param>
        public Diff3Algorithm(DiffItem[] diffBaseLocal, DiffItem[] diffBaseRemote, int[] localFile, int[] remoteFile)
        {
            this.diffBaseLocal = diffBaseLocal;
            this.diffBaseRemote = diffBaseRemote;

            this.localFile = localFile;
            this.remoteFile = remoteFile;
        }

        /// <summary>
        /// Main algorithm function.
        /// 
        /// Iterates over all two-way diffs and tries to decide if they are conflicting or not.
        /// If they are conflicting then they are merged into a one bigger chunk.
        /// </summary>
        /// <returns>Array of Diff3Items.</returns>
        public Diff3Item[] MergeIntoDiff3Chunks()
        {
            diff3Items = new List<Diff3Item>();

            while (CurrentNew != null || CurrentHis != null)
            {
                #region Solve partially overlapping diffs by extending them

                bool wasHis;
                DiffItem lowerDiff;
                Diff3Item old;

                if (diff3Items.Any() &&
                    (old = diff3Items.Last()).OldLineStart
                                        + old.OldAffectedLines
                    >= (lowerDiff = FindLowerDiff(out wasHis)).OldLineStart)
                // are they overlapping?
                {
                    // remove last diff item -- alias "old"
                    RemoveLastDiff3();

                    if (wasHis)
                        hisIterator++;
                    else
                        newIterator++;

                    // create extended chunk
                    AddNewDiff3(new Diff3Item(
                            old.OldLineStart,
                            old.NewLineStart,
                            old.HisLineStart,
                            old.OldAffectedLines + lowerDiff.DeletedInOld,
                            old.NewAffectedLines + (wasHis ? lowerDiff.DeletedInOld : lowerDiff.InsertedInNew),
                            old.HisAffectedLines + (wasHis ? lowerDiff.InsertedInNew : lowerDiff.DeletedInOld),
                            DifferencesStatusEnum.AllDifferent
                        ));

                    continue;
                }

                #endregion

                JoinDiffsIntoOne();
            }

            return diff3Items.ToArray();
        }

        /// <summary>
        /// Iterates over 2-way diffs and merge them into Diff3Items
        /// depending on their overlapping
        /// </summary>
        private void JoinDiffsIntoOne()
        {
            if (CurrentNew == null)
            // there are only a changes on his side remaining
            {
                AddNewDiff3(CreateFromHis());
                hisIterator++;
            } else if (CurrentHis == null)
            // there are only a changes on my side remaining
            {
                AddNewDiff3(CreateFromNew());
                newIterator++;

            } else if (CurrentNew.OldLineStart == CurrentHis.OldLineStart)
            // starts on the same line
            {
                if (CurrentNew.DeletedInOld == CurrentHis.DeletedInOld &&
                    CurrentNew.InsertedInNew == CurrentHis.InsertedInNew)
                // changes the same lines in old and adds same lines in new
                {
                    // check if the new lines are same => non-conflicting
                    bool areSame = true;
                    for (int i = 0; i < CurrentNew.InsertedInNew; i++)
                    {
                        if (localFile[CurrentNew.NewLineStart + i] == remoteFile[CurrentHis.NewLineStart + i])
                            continue;

                        areSame = false;
                        break;
                    }

                    AddNewDiff3(areSame
                        ? CreateFullChunk(DifferencesStatusEnum.LocalRemoteSame)
                        : CreateFullChunk(DifferencesStatusEnum.AllDifferent));

                    newIterator++; hisIterator++;

                } else
                // adding different number of lines => conflicting
                {
                    AddNewDiff3(CreateAllDifferent());
                    newIterator++; hisIterator++;
                }
            } else if (AreOverlapping(CurrentNew, CurrentHis) || AreOverlapping(CurrentHis, CurrentNew))
            // check if they are overlapping
            {
                AddNewDiff3(CreateAllDifferent());
                newIterator++; hisIterator++;
            } else if (CurrentNew.OldLineStart < CurrentHis.OldLineStart)
            // take CurrentNew as it starts lower 
            {
                AddNewDiff3(CreateFromNew());
                newIterator++;
            } else if (CurrentNew.OldLineStart > CurrentHis.OldLineStart)
            // take CurrentHis as it starts lower
            {
                AddNewDiff3(CreateFromHis());
                hisIterator++;
            } else
            {
                throw new ApplicationException("This should never happen.");
            }
        }


        /// <summary>
        /// Find lower (in terms of OldLineStart) diff
        /// </summary>
        /// <param name="wasHis">Which file does it come from?</param>
        /// <returns>DiffItem with lower OldLineStart</returns>
        private DiffItem FindLowerDiff(out bool wasHis)
        {
            wasHis = false;

            if (CurrentHis == null)
            {
                return CurrentNew;
            }

            if (CurrentNew == null)
            {
                wasHis = true;
                return CurrentHis;
            }

            if (CurrentNew.OldLineStart < CurrentHis.OldLineStart)
                return CurrentNew;

            wasHis = true;
            return CurrentHis;
        }

        /// <summary>
        /// Interface method for adding new Diff3Item.
        /// 
        /// Recalculates deltas for line numbers with regards to old file.
        /// </summary>
        /// <param name="item">Diff3Item to be added.</param>
        private void AddNewDiff3(Diff3Item item)
        {
            deltaToHis += item.HisAffectedLines - item.OldAffectedLines;
            deltaToNew += item.NewAffectedLines - item.OldAffectedLines;

            diff3Items.Add(item);
        }

        /// <summary>
        /// Interface method for removing Diff3Item.
        /// 
        /// Realculates deltas for line numbers with regards to old file.
        /// </summary>
        private void RemoveLastDiff3()
        {
            Diff3Item item = diff3Items.Last();
            deltaToHis -= item.HisAffectedLines - item.OldAffectedLines;
            deltaToNew -= item.NewAffectedLines - item.OldAffectedLines;

            diff3Items.RemoveAt(diff3Items.Count - 1);
        }

        /// <summary>
        /// Checks if two 2-way diffs are overlapping.
        /// </summary>
        /// <param name="bottom">Bottom 2-way diff.</param>
        /// <param name="top">Top 2-way diff.</param>
        /// <returns>Yes if they are ovelapping.</returns>
        bool AreOverlapping(DiffItem bottom, DiffItem top)
        {
            return (bottom.OldLineStart < top.OldLineStart
                    && bottom.OldLineStart + bottom.DeletedInOld >= top.OldLineStart);
        }

        /// <summary>
        /// Creates a Diff3Item between Local and Remote file.
        /// </summary>
        /// <returns>Diff3Item marking the change.</returns>
        private Diff3Item CreateFromNew()
        {
            return new Diff3Item(
                CurrentNew.OldLineStart,
                CurrentNew.NewLineStart,
                CurrentNew.OldLineStart + deltaToHis,
                CurrentNew.DeletedInOld,
                CurrentNew.InsertedInNew,
                CurrentNew.DeletedInOld,
                DifferencesStatusEnum.BaseRemoteSame
            );
        }

        /// <summary>
        /// Creates a Diff3Item between Local and Remote file.
        /// </summary>
        /// <returns>Diff3Item marking the change.</returns>
        private Diff3Item CreateFromHis()
        {
            return new Diff3Item(
                CurrentHis.OldLineStart,
                CurrentHis.OldLineStart + deltaToNew,
                CurrentHis.NewLineStart,
                CurrentHis.DeletedInOld,
                CurrentHis.DeletedInOld,
                CurrentHis.InsertedInNew,
                DifferencesStatusEnum.BaseLocalSame
            );
        }

        /// <summary>
        /// Creates a Diff3Item between all three files.
        /// </summary>
        /// <returns>Diff3Item marking the change.</returns>
        private Diff3Item CreateFullChunk(DifferencesStatusEnum diff)
        {
            return new Diff3Item(
                CurrentHis.OldLineStart,
                CurrentNew.NewLineStart,
                CurrentHis.NewLineStart,
                CurrentHis.DeletedInOld,
                CurrentNew.InsertedInNew,
                CurrentHis.InsertedInNew,
                diff
            );
        }

        /// <summary>
        /// All three files are different.
        /// 
        /// This block is created even for only partly overflowing chunks.
        /// That means that we need to shift some lines to cover all the lines for every one of them.
        /// </summary>
        /// <returns>Diff3Item marking the change.</returns>
        private Diff3Item CreateAllDifferent()
        {
            int minOldStart = Math.Min(CurrentNew.OldLineStart, CurrentHis.OldLineStart);
            int maxOldStart = Math.Max(CurrentNew.OldLineStart + CurrentNew.DeletedInOld,
                        CurrentHis.OldLineStart + CurrentHis.DeletedInOld);
            int oldSpan = maxOldStart - minOldStart;

            return new Diff3Item(
                    minOldStart,
                    CurrentNew.NewLineStart + (minOldStart - CurrentNew.OldLineStart),
                    CurrentHis.NewLineStart + (minOldStart - CurrentHis.OldLineStart),
                    oldSpan,
                    CurrentNew.InsertedInNew + (oldSpan - CurrentNew.DeletedInOld),
                    CurrentHis.InsertedInNew + (oldSpan - CurrentHis.DeletedInOld),
                    DifferencesStatusEnum.AllDifferent
                );
        }
    }
}
