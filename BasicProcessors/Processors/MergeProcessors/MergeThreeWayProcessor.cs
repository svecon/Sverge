﻿using System.IO;
using CoreLibrary.Enums;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;
using DiffAlgorithm.ThreeWay;
using DiffIntegration.DiffFilesystemTree;

namespace BasicProcessors.Processors.MergeProcessors
{
    /// <summary>
    /// Processor for merging 3-way diffed files.
    /// </summary>
    [Processor(ProcessorTypeEnum.Merge, 301, DiffModeEnum.ThreeWay)]
    public class MergeThreeWayProcessor : ProcessorAbstract
    {
        [Settings("Output folder for the resulting merge.", "output-folder", "o")]
        public string OutputFolder;

        public enum DefaultActionEnum
        {
            WriteConflicts, RevertToBase, ApplyLocal, ApplyRemote
        }

        [Settings("Default action for merging files.", "3merge-default", "3d")]
        public DefaultActionEnum DefaultAction;

        protected override void ProcessChecked(IFilesystemTreeDirNode node)
        {
        }

        protected override bool CheckStatus(IFilesystemTreeFileNode node)
        {
            if (node.Status == NodeStatusEnum.WasMerged)
                return false;

            if ((LocationCombinationsEnum)node.Location != LocationCombinationsEnum.OnAll3)
                return false;

            return base.CheckStatus(node);
        }

