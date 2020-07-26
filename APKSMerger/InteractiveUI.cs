using APKSMerger.AndroidRes;
using APKSMerger.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace APKSMerger
{
    public class InteractiveUI
    {
        /// <summary>
        /// random instance 
        /// </summary>
        Random random = new Random();

        /// <summary>
        /// merger instance
        /// </summary>
        AndroidMerger merger = new AndroidMerger();

        /// <summary>
        /// root of interactive ui
        /// </summary>
        public void UiRoot()
        {
            //nothing to see here ;)
            string title = (random.Next(0, 100) < 10) ? "Choose your fighter" : "Choose operation";
            BranchMenu(title,
                new string[] { "merge .apks to .apk", "merge .apkm to .apk", "merge decompiled directories to .apk", "set global log level" },
                new Action[] { () => DecompileApks(), DecodeApkm, ChooseProjectDirs, SetLogLevel });
        }

        /// <summary>
        /// set the logging level, go back to root afterwards
        /// </summary>
        void SetLogLevel()
        {
            switch (ShowMenu("Set Log level", "Info", "Debug", "Verbose", "Very Verbose"))
            {
                case 0:
                    Log.LogDebug = false;
                    Log.LogVerbose = false;
                    Log.LogVeryVerbose = false;
                    break;
                case 1:
                    Log.LogDebug = true;
                    Log.LogVerbose = false;
                    Log.LogVeryVerbose = false;
                    break;
                case 2:
                    Log.LogDebug = true;
                    Log.LogVerbose = true;
                    Log.LogVeryVerbose = false;
                    break;
                case 3:
                    Log.LogDebug = true;
                    Log.LogVerbose = true;
                    Log.LogVeryVerbose = true;
                    break;
            }

            //go back
            UiRoot();
        }

        /// <summary>
        /// Decode a .apkm file, continue to decompileAPks()
        /// </summary>
        void DecodeApkm()
        {
            //get path
            string apkm = GetPath("enter path to .apkm file", true);

            //decode apkm, use same but change extension
            string apks = Path.Combine(Path.GetDirectoryName(apkm), Path.GetFileNameWithoutExtension(apkm) + ".apks");
            using (new WaitSpinner())
            {
                if (!LibWrapper.UnAPKM.Decode(apkm, apks))
                {
                    Console.WriteLine("decode failed!");
                    return;
                }
            }

            //continue
            DecompileApks(apks);
        }

        /// <summary>
        /// extract and decompile a apks file, continue to showCapabilities()
        /// </summary>
        /// <param name="apks">apks file to decompile</param>
        void DecompileApks(string apks = "")
        {
            //get path to apks
            if (string.IsNullOrEmpty(apks))
            {
                apks = GetPath("enter path to .apks file", true);
            }

            string baseDir = null;
            List<string> splitDirs = new List<string>();
            using (new WaitSpinner())
            {
                //prepare project directory in same dir as apks file
                string projectDirRoot = Path.Combine(Path.GetDirectoryName(apks), "merge");
                string apksExtractDir = Path.Combine(projectDirRoot, "extracted");

                //create dirs
                Directory.CreateDirectory(apksExtractDir);

                //extract apks file
                ZipFile.ExtractToDirectory(apks, apksExtractDir, true);

                //decompile all apk files in extraction directory, try to find base apk
                //base apk is the one that is not called split_config.*
                bool autoFindBase = true;
                foreach (string apk in Directory.EnumerateFiles(apksExtractDir, "*.apk"))
                {
                    //prepare output directory in project root
                    string decompileDir = Path.Combine(projectDirRoot, Path.GetFileNameWithoutExtension(apk));
                    Directory.CreateDirectory(decompileDir);

                    //decompile with apktool
                    if (!LibWrapper.ApkTool.Decompile(apk, decompileDir))
                    {
                        Console.WriteLine($"decompile {apk} failed!");
                        return;
                    }

                    //add to lists
                    bool isBase = !Path.GetFileName(apk).StartsWith("split");
                    if (isBase && baseDir != null)
                    {
                        Console.WriteLine("found two base! please enter base directory manually later.");
                        splitDirs.Add(baseDir);
                        autoFindBase = false;
                    }

                    if (isBase && autoFindBase)
                    {
                        Console.WriteLine($"{decompileDir} detected as base");
                        baseDir = decompileDir;
                    }
                    else
                    {
                        Console.WriteLine($"{decompileDir} detected as split");
                        splitDirs.Add(decompileDir);
                    }
                }

                //manually choose base dir if auto find failed
                if (baseDir == null || !autoFindBase)
                {
                    Console.WriteLine("could not auto- detect base dir! please enter manually");
                    baseDir = GetPath("base dir: ", true, true);

                    //remove from splitDirs
                    splitDirs.Remove(baseDir);
                }

                //remove extracted files
                Directory.Delete(apksExtractDir, true);
            }

            //continue to merge
            ShowCapabilities(baseDir, splitDirs);
        }

        /// <summary>
        /// let user choose decompiled project dirs, continue to showCapabilities()
        /// </summary>
        void ChooseProjectDirs()
        {
            //get base dir
            string baseDir = GetPath("enter base dir:", true, true);

            //get split dirs
            List<string> splitDirs = new List<string>();
            while (true)
            {
                //print previous splits
                Console.Clear();
                Console.WriteLine("Base:");
                Console.WriteLine($" {baseDir}");

                if (splitDirs.Count > 0)
                {
                    Console.WriteLine("Splits: ");
                    foreach (string split in splitDirs)
                    {
                        Console.WriteLine($" {split}");
                    }
                }

                Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Console.WriteLine("Drag & Drop next split here. empty to stop");
                string newSplit = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newSplit)) break;

                //check exists and is not added
                if (Directory.Exists(newSplit)
                    && !splitDirs.Contains(newSplit))
                    splitDirs.Add(newSplit);
            }

            //remove base from splits if its in there
            if (splitDirs.Remove(baseDir))
            {
                Console.WriteLine("baseDir was included in splits! removeing...");
            }

            //continue
            ShowCapabilities(baseDir, splitDirs);
        }

        /// <summary>
        /// show capabilities of merged apk, and merge apk
        /// </summary>
        /// <param name="_baseDir">base apk dir</param>
        /// <param name="_splitDirs">split directories</param>
        void ShowCapabilities(string _baseDir, List<string> _splitDirs)
        {
            //get directories as DirectoryInfo
            DirectoryInfo baseDir = new DirectoryInfo(_baseDir);
            if (!baseDir.Exists)
            {
                Console.WriteLine($"{_baseDir} was not found!");
                return;
            }

            List<DirectoryInfo> splitDirs = new List<DirectoryInfo>();
            foreach (string split in _splitDirs)
            {
                DirectoryInfo splitI = new DirectoryInfo(split);
                if (!splitI.Exists)
                {
                    Console.WriteLine($"{split} was not found!");
                    return;
                }

                //add to list
                splitDirs.Add(splitI);
            }

            //get capabilities
            Dictionary<string, string> locales, abis;
            using (new WaitSpinner())
            {
                merger.CollectCapabilities(out locales, out abis, baseDir, splitDirs.ToArray());
            }

            //print capabilities
            Console.Clear();
            Console.WriteLine("~~ Details ~~");
            Console.WriteLine($"Base:");
            Console.WriteLine($" {_baseDir}");
            Console.WriteLine("Splits:");
            foreach (string split in _splitDirs)
            {
                Console.WriteLine($" {split}");
            }

            Console.WriteLine("\n\n~~ Capabilities ~~");
            Console.WriteLine("supported locales:");
            foreach (string locale in locales.Keys)
            {
                Console.WriteLine($"- {locale} (in {locales[locale]})");
            }

            Console.WriteLine("\nsupported abis:");
            foreach (string abi in abis.Keys)
            {
                Console.WriteLine($"- {abi} (in {abis[abi]})");
            }

            //check common abis
            Console.WriteLine("\n\n");
            if (!abis.ContainsKey(@"arm64-v8a"))
                WriteLineColored("Warning: merged apk will not support arm64 (= current devices)", ConsoleColor.Red);
            if (!abis.ContainsKey(@"armeabi-v7a"))
                WriteLineColored("Warning: merged apk will not support arm (= legacy devices)", ConsoleColor.DarkYellow);
            if (!abis.ContainsKey(@"x86"))
                WriteLineColored("Warning: merged apk will not support x86 (= legacy devices / emulators)", ConsoleColor.DarkYellow);
            //dont warn for x86_64, noone uses that anyways

            //ask for merge
            if (ShowMenu("Do you want to merge to one apk?", "yes, merge", "no, exit") != 0) return;

            //do merge
            Console.Clear();
            Console.WriteLine("merging apks...");
            using (new WaitSpinner())
            {
                merger.MergeSplits(baseDir, splitDirs.ToArray());
            }
            Console.WriteLine("\n\nmerge finished!\n<ENTER> to continue");
            Console.ReadLine();
            Console.Clear();

            //remove split dirs
            if (ShowMenu("Clean up split directories?", "yes", "no") == 0)
            {
                foreach (DirectoryInfo dir in splitDirs)
                {
                    Console.WriteLine($"remove {dir.Name}...");
                    dir.Delete(true);
                }
            }

            //continue
            ExitOrRecompile(_baseDir);
        }

        /// <summary>
        /// exit the app or recompile the base apk
        /// </summary>
        /// <param name="baseDir">base apk dir</param>
        void ExitOrRecompile(string baseDir)
        {
            //ask for recompile
            Console.Clear();
            if (ShowMenu("Do you want to recompile the apk?", "yes, recompile", "no, exit") != 0) return;

            //recompile
            using (new WaitSpinner())
            {
                //prepare apk output name
                string apkOutput = Path.Combine(Directory.GetParent(baseDir).FullName, "merged-unsigned.apk");

                //compile
                if (!LibWrapper.ApkTool.Compile(baseDir, apkOutput))
                {
                    Console.WriteLine("compile failed!");
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("compile finished.");
                }
            }
        }


        /// <summary>
        /// get a path to a file or directory
        /// </summary>
        /// <param name="title">title of the prompt</param>
        /// <param name="hasToExist">check if the file / directory exists?</param>
        /// <param name="getDirectory">aks for a file or directory? (false = file, true = dir)</param>
        /// <returns>path to the file / directory</returns>
        string GetPath(string title, bool hasToExist, bool getDirectory = false)
        {
            string path;
            do
            {
                //read string from user
                Console.Write($"{title}: ");
                path = Console.ReadLine();

                //check if path is empty
                if (string.IsNullOrWhiteSpace(path)) continue;

                //check if is directory
                if (IsDirectory(path))
                {
                    //is a directory
                    //did we expect a directory?
                    if (!getDirectory)
                    {
                        Console.WriteLine("file path needed!");
                        continue;
                    }

                    //does exist? (if it has to)
                    if (hasToExist && !Directory.Exists(path))
                    {
                        Console.WriteLine("directory not found!");
                        continue;
                    }
                }
                else
                {
                    //is a file
                    //did we expect a file?
                    if (getDirectory)
                    {
                        Console.WriteLine("directory path needed!");
                        continue;
                    }

                    //does exist? (if it has to)
                    if (hasToExist && !File.Exists(path))
                    {
                        Console.WriteLine("file not found!");
                        continue;
                    }
                }

                //all ok
                return path;
            } while (true);
        }

        /// <summary>
        /// use ShowMenu() to branch between methods
        /// </summary>
        /// <param name="title">title to display for the menu</param>
        /// <param name="labels">labels for branch targets</param>
        /// <param name="targets">branch targets</param>
        void BranchMenu(string title, string[] labels, Action[] targets)
        {
            if (labels.Length != targets.Length)
                throw new ArgumentException("labels and targets lenght has to be equal!");

            //show menu
            int res = ShowMenu(title, labels);
            targets[res].Invoke();
        }

        /// <summary>
        /// show a menu in the console, that can be controlled using the arrow keys
        /// </summary>
        /// <param name="title">title to display for the menu</param>
        /// <param name="options">options in the menu</param>
        /// <returns>selected index in options</returns>
        int ShowMenu(string title, params string[] options)
        {
            int initialTop = Console.CursorTop,
                initialLeft = Console.CursorLeft;
            int currentSelection = 0;
            do
            {
                //render menu
                //Console.Clear();
                //Console.SetCursorPosition(0, 0);
                Console.SetCursorPosition(initialLeft, initialTop);
                Console.WriteLine(title);
                Console.WriteLine('~'.Repeat(title.Length));

                for (int i = 0; i < options.Length; i++)
                {
                    Console.WriteLine($"{(currentSelection == i ? " >" : "  ")} {options[i]}");
                }

                Console.WriteLine('~'.Repeat(title.Length));
                Console.WriteLine("UP / DOWN to move, ENTER to select");

                //listen for key press
                ConsoleKeyInfo keyPress = Console.ReadKey(true);
                switch (keyPress.Key)
                {
                    case ConsoleKey.DownArrow:
                    {
                        currentSelection++;
                        if (currentSelection >= options.Length)
                            currentSelection = 0;
                        break;
                    }
                    case ConsoleKey.UpArrow:
                    {
                        currentSelection--;
                        if (currentSelection < 0)
                            currentSelection = options.Length - 1;
                        break;
                    }
                    case ConsoleKey.Enter:
                    {
                        Console.Clear();
                        return currentSelection;
                    }
                }
            } while (true);
        }

        /// <summary>
        /// is this path a directory?
        /// </summary>
        /// <param name="path">the path to check</param>
        /// <returns>is a directory?</returns>
        bool IsDirectory(string path)
        {
            return string.IsNullOrWhiteSpace(Path.GetExtension(path));
        }

        /// <summary>
        /// writes a colored message to console
        /// </summary>
        /// <param name="s">the string to log</param>
        /// <param name="color">color to log in</param>
        static void WriteLineColored(string s, ConsoleColor color)
        {
            //set color
            ConsoleColor iColor = Console.ForegroundColor;
            Console.ForegroundColor = color;

            //write log
            Console.WriteLine(s);

            //restore color
            Console.ForegroundColor = iColor;
        }
    }

    /// <summary>
    /// wait spinner OoO
    /// </summary>
    public class WaitSpinner : IDisposable
    {
        /// <summary>
        /// stages of the spinner
        /// </summary>
        readonly string[] stages = { "/", "-", "\\", "|" };

        /// <summary>
        /// task that renders the spinner
        /// </summary>
        Task renderTask;

        /// <summary>
        /// request the render task to stop
        /// </summary>
        bool requestStop = false;

        /// <summary>
        /// position of the spinner
        /// </summary>
        int top, left;

        /// <summary>
        /// start the spinner
        /// </summary>
        /// <param name="left">left position of spinner</param>
        /// <param name="top">top position of spinner</param>
        public WaitSpinner(int left = 0, int top = 0)
        {
            this.top = top;
            this.left = left;
            renderTask = Task.Run(RenderTask);
        }

        /// <summary>
        /// Render task for the spinner
        /// </summary>
        async void RenderTask()
        {
            int currentStage = 0;
            while (!requestStop)
            {
                //get original cursor pos
                //int iTop = Console.CursorTop,
                //    iLeft = Console.CursorLeft;

                //move to target pos and write
                //Console.SetCursorPosition(left, top);
                //Console.Write(stages[currentStage]);

                //reset cursor pos
                //Console.SetCursorPosition(iLeft, iTop);

                //get cursor left
                int iLeft = Console.CursorLeft;

                //write cursor at left = 0 
                Console.CursorLeft = 0;
                Console.Write(stages[currentStage]);

                //restore cursor left
                Console.CursorLeft = iLeft;

                //update current stage
                currentStage++;
                if (currentStage >= stages.Length)
                    currentStage = 0;


                //wait
                await Task.Delay(100);
            }
        }

        /// <summary>
        /// stop the spinner
        /// </summary>
        public void Dispose()
        {
            requestStop = true;
            renderTask.Wait();
        }
    }
}
