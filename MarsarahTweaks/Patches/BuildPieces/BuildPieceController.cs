using HarmonyLib;
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
