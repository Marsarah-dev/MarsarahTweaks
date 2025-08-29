//using HarmonyLib;
//using Jotunn.Configs;
//using MarsarahTweaks.Managers;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace MarsarahTweaks.Patches.BuildPieces
//{
//	internal class MysticalLightWard
//	{
//		private static readonly LogManager log = new LogManager("Mystical Light Ward", LogManager.LogLevel.Info);

//		private static bool initialized = false;
//		private static GameObject MysticalWardPrefab;

//		[HarmonyPatch(typeof(ZNetScene), "Awake")]
//		public static class ZNetScene_Awake_Patch
//		{
//			static void Postfix(ZNetScene __instance)
//			{
//				if (__instance == null || initialized)
//					return;

//				initialized = true;

//				CreateMysticalWard();
//			}
//		}

//		private static void CreateMysticalWard()
//		{
//			if (MPrefabManager.GetPrefab("mystical_ward") != null) return;

//			// Clone ward
//			MysticalWardPrefab = CloneWardPrefab("guard_stone", "mystical_ward");
//			if (MysticalWardPrefab == null) return;

//			// Apply ward-specific data
//			SetupMysticalWardDefaults(MysticalWardPrefab, "Mystical Light Ward", "piece_workbench");

//			// Decrease ward size
//			ScaleMysticalWard(MysticalWardPrefab);

//			// Modify mystical ward icon
//			ModifyMysticalWardIcon(MysticalWardPrefab);

//			// Add mystical ward to ZNetScene
//			MPrefabManager.RegisterToZNetScene(MysticalWardPrefab);

//			// Configure the Piece data and add to buiild menu
//			ConfigureMysticalWardPieceData(MysticalWardPrefab, "FineWood", "Silver", "Obsidian");

//			// Toggle visibility
//			ToggleMysticalWardVisibility();

//			MysticalWardPrefab.SetActive(true);
//			log.Info("Mystical Light Ward registered and ready.");

//			// Test - show all components
//			//DisplayWardComponents();
//		}

//		private static GameObject CloneWardPrefab(string sourcePrefabName, string newPrefabName)
//		{
//			GameObject prefab = MPrefabManager.ClonePrefab(sourcePrefabName, newPrefabName);
//			if (prefab == null)
//			{
//				log.Error($"Cloning of {sourcePrefabName} failed.");
//			}

//			return prefab;
//		}

//		private static void SetupMysticalWardDefaults(GameObject prefab, string name, string craftingStation = null)
//		{
//			ZNetView znet = prefab.GetComponent<ZNetView>();
//			if (znet != null)
//			{
//				znet.m_persistent = true;
//				znet.m_distant = false;
//				znet.m_type = ZDO.ObjectType.Default;
//				znet.m_syncInitialScale = true;
//			}

//			Piece piece = prefab.GetComponent<Piece>();
//			if (piece != null)
//			{
//				piece.m_enabled = true;
//				piece.m_name = name;
//				piece.m_description = "";

//				GameObject craftingStationPrefab = MPrefabManager.GetPrefab(craftingStation);
//				if (craftingStationPrefab != null)
//				{
//					CraftingStation newCraftingStation = craftingStationPrefab.GetComponent<CraftingStation>();
//					if (newCraftingStation != null)
//					{
//						piece.m_craftingStation = newCraftingStation;
//						log.Info($"Crafting station set to {newCraftingStation.name} for piece {piece.name}");
//					}
//				}
//			}
//		}

//		private static void ScaleMysticalWard(GameObject prefab)
//		{
//			// Set the local scale of the prefab's root
//			Vector3 scale = Vector3.one * 0.6f;
//			prefab.transform.localScale = scale;
//		}

//		private static Sprite ModifyMysticalWardIcon(GameObject prefab)
//		{
//			log.Info("Modifying icon...");

//			Sprite newIcon = null;

//			return newIcon;
//		}

//		private static void ConfigureMysticalWardPieceData(GameObject prefab, string resourceWood, string resourceMetal, string resourceOther)
//		{
//			var pieceConfig = new PieceConfig
//			{
//				PieceTable = "Hammer",
//				Category = "Misc",
//				Requirements = new[]
//				{
//					new RequirementConfig(resourceWood, 2, recover: true),
//					new RequirementConfig(resourceMetal, 1, recover: true),
//					new RequirementConfig(resourceOther, 2, recover: true) 
//				}
//			};

//			MPrefabManager.AddToBuildMenu(prefab, pieceConfig);
//		}

//		public static void ToggleMysticalWardVisibility()
//		{
//			Piece mysticalWardPiece = MysticalWardPrefab?.GetComponent<Piece>();
//			if (mysticalWardPiece == null)
//			{
//				log.Warn($"Piece component does not exist. No toggle made.");
//				return;
//			}

//			if (ConfigManager.MysticalLightWardEnabled.Value)
//			{
//				mysticalWardPiece.m_enabled = true;
//			}
//			else
//			{
//				mysticalWardPiece.m_enabled = false;
//			}
//		}

//		// Modify tutorial
//		private static void DisplayWardComponents()
//		{
//			if (MysticalWardPrefab == null)
//			{
//				log.Warn("MysticalWardPrefab is null, cannot display components.");
//				return;
//			}

//			log.Info("Displaying MysticalWardPrefab components.");
//			Component[] prefabComponents = MysticalWardPrefab.GetComponentsInChildren<Component>(true);
//			foreach (Component comp in prefabComponents)
//			{
//				log.Info($"{comp.gameObject.name} - {comp.GetType().Name}");
//			}
//		}

//		[HarmonyPatch(typeof(Player), nameof(Player.ShowTutorial))]
//		public static class Tutorial_TracePatch
//		{
//			static void Prefix(string name)
//			{
//				// Always log the tutorial name
//				log.Info($"[Tutorial Trace] ShowTutorial called for '{name}'");

//				// Log the stack trace so we know the exact caller
//				var trace = new StackTrace(1, true); // skip this frame
//				log.Info($"[Tutorial Trace] Stack Trace:\n{trace}");
//			}
//		}

//		[HarmonyPatch(typeof(Tutorial), nameof(Tutorial.ShowText))]
//		public static class Trace_Tutorial_ShowText
//		{
//			static void Prefix(string name)
//			{
//				if (!string.Equals(name, "guard_stone", StringComparison.OrdinalIgnoreCase)) return;

//				log.Info($"[Tutorial Trace] Tutorial.ShowText('{name}')");
//				var st = new StackTrace(1, true);
//				log.Info($"[Tutorial Trace] Stack Trace:\n{st}");
//			}
//		}

//		[HarmonyPatch(typeof(Localization), nameof(Localization.Localize), new[] { typeof(string) })]
//		public static class Trace_Localization_TutorialKeys
//		{
//			static void Prefix(string text)
//			{
//				if (text == null) return;
//				if (text.IndexOf("tutorial_guardstone", StringComparison.OrdinalIgnoreCase) >= 0)
//				{
//					log.Info($"[Loc Trace] Localize called with: {text}");
//					var st = new StackTrace(1, true);
//					log.Info($"[Loc Trace] Stack:\n{st}");
//				}
//			}
//		}

//		[HarmonyPatch(typeof(Tutorial), "Awake")]
//		public static class Dump_Tutorials_OnAwake
//		{
//			static void Postfix(Tutorial __instance)
//			{
//				if (__instance == null || __instance.m_texts == null) return;

//				foreach (var t in __instance.m_texts)
//				{
//					log.Info($"[Tutorial Dump] name={t.m_name}, topicKey=$tutorial_{t.m_name}_topic");
//				}
//			}
//		}
//	}
//}
