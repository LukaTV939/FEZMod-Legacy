﻿using System;
using FezGame.Mod;
using System.Collections.Generic;
using System.IO;
using FezEngine.Structure;
using FezEngine.Services;

namespace FezEngine.Tools {
    public class MemoryContentManager {

        private static IEnumerable<string> assetNames;
        public static IEnumerable<string> get_AssetNames() {
            if (!CacheDisabled) {
                return MemoryContentManager.cachedAssets.Keys;
            } else {
                if (assetNames == null) {
                    List<string> files = TraverseThrough("Resources");
                    List<string> assets = new List<string>(files.Count);
                    foreach (string file in files) {
                        assets.Add(file.Substring(10, file.Length-14).Replace("/", "\\"));
                    }
                    assetNames = assets;
                }
                return assetNames;
            }
        }

        private static List<string> TraverseThrough(string dir) {
            return TraverseThrough(new List<string>(), dir);
        }

        private static List<string> TraverseThrough(List<string> list, string dir) {
            if (!Directory.Exists(dir)) {
                return list;
            }

            foreach (string subdir in Directory.GetDirectories(dir)) {
                list = TraverseThrough(list, subdir);
            }

            foreach (string file in Directory.GetFiles(dir)) {
                list.Add(file);
            }

            return list;
        }

        private static Dictionary<String, byte[]> cachedAssets;

        public static bool DumpResources = false;
        public static bool DumpAllResources = false;
        private static int DumpAllResourcesCount = 0;

        public static bool CacheDisabled = false;

        public static void DumpAll() {
            if (cachedAssets == null) {
                ModLogger.Log("FEZMod.Engine", "Cached assets do not exist; ignoring...");
                return;
            }

            int dumped = 0;

            int count = cachedAssets.Count;
            if (count <= DumpAllResourcesCount) {
                return;
            }
            DumpAllResourcesCount = count;
            ModLogger.Log("FEZMod.Engine", "Dumping "+count+" assets...");
            String[] assetNames_ = new String[count];
            cachedAssets.Keys.CopyTo(assetNames_, 0);

            for (int i = 0; i < count; i++) {
                byte[] bytes = cachedAssets[assetNames_[i]];
                string assetName = assetNames_[i].ToLower();
                string filePath = assetName.Externalize() + ".xnb";
                FileInfo file = new FileInfo(filePath);
                if (!file.Exists) {
                    file.Directory.Create();
                    ModLogger.Log("FEZMod.Engine", (i+1)+" / "+count+": "+assetName+" -> "+filePath);
                    FileStream fos = new FileStream(filePath, FileMode.CreateNew);
                    fos.Write(bytes, 0, bytes.Length);
                    fos.Close();
                    dumped++;
                }
            }

            ModLogger.Log("FEZMod.Engine", "Dumped: "+dumped+" / "+count);
        }

        protected Stream orig_OpenStream(string assetName) {
            return null;
        }

        protected Stream OpenStream(string assetName) {
            if (DumpAllResources) {
                DumpAll();
            }

            string filePath = assetName.Externalize() + ".xnb";
            FileInfo file = new FileInfo(filePath);
            if (file.Exists) {
                FileStream fis = new FileStream(filePath, FileMode.Open);
                return fis;
            } else if (DumpResources) {
                file.Directory.Create();
                ModLogger.Log("FEZMod.Engine", assetName+" -> "+filePath);
                Stream ois = orig_OpenStream(assetName);
                FileStream fos = new FileStream(filePath, FileMode.CreateNew);
                ois.CopyTo(fos);
                ois.Close();
                fos.Close();
            }
            return orig_OpenStream(assetName);
        }

        public static bool orig_AssetExists(string assetName) {
            return false;
        }

        public static bool AssetExists(string assetName) {
            if (assetName == "FEZMOD_WORKAROUND_DUMP") {
                DumpResources = true;
                return true;
            }
            if (assetName == "FEZMOD_WORKAROUND_DUMPALL") {
                DumpAllResources = true;
                return true;
            }
            if (assetName == "FEZMOD_WORKAROUND_NOCACHE") {
                CacheDisabled = true;
                return true;
            }
            if (assetName == "FEZMOD_WORKAROUND_NOFLAT") {
                Level.FlatDisabled = true;
                return true;
            }
            if (assetName == "FEZMOD_WORKAROUND_CUSTOMMUSICEXTRACT") {
                SoundManager.ExtractCustom = true;
                return true;
            }
            if (assetName == "FEZMOD_WORKAROUND_NOMUSICEXTRACT") {
                SoundManager.ExtractDisabled = true;
                return true;
            }

            if (File.Exists(assetName.Externalize() + ".xnb")) {
                return true;
            }
            if (File.Exists(assetName.Externalize() + ".ogg")) {
                return true;
            }
            if (File.Exists(assetName.Externalize() + ".png")) {
                return true;
            }
            if (File.Exists(assetName.Externalize() + ".jpg")) {
                return true;
            }
            if (File.Exists(assetName.Externalize() + ".jpeg")) {
                return true;
            }
            if (File.Exists(assetName.Externalize() + ".gif")) {
                return true;
            }

            return orig_AssetExists(assetName);
        }

        public void orig_LoadEssentials() {
        }

        public void LoadEssentials() {
            if (!CacheDisabled) {
                orig_LoadEssentials();
            } else {
                cachedAssets = new Dictionary<string, byte[]>(0);
            }
            FEZMod.LoadEssentials();
        }

        public void orig_Preload() {
        }

        public void Preload() {
            if (!CacheDisabled) {
                orig_Preload();
            }
            FEZMod.Preload();
        }

    }
}

