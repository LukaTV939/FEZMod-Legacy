﻿using System;
using Common;
using FezGame.Mod;

namespace FezGame {
	public class Program {

		public static void orig_Main(string[] args) {
		}

		public static void Main(string[] args) {
            FEZMod.PreInitialize(args);

			ModLogger.Log("JAFM", "Passing to FEZ...");
			orig_Main(args);
		}

        private static void orig_MainInternal() {
        }

        private static void MainInternal() {
            orig_MainInternal();
            FEZMod.Initialize();
        }

	}
}

