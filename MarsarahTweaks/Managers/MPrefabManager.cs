using BepInEx;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using MarsarahTweaks.Patches.BuildPieces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Managers
{
	// MPrefabManager: MarsarahTweaks prefab cloning and registration system
	internal class MPrefabManager
	{
		private static readonly LogManager log = new LogManager("M Prefab Manager", LogManager.LogLevel.Info);

		public static GameObject GetPrefab(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				log.Error("GetPrefab: Given prefab name is null or empty.");
				return null;
			}

			return PrefabManager.Instance.GetPrefab(name);
		}

		public static GameObject ClonePrefab(string nameOfOriginal, string nameOfClone)
		{
			if (nameOfOriginal.IsNullOrWhiteSpace() || nameOfClone.IsNullOrWhiteSpace())
			{
				log.Warn("Given strings for cloning are null or empty. Cannot clone prefab.");
				return null;
			}

			if (GetPrefab(nameOfClone) != null)
			{
				log.Warn($"A prefab named {nameOfClone} already exists in ZNetScene. Skipping clone.");
				return null;
			}

			GameObject originalPrefab = GetPrefab(nameOfOriginal);
			if (originalPrefab == null)
			{
				log.Error($"Original prefab {nameOfOriginal} not found.");
				return null;
			}

			return ClonePrefab(originalPrefab, nameOfClone);
		}

		public static GameObject ClonePrefab(GameObject originalPrefab, string nameOfClone)
		{
			if (originalPrefab == null || nameOfClone.IsNullOrWhiteSpace())
			{
				log.Warn("Null original or empty clone name.");
				return null;
			}

			if (GetPrefab(nameOfClone) != null)
			{
				log.Warn($"A prefab named {nameOfClone} already exists in ZNetScene. Skipping clone.");
				return null;
			}

			return PrefabManager.Instance.CreateClonedPrefab(nameOfClone, originalPrefab);
		}

		public static void RegisterToZNetScene(GameObject prefab)
		{
			if (prefab == null)
			{
				log.Error("Tried to register null prefab.");
				return;
			}

			ZNetScene znetScene = ZNetScene.instance;
			if (znetScene == null)
			{
				log.Error("ZNetScene.instance is null. Cannot register prefab.");
				return;
			}

			if (znetScene.GetPrefab(prefab.name) != null)
			{
				log.Warn($"Prefab '{prefab.name}' already registered in ZNetScene.");
				return;
			}

			CustomPrefab customPrefab = new CustomPrefab(prefab, fixReference: true);
			PrefabManager.Instance.AddPrefab(customPrefab);
			PrefabManager.Instance.RegisterToZNetScene(prefab);

			if (GetPrefab(prefab.name) == null)
			{
				log.Error($"Failed to register prefab '{prefab.name}'!");
			}
			else
			{
				log.Info($"Registered prefab '{prefab.name}' to ZNetScene.");
			}
		}

		public static void RegisterItem(GameObject prefab)
		{
			if (prefab == null)
			{
				log.Error("Tried to register null prefab.");
				return;
			}

			CustomItem customItem = new CustomItem(prefab, fixReference: true);
			ItemManager.Instance.AddItem(customItem);
		}

		public static Recipe RegisterRecipe(RecipeConfig recipe)
		{
			if (recipe == null)
			{
				log.Error("Tried to register null recipe.");
				return null;
			}

			CustomRecipe customRecipe = new CustomRecipe(recipe);
			ItemManager.Instance.AddRecipe(customRecipe);

			return customRecipe.Recipe;
		}

		public static void AddToBuildMenu(GameObject prefab, PieceConfig pieceConfig)
		{
			if (prefab == null)
			{
				log.Warn($"Given prefab is null. Cannot add to build menu.");
				return;
			}

			if (pieceConfig == null)
			{
				log.Warn($"Given piece config is null. Cannot add to build menu.");
				return;
			}

			CustomPiece customPiece = new CustomPiece(prefab, fixReference: true, pieceConfig);
			PieceManager.Instance.AddPiece(customPiece);
		}

		/*public static void AddToHammerBuildMenu(GameObject prefab)
		{
			if (prefab == null)
			{
				log.Error("Tried to add null prefab to hammer.");
				return;
			}

			GameObject hammerPrefab = ObjectDB.instance?.GetItemPrefab("Hammer");
			ItemDrop hammer = hammerPrefab?.GetComponent<ItemDrop>();
			PieceTable table = hammer?.m_itemData?.m_shared?.m_buildPieces;

			if (table == null)
			{
				log.Error("Could not get Hammer piece table.");
				return;
			}

			if (!table.m_pieces.Contains(prefab))
			{
				table.m_pieces.Add(prefab);
				log.Info($"Added '{prefab.name}' to hammer build menu.");
			}
		}*/
	}
}
