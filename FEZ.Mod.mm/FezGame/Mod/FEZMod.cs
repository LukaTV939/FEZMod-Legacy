﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Common;
using FezGame;
using FezEngine.Tools;
using FezEngine.Services;
using FezGame.Services;
using FezGame.Components;

namespace FezGame.Mod {
    public static class FEZMod {
        public static string Version = "0.0.5";

        public static List<FezModule> Modules = new List<FezModule>();

        public static bool IsAlwaysTurnable = false;
        public static float OverridePixelsPerTrixel = 0f;
        public static bool EnableDebugControls = false;

        public static void PreInitialize(string[] args) {
            PreInitialize();
            ParseArgs(args);
        }

        public static void PreInitialize() {
            ModLogger.Clear();
            ModLogger.Log("JAFM", "JustAnotherFEZMod (JAFM) "+FEZMod.Version);

            Fez.Version = FEZMod.Version;
            Fez.Version += " (JustAnotherFEZMod)";

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            PreInitializeModules();
        }

        public static void PreInitializeModules() {
            ModLogger.Log("JAFM", "Initializing FEZ mods...");
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                PreInitializeModules(assembly);
                foreach (AssemblyName reference in assembly.GetReferencedAssemblies()) {
                    PreInitializeModules(Assembly.Load(reference));
                }
            }
        }

        public static void PreInitializeModules(Assembly assembly) {
            foreach (Type type in assembly.GetTypes()) {
                if (typeof(FezModule).IsAssignableFrom(type) && !type.IsAbstract) {
                    PreInitializeModule(type);
                }
            }
        }

        public static void PreInitializeModule(Type type) {
            FezModule module = (FezModule) type.GetConstructor(new Type[0]).Invoke(new object[0]);
            ModLogger.Log("JAFM", "Pre-Initializing "+module.Name);
            module.PreInitialize();
            Modules.Add(module);
        }

        public static void ParseArgs(string[] args) {
            ModLogger.Log("JAFM", "Checking for custom arguments...");

            for (int i = 0; i < args.Length; i++) {
                if ((args[i] == "-l" || args[i] == "--load-level") && i+1 < args.Length) {
                    ModLogger.Log("JAFM", "Found -l / --load-level: "+args[i+1]);
                    Fez.ForcedLevelName = args[i+1];
                    //Fez.SkipLogos = true;
                    Fez.SkipIntro = true;
                }
                if (args[i] == "-lc" || args[i] == "--level-chooser") {
                    ModLogger.Log("JAFM", "Found -lc / --level-chooser");
                    Fez.LevelChooser = true;
                    //Fez.SkipLogos = true;
                    Fez.SkipIntro = true;
                }
                if (args[i] == "-ls" || args[i] == "--long-screenshot") {
                    if (i+1 < args.Length && args[i+1] == "double") {
                        ModLogger.Log("JAFM", "Found -ls / --long-screenshot double");
                        Fez.DoubleRotations = true;
                    } else {
                        ModLogger.Log("JAFM", "Found -ls / --long-screenshot");
                    }
                    Fez.LongScreenshot = true;
                }
                if (args[i] == "-pp" || args[i] == "--pixel-perfect") {
                    ModLogger.Log("JAFM", "Found -pp / --pixel-perfect");
                    OverridePixelsPerTrixel = 1f;
                }
                if (args[i] == "-dc" || args[i] == "--debug-controls") {
                    ModLogger.Log("JAFM", "Found -dc / --debug-controls");
                    EnableDebugControls = true;
                }
            }

            CallInEachModule("ParseArgs", new object[] {args});
        }

        public static void Initialize() {
            ServiceHelper.AddComponent(new FEZModComponent(ServiceHelper.Game));

            if (EnableDebugControls) {
                ServiceHelper.AddComponent(new DebugControls(ServiceHelper.Game));
            }

            CallInEachModule("Initialize", new object[0]);
        }

        private static void CallInEachModule(String methodName, object[] args) {
            Type[] argsTypes = Type.GetTypeArray(args);
            foreach (FezModule module in Modules) {
                module.GetType().GetMethod(methodName, argsTypes).Invoke(module, args);
            }
        }

    }
}

