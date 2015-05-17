﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CoreLibrary.Exceptions;
using CoreLibrary.Exceptions.NotFound;
using CoreLibrary.FilesystemDiffTree;
using CoreLibrary.FilesystemTree;
using CoreLibrary.Plugins;
using CoreLibrary.Plugins.Processors;
using CoreLibrary.Plugins.Processors.Settings;
using SvergeConsole.Printers;

namespace SvergeConsole
{
    /// <summary>
    /// Console user interface for Sverge
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Setting that shows help about how to user console interface to the user.
        /// </summary>
        [Settings("Show help about using the console.", "help", "h")]
        public static bool ShowHelp = false;

        /// <summary>
        /// An entry point for the user console interface.
        /// </summary>
        /// <param name="args">String array of command line arguments.</param>
        static void Main(string[] args)
        {
            #region Load all available processors and their settings

            IProcessorLoader loader = new ProcessorLoader();
            try
            {
                PluginsLoader.LoadAssemblies();
                // Load available processors and their settings
                loader.LoadAll();
                // Add special settings from this Program
                loader.RetrieveSettings(typeof(Program), true);
                
            } catch (ProcessorPriorityColissionException e)
            {
                Console.WriteLine("Processor " + e.Message + "could not be loaded because of a priority collision.");
            }

            var runner = new ProcessorRunner(loader);

            #endregion

            #region Parse arguments as settings
            try
            {
                // pass the arguments and parse them
                var parser = new SettingsParser(loader.GetSettings());
                args = parser.ParseSettings(args);
            } catch (SettingsNotFoundException e)
            {
                Console.WriteLine("This option has not been found: " + e.Message);
                return;
            } catch (SettingsUnknownValue e)
            {
                Console.WriteLine("This value for given option is invalid: " + e.Message);
                return;
            }
            #endregion

            #region Show help
            // if the help argument is passed - list all the available settings
            if (ShowHelp)
            {
                Console.WriteLine("Sverge – A Flexible Tool for Comparing & Merging [{0}]", GetVersion());
                Console.WriteLine("Usage: [OPTIONS]* <LOCAL> [BASE] <REMOTE>");
                Console.WriteLine("LOCAL, BASE, REMOTE must be paths to files or directories.");
                Console.WriteLine("BASE is optional.");

                Console.WriteLine();
                Console.WriteLine("Listing all found Processors and their parameters:");
                var processorPrinter = new ProcessorPrinter(loader, true);
                processorPrinter.Print();
                return;
            }
            #endregion

            #region Wrong number of arguments
            if (args.Length != 2 && args.Length != 3)
            {
                Console.WriteLine("You need to run the program with 2 or 3 paths as arguments.");
                Console.WriteLine("Show more information using '--help' option.");
                return;
            }
            #endregion

            #region Constructs simple mask - 1 if path is a file

            int areArgsFiles = 0;

            try
            {
                foreach (string s in args)
                {
                    areArgsFiles <<= 1;
                    areArgsFiles |= IsFile(s);
                }
            } catch (FileNotFoundException)
            { // do nothing, we will catch it later
            } catch (DirectoryNotFoundException)
            { // do nothing, we will catch it later
            }
            #endregion

            #region Creating main structure

            INodeVisitable diffTree;

            try
            {
                if (args.Length == 2 && areArgsFiles > 0)
                {
                    diffTree = new FileDiffNode(args[0], args[1]);
                } else if (args.Length == 3 && areArgsFiles > 0)
                {
                    diffTree = new FileDiffNode(args[0], args[1], args[2]);
                } else if (args.Length == 2 && areArgsFiles == 0)
                {
                    diffTree = new DiffCrawler().InitializeCrawler(args[0], args[1]).TraverseTree();
                } else if (args.Length == 3 && areArgsFiles == 0)
                {
                    diffTree = new DiffCrawler().InitializeCrawler(args[0], args[1], args[2]).TraverseTree();
                } else
                {
                    Console.WriteLine("You can not mix folders and files together as arguments.");
                    return;
                }

                #region Catch all possible NotFoundExceptions
            } catch (LocalFileNotFoundException e)
            {
                Console.WriteLine(e);
                return;
            } catch (BaseFileNotFoundException e)
            {
                Console.WriteLine(e);
                return;
            } catch (RemoteFileNotFoundException e)
            {
                Console.WriteLine(e);
                return;
            } catch (LocalDirectoryNotFoundException e)
            {
                Console.WriteLine(e);
                return;
            } catch (BaseDirectoryNotFoundException e)
            {
                Console.WriteLine(e);
                return;
            } catch (RemoteDirectoryNotFoundException e)
            {
                Console.WriteLine(e);
                return;
            }
                #endregion

            #endregion

            #region Run Processors

            runner.RunDiff(diffTree).Wait();

            // print the filesystem tree
            diffTree.Accept(new PrinterVisitor());

            Console.WriteLine("\nDo you want to run interactive processors? [Y/n]");
            string input = Console.ReadLine();
            if (input == null || input.Trim().ToUpperInvariant() != "Y") return;

            // run interactive diffing
            runner.RunInteractiveResolving(diffTree);

            Console.WriteLine("\nDo you want to run merging processors? [Y/n]");
            input = Console.ReadLine();
            if (input == null || input.Trim().ToUpperInvariant() != "Y") return;

            // run merging and syncing in parallel
            runner.RunMerge(diffTree).Wait();

            runner.RunDiff(diffTree).Wait();

            // print the filesystem tree
            diffTree.Accept(new PrinterVisitor());

            #endregion
        }

        /// <summary>
        /// Return assembly version for current program
        /// </summary>
        /// <returns>Current assembly version</returns>
        private static string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        /// <summary>
        /// Determine if the path is an existing file.
        /// </summary>
        /// <param name="path">Path to the file to be checked</param>
        /// <returns>1 if the path is an existing file.</returns>
        private static int IsFile(string path)
        {
            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(@path);

            //detect whether its a directory or file
            return (attr & FileAttributes.Directory) == FileAttributes.Directory ? 0 : 1;
        }
    }
}
