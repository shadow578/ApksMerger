using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using APKSMerger.AndroidRes.Model;
using APKSMerger.AndroidRes.Model.Generic;
using APKSMerger.Util;

namespace APKSMerger.AndroidRes
{
    /// <summary>
    /// merges android resource files
    /// </summary>
    public sealed class AndroidResourceMerger
    {
        /// <summary>
        /// merge all splits into the base project dir
        /// </summary>
        /// <param name="baseDir">base project dir</param>
        /// <param name="splits">split dirs to merge</param>
        public void MergeSplits(DirectoryInfo baseDir, params DirectoryInfo[] splits)
        {
            //check all dirs exists
            if (!baseDir.Exists)
            {
                Log.e($"baseDir {baseDir.FullName} does not exist!");
                return;
            }

            foreach (DirectoryInfo dir in splits)
            {
                if (!dir.Exists)
                {
                    Log.e($"split dir {dir.FullName} dos not exist!");
                    return;
                }
            }

            //enumarate all splitted files
            Dictionary</*original*/string, /*replacement*/string> globalNameReplacements = new Dictionary<string, string>();
            foreach (DirectoryInfo split in splits)
            {
                split.EnumerateAllFiles("*.*", true, (FileInfo splittedFile) =>
                {
                    //check if should process
                    string splitRel = Path.GetRelativePath(split.FullName, splittedFile.FullName);
                    if (!ShouldProcess(splittedFile, split))
                    {
                        Log.v($"skip excluded split file {splitRel}");
                        return;
                    }

                    //get file path for base dir
                    FileInfo baseFile = new FileInfo(Path.Combine(baseDir.FullName, splitRel));

                    //create target dir in base if needed
                    string baseFileDir = Path.GetDirectoryName(baseFile.FullName);
                    if (!Directory.Exists(baseFileDir))
                    {
                        Directory.CreateDirectory(baseFileDir);
                    }

                    //check file exists in base and is resource xml
                    if (!baseFile.Exists || !IsResourceXml(baseFile))
                    {
                        //nothing to merge, just copy
                        Log.v($"move split file {splitRel} to base...");
                        splittedFile.MoveTo(baseFile.FullName, true);
                    }
                    else
                    {
                        //already exists, merge
                        Log.v($"merge split file {splitRel} with base...");

                        //skip if files are equal
                        if (baseFile.HasSameHash(splittedFile))
                        {
                            Log.vv($"base and split of {splitRel} have same hash, skipping...");
                            return;
                        }

                        //check base and split are both resource xmls, if not skip
                        if (/*!IsResourceXml(baseFile) ||*/ !IsResourceXml(splittedFile))
                        {
                            Log.vv($"split of {splitRel} is not resource xml, skipping...");
                            return;
                        }

                        //merge
                        MergeResourceXML(baseFile, splittedFile, globalNameReplacements);
                    }
                });
            }

            //skip replacement if no global name replacements are available
            if (globalNameReplacements.Count <= 0)
            {
                Log.d("skip global name replacements: count is 0");
            }

            //replace names globally (in xml only)
            Log.d($"process {globalNameReplacements.Count} global name replacements...");
            foreach (string org in globalNameReplacements.Keys)
            {
                Log.v($"replace {org} with {globalNameReplacements[org]}");
            }
            baseDir.EnumerateAllFiles("*.xml", true, (FileInfo file) =>
            {
                Log.vv($"name replace in {file.FullName}");

                //create temp file
                FileInfo temp = new FileInfo(Path.GetTempFileName());

                //copy from input to temp, replace everything on replace list
                using (StreamReader inp = file.OpenText())
                using (StreamWriter oup = temp.CreateText())
                {
                    string ln;
                    while ((ln = inp.ReadLine()) != null)
                    {
                        //replace all
                        foreach (string org in globalNameReplacements.Keys)
                        {
                            ln = ln.Replace(org, globalNameReplacements[org]);
                        }

                        //write back
                        oup.WriteLine(ln);
                    }
                }

                //move temp to input and delete temp if still exists
                string tempPath = temp.FullName;
                temp.MoveTo(file.FullName, true);

                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            });
        }

        /// <summary>
        /// merge two splitted resource xmls, overwrite a with merged
        /// </summary>
        /// <param name="a">file a to merge</param>
        /// <param name="b">file b to merge</param>
        /// <param name="globalNameReplacements">dictionary that can be used to replace names of resources globally</param>
        void MergeResourceXML(FileInfo a, FileInfo b, Dictionary</*original*/string, /*replacement*/string> globalNameReplacements)
        {
            //deserialize both
            AndroidResources resBase = AndroidResources.FromFile(a.FullName);
            AndroidResources resSplit = AndroidResources.FromFile(b.FullName);

            //merge resources to resA
            foreach (AndroidResource res in resSplit.Values)
            {
                if (res is AndroidPublic splitP)
                {
                    //entry of public.xml, special merge (Id has to be unique)
                    //try to find public with same id in base apk
                    AndroidPublic baseP = resBase.FindPublicWithId(splitP.Id);
                    if (baseP == null || !baseP.Type.Equals(splitP.Type))
                    {
                        //id not found or wrong type, add from split
                        resBase.Values.Add(splitP);
                    }
                    else
                    {
                        //id with correct ypefound in base, 
                        //check if name of base is apktool dummy and name of split is not
                        if (baseP.Name.StartsWith("APKTOOL_DUMMY") && !splitP.Name.StartsWith("APKTOOL_DUMMY"))
                        {
                            globalNameReplacements.Add(baseP.Name, splitP.Name);
                            baseP.Name = splitP.Name;
                        }
                    }
                }
                else
                {
                    //normal resource entry (string / color / ...)
                    if (!resBase.Values.Contains(res))
                    {
                        resBase.Values.Add(res);
                    }
                }
            }

            //serialize back to a
            resBase.ToFile(a.FullName);
        }

        /// <summary>
        /// check if the xml file contains the resources xml tag
        /// </summary>
        /// <param name="xml">the xml to check</param>
        /// <returns>does the xml contain the tag?</returns>
        bool IsResourceXml(FileInfo f)
        {
            //check file exists
            if (!f.Exists) return false;

            try
            {
                //check xml root
                XmlDocument xml = new XmlDocument();
                xml.Load(f.FullName);

                return xml.DocumentElement.Name.Equals("resources", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception _)
            {
                //probably bad xml
                return false;
            }
        }

        /// <summary>
        /// should the file be processed?
        /// Example for files to exclude from processing are AndroidManifest.xml, apktool.yml, and META-INF/*
        /// </summary>
        /// <param name="file">the file to check</param>
        /// <param name="projDir">the project dir the file is in</param>
        /// <returns>process the file?</returns>
        bool ShouldProcess(FileInfo file, DirectoryInfo projDir)
        {
            //get relative path
            string filePathRel = Path.GetRelativePath(projDir.FullName, file.FullName).TrimStart('/').TrimStart('\\');

            //check if in META-INF (exclude all)
            if (filePathRel.StartsWith("META-INF", StringComparison.OrdinalIgnoreCase))
                return false;

            //check if in original (exlude all)
            if (filePathRel.StartsWith("original", StringComparison.OrdinalIgnoreCase))
                return false;

            //check if AndroidManifest.xml OR apktool.yml
            if (file.Name.Equals("androidmanifest.xml", StringComparison.OrdinalIgnoreCase)
                || file.Name.Equals("apktool.yml", StringComparison.OrdinalIgnoreCase))
                return false;

            //all ok, include
            return true;
        }
    }
}
