using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace MarsarahTweaks
{
	[BepInPlugin(ModGUID, ModName, ModVersion)]
	public class MarsarahTweaks : BaseUnityPlugin
	{
		internal const string ModName = "MarsarahTweaks";
		internal const string ModVersion = "0.1.0";
		internal const string Author = "Marsarah";
		public const string ModGUID = Author + "." + ModName;
		public static string ConfigFileName = ModGUID + ".cfg";
		public static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

		private readonly Harmony harmony = new Harmony(ModGUID);

		void Awake()
		{
			harmony.PatchAll();
		}
	}
}
