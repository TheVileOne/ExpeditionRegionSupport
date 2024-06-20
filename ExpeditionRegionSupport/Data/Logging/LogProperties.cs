﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RWCustom;
using UnityEngine;

namespace ExpeditionRegionSupport.Data.Logging
{
    public class LogProperties
    {
        public static PropertyDataController PropertyManager
        {
            get => PropertyDataController.PropertyManager;
            set => PropertyDataController.PropertyManager = value;
        }

        /// <summary>
        /// Indicates that properties are in the process of being fetched/loaded
        /// </summary>
        public static bool LoadInProgress;

        public readonly LogID LogID;

        public readonly string ContainingFolderPath;


        /// <summary>
        /// A string representation of the content state. This is useful for preventing user sourced changes from being overwritten by mods
        /// </summary>
        public string Version;

        /// <summary>
        /// The filename that will be used in the typical write path for the log file
        /// </summary>
        public string Filename;

        /// <summary>
        /// The filename that will be used if the write path is the LogManager Logs directory. May be null if same as Filename
        /// </summary>
        public string AltFilename;

        /// <summary>
        /// A list of filenames that should be considered equal to Filename/AltFilename
        /// </summary>
        public string[] Aliases;

        private List<LogRule> _rules = new List<LogRule>();

        /// <summary>
        /// A prioritized order of process actions that must be applied to a message string before logging it to file 
        /// </summary>
        protected IOrderedEnumerable<LogRule> Rules => _rules.OrderBy(r => r.ApplyPriority);

        public LogProperties(string filename, string relativePathNoFile = "customroot")
        {
            Filename = filename;
            ContainingFolderPath = GetContainingPath(relativePathNoFile);
        }

        public LogProperties(LogID logID, string relativePathNoFile = "customroot")
        {
            LogID = logID;
            ContainingFolderPath = GetContainingPath(relativePathNoFile);
        }

        public void AddRule(LogRule rule)
        {
            _rules.Add(rule);
        }

        public bool HasPath(string path)
        {
            //TODO: Strip filename
            return LogUtils.ComparePaths(ContainingFolderPath, GetContainingPath(path));
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static LogProperties Deserialize(string propertyString)
        {
            return JsonConvert.DeserializeObject<LogProperties>(propertyString);
        }

        public static void LoadProperties()
        {
            if (LoadInProgress || PropertyManager != null) return;

            LoadInProgress = true;
            PropertyDataController.Initialize();
            LoadInProgress = false;
        }

        public static string GetContainingPath(string relativePathNoFile)
        {
            if (relativePathNoFile == null)
                return Application.streamingAssetsPath;

            relativePathNoFile = relativePathNoFile.ToLower();

            if (relativePathNoFile == "customroot")
                return Application.streamingAssetsPath;
            else if (relativePathNoFile == "root")
                return Application.dataPath; //TODO: Check that this path is correct

            if (Directory.Exists(relativePathNoFile)) //No need to change the path when it is already valid
                return relativePathNoFile;

            if (Custom.rainWorld != null)
            {
                string customPath = AssetManager.ResolveDirectory(relativePathNoFile); //This cannot be called too early in the load process

                if (Directory.Exists(customPath))
                    return customPath;
            }

            return Application.streamingAssetsPath; //Fallback path - Should register custom path later if it needs to be resolved through AssetManager
        }
    }
}
