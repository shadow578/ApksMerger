using APKSMerger.Util;
using System;
using System.Diagnostics;
using System.IO;

namespace APKSMerger
{
    /// <summary>
    /// wrapper for unapkm and apktool in lib directory
    /// </summary>
    public static class LibWrapper
    {
        /// <summary>
        /// path of JAVA_HOME
        /// </summary>
        static string JavaHome
        {
            get
            {
                return Environment.GetEnvironmentVariable("JAVA_HOME");
            }
        }

        /// <summary>
        /// path to system java.exe
        /// </summary>
        static string JavaExe
        {
            get
            {
                return Path.Combine(JavaHome, "bin", "java.exe");
            }
        }

        /// <summary>
        /// check if java is installed
        /// </summary>
        /// <returns>java installed?</returns>
        public static bool JavaInstalled()
        {
            //check JAVA_HOME not empty
            if (string.IsNullOrEmpty(JavaHome)) return false;

            //check java.exe exists
            return File.Exists(JavaExe);
        }

        /// <summary>
        /// run a command
        /// </summary>
        /// <param name="exe">the program to run</param>
        /// <param name="args">arguments for the program</param>
        /// <returns>did program exit with exitcode 0?</returns>
        static bool Run(string exe, string args)
        {
            //log what we're starting
            Log.v($"Calling {exe} args {args}");

            //prepare process with output redirect
            ProcessStartInfo pi = new ProcessStartInfo(exe, args)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            //start process
            using (Process p = new Process())
            {
                //prepare process
                p.StartInfo = pi;
                p.EnableRaisingEvents = true;
                p.ErrorDataReceived += LogExternalOutput;
                p.OutputDataReceived += LogExternalOutput;

                //start
                p.Start();

                //begin reading output
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();

                //wait until process exits
                p.WaitForExit();
                return p.ExitCode == 0;
            }
        }

        /// <summary>
        /// log console output of external calls (made in Run())
        /// </summary>
        static void LogExternalOutput(object sender, DataReceivedEventArgs e)
        {
            Log.d($"[OUT]: {e.Data}");
        }

        /// <summary>
        /// get the absolute path for a file in ./lib/ directory
        /// </summary>
        /// <param name="libName">the name of the file</param>
        /// <returns>the absolute path</returns>
        static string GetLibPath(string libName)
        {
            return Path.Combine(Environment.CurrentDirectory, "lib", libName);
        }

        /// <summary>
        /// Wrapper for apktool
        /// </summary>
        public static class ApkTool
        {
            /// <summary>
            /// compile a apktool project dir to a apk file
            /// </summary>
            /// <param name="projectDir">the directory to compile</param>
            /// <param name="outputApk">output apk file</param>
            /// <returns>was compile success?</returns>
            public static bool Compile(string projectDir, string outputApk)
            {
                //check input exists
                if (!Directory.Exists(projectDir)) return false;

                //run apktool
                return Run(JavaExe, @$"-jar ""{GetLibPath("apktool.jar")}"" b ""{projectDir}"" -o ""{outputApk}""");
            }

            /// <summary>
            /// decompile a apk file to apktool project
            /// </summary>
            /// <param name="inputApk">the apk to decompile</param>
            /// <param name="outputDir">the output directory</param>
            /// <returns>was decompile success?</returns>
            public static bool Decompile(string inputApk, string outputDir)
            {
                //check input exists
                if (!File.Exists(inputApk)) return false;

                //check output does not exist, delete if
                if (Directory.Exists(outputDir))
                    Directory.Delete(outputDir, true);

                //run apktool
                return Run(JavaExe, @$"-jar ""{GetLibPath("apktool.jar")}"" d ""{inputApk}"" -o ""{outputDir}""");
            }
        }

        /// <summary>
        /// wrapper for unapkm
        /// </summary>
        public static class UnAPKM
        {
            /// <summary>
            /// decode a .apkm file to .apks using unapkm
            /// </summary>
            /// <param name="input">input file, .apkm</param>
            /// <param name="output">output file, .apks</param>
            /// <returns>was decode success?</returns>
            public static bool Decode(string input, string output)
            {
                //check input exists
                if (!File.Exists(input)) return false;

                //delete output if exists
                if (File.Exists(output))
                    File.Delete(output);

                //run unapkm
                return Run(JavaExe, @$"-jar ""{GetLibPath("unapkm.jar")}"" ""{input}"" ""{output}""");
            }
        }
    }
}