        protected override void ProcessChecked(IFilesystemTreeFileNode node)
        {
            var dnode = node as DiffFileNode;

            if (dnode == null)
                return;

            if (dnode.Diff3 == null)
                return;

            node.Status = NodeStatusEnum.WasMerged;

            // create temporary file if the target file exists
            string temporaryPath;
            bool isTemporary = false;
            if (File.Exists(CreatePath(node)))
            {
                temporaryPath = CreatePath(node) + ".temp";
                isTemporary = true;
            } else
            {
                temporaryPath = CreatePath(node);
            }

            using (StreamReader localStream = ((FileInfo)dnode.InfoLocal).OpenText())
            using (StreamReader remoteStream = ((FileInfo)dnode.InfoRemote).OpenText())
            using (StreamReader baseStream = ((FileInfo)dnode.InfoBase).OpenText())
            using (StreamWriter writer = File.CreateText(temporaryPath))
            {
                int m = 0;
                int n = 0;
                int o = 0;

                foreach (Diff3Item diff in dnode.Diff3.Items)
                {
                    // change default action depending on processor settings
                    if (diff.PreferedAction == PreferedActionThreeWayEnum.Default)
                    {
                        switch (DefaultAction)
                        {
                            case DefaultActionEnum.WriteConflicts:
                                // keep default
                                break;
                            case DefaultActionEnum.RevertToBase:
                                diff.PreferedAction = PreferedActionThreeWayEnum.RevertToBase;
                                break;
                            case DefaultActionEnum.ApplyLocal:
                                diff.PreferedAction = PreferedActionThreeWayEnum.ApplyLocal;
                                break;
                            case DefaultActionEnum.ApplyRemote:
                                diff.PreferedAction = PreferedActionThreeWayEnum.ApplyRemote;
                                break;
                        }
                    }

                    // print between diffs
                    for (; o < diff.BaseLineStart; o++) { writer.WriteLine(baseStream.ReadLine()); }
                    for (; m < diff.LocalLineStart; m++) { localStream.ReadLine(); }
                    for (; n < diff.RemoteLineStart; n++) { remoteStream.ReadLine(); }

                    // if there is an action asociated:
                    if (diff.PreferedAction != PreferedActionThreeWayEnum.Default)
                    {
                        for (int p = 0; p < diff.LocalAffectedLines; p++)
                        {
                            m++;

                            if (diff.PreferedAction == PreferedActionThreeWayEnum.ApplyLocal)
                                writer.WriteLine(localStream.ReadLine());
                            else
                                localStream.ReadLine();
                        }
                        for (int p = 0; p < diff.BaseAffectedLines; p++)
                        {
                            o++;

                            if (diff.PreferedAction == PreferedActionThreeWayEnum.RevertToBase)
                                writer.WriteLine(baseStream.ReadLine());
                            else
                                baseStream.ReadLine();

                        }
                        for (int p = 0; p < diff.RemoteAffectedLines; p++)
                        {
                            n++;

                            if (diff.PreferedAction == PreferedActionThreeWayEnum.ApplyRemote)
                                writer.WriteLine(remoteStream.ReadLine());
                            else
                                remoteStream.ReadLine();
                        }

                        continue;
                    }

                    // print diffs
                    switch (diff.Differeces)
                    {
                        case DifferencesStatusEnum.BaseLocalSame:
                            for (int p = 0; p < diff.LocalAffectedLines; p++) { localStream.ReadLine(); m++; }
                            for (int p = 0; p < diff.BaseAffectedLines; p++) { baseStream.ReadLine(); o++; }
                            for (int p = 0; p < diff.RemoteAffectedLines; p++) { writer.WriteLine(remoteStream.ReadLine()); n++; }
                            break;
                        case DifferencesStatusEnum.BaseRemoteSame:
                            for (int p = 0; p < diff.LocalAffectedLines; p++) { writer.WriteLine(localStream.ReadLine()); m++; }
                            for (int p = 0; p < diff.BaseAffectedLines; p++) { baseStream.ReadLine(); o++; }
                            for (int p = 0; p < diff.RemoteAffectedLines; p++) { remoteStream.ReadLine(); n++; }
                            break;
                        case DifferencesStatusEnum.LocalRemoteSame:
                            for (int p = 0; p < diff.LocalAffectedLines; p++) { writer.WriteLine(localStream.ReadLine()); m++; }
                            for (int p = 0; p < diff.BaseAffectedLines; p++) { baseStream.ReadLine(); o++; }
                            for (int p = 0; p < diff.RemoteAffectedLines; p++) { remoteStream.ReadLine(); n++; }
                            break;
                        case DifferencesStatusEnum.AllDifferent:

                            if (diff.PreferedAction == PreferedActionThreeWayEnum.Default)
                            {
                                node.Status = NodeStatusEnum.HasConflicts;
                                writer.WriteLine("<<<<<<< " + dnode.InfoLocal.FullName);
                            }


                            for (int p = 0; p < diff.LocalAffectedLines; p++)
                            {
                                if (diff.PreferedAction == PreferedActionThreeWayEnum.ApplyLocal
                                    || diff.PreferedAction == PreferedActionThreeWayEnum.Default)
                                {
                                    writer.WriteLine(localStream.ReadLine());
                                } else
                                {
                                    localStream.ReadLine();
                                }
                                m++;
                            }

                            if (diff.PreferedAction == PreferedActionThreeWayEnum.Default)
                                writer.WriteLine("||||||| " + dnode.InfoBase.FullName);


                            for (int p = 0; p < diff.BaseAffectedLines; p++)
                            {
                                if (diff.PreferedAction == PreferedActionThreeWayEnum.RevertToBase
                                    || diff.PreferedAction == PreferedActionThreeWayEnum.Default)
                                {
                                    writer.WriteLine(baseStream.ReadLine());
                                } else
                                {
                                    baseStream.ReadLine();
                                }
                                o++;
                            }

                            if (diff.PreferedAction == PreferedActionThreeWayEnum.Default)
                                writer.WriteLine("=======");


                            for (int p = 0; p < diff.RemoteAffectedLines; p++)
                            {
                                if (diff.PreferedAction == PreferedActionThreeWayEnum.ApplyRemote
                                    || diff.PreferedAction == PreferedActionThreeWayEnum.Default)
                                {
                                    writer.WriteLine(remoteStream.ReadLine());
                                } else
                                {
                                    remoteStream.ReadLine();
                                }
                                n++;
                            }


                            if (diff.PreferedAction == PreferedActionThreeWayEnum.Default)
                                writer.WriteLine(">>>>>>> " + dnode.InfoRemote.FullName);

                            break;
                    }
                }

                // print end
                for (; o < dnode.Diff3.FilesLineCount.Base; o++) { writer.WriteLine(baseStream.ReadLine()); }
                //for (; m < dnode.Diff3.FilesLineCount.Local; m++) { localStream.ReadLine(); }
                //for (; n < dnode.Diff3.FilesLineCount.Remote; n++) { remoteStream.ReadLine(); }
            }

            // copy temporary file to correct location
            if (!isTemporary) return;

            File.Delete(CreatePath(node));
            File.Move(temporaryPath, CreatePath(node));
        }

        private void CheckAndCreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private string CreatePath(IFilesystemTreeFileNode node)
        {
            string output = OutputFolder == null
                ? node.GetAbsolutePath(LocationEnum.OnBase)
                : Path.Combine(OutputFolder, node.Info.Name);

            CheckAndCreateDirectory(Path.GetDirectoryName(output));

            return output;
        }
    }
}
