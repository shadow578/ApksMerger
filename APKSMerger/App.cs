using APKSMerger.AndroidRes;
using APKSMerger.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace APKSMerger
{
    public static class App
    {

        public static void Main(string[] args)
        {
            //Log.LogDebug = true;
            //Log.LogVerbose = true;
            //Log.LogVeryVerbose = true;

            new InteractiveUI().UiRoot();

#if DEBUG
            if (args.Length <= 0)
            {
                //override args on debug builds
                Console.Write("DEBUG Build, enter launch args (NO Spaces in args): app.exe ");
                string argsR = Console.ReadLine();
                if (!string.IsNullOrEmpty(argsR))
                {
                    args = argsR.Split(' ');
                }
                Console.Title = argsR;
            }
#endif

            //parse command line
            bool logDebug = false,
                logVerbose = false,
                logVVerbose = false;

            ParseFlags(args, ref logDebug, ref logVerbose, ref logVVerbose);

            //set log levels
            Log.LogDebug = logDebug || logVerbose || logVerbose;
            Log.LogVerbose = logVerbose || logVVerbose;
            Log.LogVeryVerbose = logVVerbose;

            //enter interactive mode
            EnterInteractive();
        }

        /// <summary>
        /// enter interactive mode
        /// </summary>
        static void EnterInteractive()
        {

        }

        /// <summary>
        /// parse the input and output paths from command line
        /// </summary>
        /// <param name="args">command line args to parse</param>
        /// <param name="inputPath">the input path</param>
        /// <param name="outputPath">the output path</param>
        static void ParseStrings(string[] args, ref string inputPath, ref string outputPath)
        {
            foreach (string arg in args)
            {
                //split arg on =
                string[] splits = arg.Split('=');
                if (splits.Length != 2) continue;

                if (splits[0].Equals("-input", StringComparison.OrdinalIgnoreCase)
                    || splits[0].Equals("-i", StringComparison.OrdinalIgnoreCase))
                {
                    inputPath = splits[1];
                }

                if (splits[0].Equals("-output", StringComparison.OrdinalIgnoreCase)
                    || splits[0].Equals("-o", StringComparison.OrdinalIgnoreCase))
                {
                    outputPath = splits[1];
                }
            }
        }

        /// <summary>
        /// parse logging related flags in command line
        /// </summary>
        /// <param name="args">command line args</param>
        /// <param name="debugLogging">is -debug flag set?</param>
        /// <param name="verboseLogging">is -verbose flag set?</param>
        /// <param name="veryVerboseLogging">is -vverbose flag set?</param>
        /// <param name="interactive">is -interactive flag set?</param>
        static void ParseFlags(string[] args, ref bool debugLogging, ref bool verboseLogging, ref bool veryVerboseLogging)
        {
            debugLogging = args.ContainsIgnoreCase("-debug") || args.ContainsIgnoreCase("-d");
            verboseLogging = args.ContainsIgnoreCase("-verbose") || args.ContainsIgnoreCase("-v");
            veryVerboseLogging = args.ContainsIgnoreCase("-vverbose") || args.ContainsIgnoreCase("-vv");
        }
    }
}
