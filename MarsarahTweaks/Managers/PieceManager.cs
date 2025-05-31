using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Managers
{
	public class PieceManager
	{


		// ===================================================================

		/*public void RegisterPiece(GameObject piecePrefab, string hammerPrefabName, string category, string craftingStation)
		{
			if (ObjectDB.instance == null || !ObjectDB.instance.m_items.Any())
			{
				MarsarahTweaks.LogWarn("ObjectDB not ready. Try delaying piece registration.");
				return;
			}

			var hammer = ObjectDB.instance.m_items
				.Select(i => i.GetComponent<ItemDrop>())
				.FirstOrDefault(i => i != null && i.name == hammerPrefabName);

			if (hammer == null)
			{
				Debug.LogWarning("Hammer not found: " + hammerPrefabName);
				return;
			}

			var table = hammer.m_itemData.m_shared.m_buildPieces;
			if (table != null && !table.m_pieces.Contains(piecePrefab))
			{
				table.m_pieces.Add(piecePrefab);
			}

			var piece = piecePrefab.GetComponent<Piece>();
			if (piece != null)
			{
				piece.m_category = (Piece.PieceCategory)Enum.Parse(typeof(Piece.PieceCategory), category);
				piece.m_craftingStation = ZNetScene.instance.GetPrefab(craftingStation)?.GetComponent<CraftingStation>();
			}
		}*/
	}
}
