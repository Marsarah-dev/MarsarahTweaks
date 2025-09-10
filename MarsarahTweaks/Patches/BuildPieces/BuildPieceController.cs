using HarmonyLib;
using Jotunn.Configs;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using UnityEngine;

namespace MarsarahTweaks.Patches.BuildPieces
{
	internal class BuildPieceController
	{
		private static readonly LogManager log = new LogManager("Build Piece Controller", LogManager.LogLevel.Info);

		[HarmonyPatch(typeof(Player), "Awake")]
		internal static class ObjectDB_Awake_Patch
		{
			private static bool initialized;

			static void Postfix()
			{
				if (initialized) return;
				bool toggled = true;
				
				// Run initial visibility sync
				toggled &= SilverSconce.ToggleVisibility();
				toggled &= ColoredDvergerLanterns.ToggleVisibility();
				toggled &= GreenStandingBrazier.ToggleVisibility();
				toggled &= SilverHangingBrazier.ToggleVisibility();

				initialized = toggled;

				if (initialized)
				{
					log.Info("Initial build piece visibility toggled.");
				}
				else
				{
					log.Info("Initial build piece visibility not ready.");
				}
			}
		}

		public static void ConfigurePiece(GameObject prefab, string category, RequirementConfig[] requirements)
		{
			if (prefab == null)
			{
				log.Warn("Cannot configure null prefab.");
				return;
			}

			var pieceConfig = new PieceConfig
			{
				PieceTable = "Hammer",
				Category = category,
				Requirements = requirements
			};

			MPrefabManager.AddToBuildMenu(prefab, pieceConfig);
			log.Info($"Registered prefab '{prefab.name}' in build menu under '{category}'.");
		}

		public static RequirementConfig MakeRequirement(string item, int amount, bool givenRecover = true)
		{
			return new RequirementConfig(item, amount, recover: givenRecover);
		}

		public static void RefreshPieceRequirements(Piece piece, Dictionary<string, int> amounts)
		{
			if (piece == null)
			{
				log.Warn("Cannot configure a null piece.");
				return;
			}

			if (piece.m_resources == null)
			{
				log.Warn($"Resources for piece '{piece.name}' is null.");
				return;
			}

			for (int i = 0; i < piece.m_resources.Length; i++)
			{
				var res = piece.m_resources[i];
				if (res?.m_resItem == null) continue;

				string name = res.m_resItem.name;
				if (amounts.TryGetValue(name, out int value))
				{
					res.m_amount = value;
					log.Info($"Updated resource {name}: {value} for piece '{piece.name}'");
				}
			}
		}

		/*public static void RefreshPieceRequirements(Piece piece, int amount1, int amount2, int amount3)
		{
			if (piece == null)
			{
				log.Warn("Cannot configure a null piece.");
				return;
			}

			if (piece.m_resources == null)
			{
				log.Warn($"Resources for piece '{piece.name}' is null.");
				return;
			}

			if (piece.m_resources.Length > 0) piece.m_resources[0].m_amount = amount1;
			if (piece.m_resources.Length > 1) piece.m_resources[1].m_amount = amount2;
			if (piece.m_resources.Length > 2) piece.m_resources[2].m_amount = amount3;

			log.Info($"Refreshed requirements for '{piece.name}'");
		}*/

		/*public static void RefreshPieceRequirements(Piece piece, params Func<int>[] amountFuncs)
		{
			if (piece == null)
			{
				log.Warn("Cannot configure null piece.");
				return;
			}

			for (int i = 0; i < piece.m_resources.Length && i < amountFuncs.Length; i++)
			{
				piece.m_resources[i].m_amount = amountFuncs[i]();
			}

			log.Info($"Refreshed requirements for '{piece.name}': " + string.Join(", ", piece.m_resources.Select(r => $"{r.m_resItem.name}={r.m_amount}")));
		}*/

		public static bool TogglePiece(Piece piece, bool enabled, LogManager specificLog)
		{
			if (piece == null)
			{
				//specificLog.Warn("Piece is null.");
				return false;
			}
			if (ObjectDB.instance == null)
			{
				specificLog.Warn("ObjDB is not ready yet.");
				return false;
			}

			PieceTable hammer = ObjectDB.instance.GetItemPrefab("Hammer").GetComponent<ItemDrop>().m_itemData.m_shared.m_buildPieces;
			if (hammer == null) return false;

			piece.m_enabled = enabled;

			if (enabled)
			{
				if (!hammer.m_pieces.Contains(piece.gameObject))
				{
					hammer.m_pieces.Add(piece.gameObject);
				}
			}
			else
			{
				if (hammer.m_pieces.Contains(piece.gameObject))
				{
					hammer.m_pieces.Remove(piece.gameObject);
				}
			}

			return true;
		}
	}
}
