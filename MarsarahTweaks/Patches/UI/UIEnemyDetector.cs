using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;

namespace MarsarahTweaks.Patches.UI
{
	internal class UIEnemyDetector : UIController
	{
		// UI data
		public static int numEnemies;
		public static int numEnemiesPassive;

		// UI elements
		internal static GameObject UIEnemyArea = null;
		private static Text UIEnemyText = null;
		private static Image enemyAreaBackground = null;

		[HarmonyPatch(typeof(Player), "Update")]
		class EnemyDetector_PlayerPatch
		{
			static void Prefix(ref Player ___m_localPlayer)
			{
				if (___m_localPlayer == null) return;

				if (ConfigManager.showEnemyDetector.Value)
				{
					int numEnemiesIgnore = 0;
					int numPassiveDverger = 0;
					List<Character> characters = new List<Character>();
					Character.GetCharactersInRange(___m_localPlayer.transform.position, 30f, characters);
					foreach (Character character in characters)
					{
						if (character.GetFaction() == Character.Faction.Dverger && character.GetBaseAI() != null && !character.GetBaseAI().IsAggravated())
							numPassiveDverger++;
						if (character.m_name.ToString() == "Human" || character.m_name.ToString() == "$enemy_deer" || character.m_name.ToString() == "$enemy_hare" || character.m_name.ToString() == "$enemy_summonedroot" || character.IsTamed())
							numEnemiesIgnore++;
					}
					numEnemies = characters.Count - numEnemiesIgnore - numPassiveDverger;
					numEnemiesPassive = numPassiveDverger;
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		public static class EnemyDetector_HUDUpdatePatch
		{
			public static void Postfix(Hud __instance)
			{
				if (__instance == null) return;

				if (ConfigManager.showEnemyDetector.Value)
				{
					CreateUI(__instance); // Create UI if missing

					UIEnemyText.enabled = showUI;
					enemyAreaBackground.enabled = showUI;

					if (showUI)
					{
						UIEnemyText.color = GetColorFromNum(numEnemies);

						if (numEnemiesPassive > 0)
							UIEnemyText.text = "Enemies " + numEnemies.ToString() + " (" + numEnemiesPassive + ")";
						else
							UIEnemyText.text = "Enemies " + numEnemies.ToString();

					}
				}
				else
				{
					if (UIEnemyText != null)
						UIEnemyText.enabled = false;
					if (enemyAreaBackground != null)
						enemyAreaBackground.enabled = false;
				}
			}

			private static void CreateUI(Hud hud)
			{
				if (UIEnemyText != null && enemyAreaBackground != null)
					return;  // UI already exists, no need to create again

				int UITextFontSize = 16;
				string UITextFontName = "AveriaSansLibre-Bold";
				Vector2 UIEnemyAreaSize = new Vector2(100f, 30f); // width, height

				// Enemy area object
				UIEnemyArea = new GameObject("EnemyArea");
				UIEnemyArea.layer = 5;
				UIEnemyArea.transform.SetParent(hud.m_healthPanel.transform);
				RectTransform enemyAreaTransform = UIEnemyArea.AddComponent<RectTransform>();
				enemyAreaTransform.anchorMin = new Vector2(1f, 1f);
				enemyAreaTransform.anchorMax = new Vector2(1f, 1f);
				enemyAreaTransform.anchoredPosition = new Vector2(ConfigManager.showInventoryWeightAndSlots.Value ? 70f : -40f, -230f);
				enemyAreaTransform.sizeDelta = UIEnemyAreaSize;
				UIEnemyArea.transform.localScale = Vector3.one;  // Ensure correct scale

				// Background texture
				Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((Sprite tempSprite) => tempSprite.name == "InputFieldBackground");
				enemyAreaBackground = UIEnemyArea.AddComponent<Image>();
				enemyAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
				enemyAreaBackground.sprite = sprite;
				enemyAreaBackground.type = Image.Type.Sliced;
				enemyAreaBackground.enabled = showUI;

				// Enemy area text object
				UIEnemyText = CreateTextObject("EnemyText", UIEnemyArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleCenter, new Vector2(0f, 0f), UIEnemyAreaSize);
			}

			private static Color GetColorFromNum(int num)
			{
				if (num < 3) return Color.green;
				if (num < 5) return Color.yellow;
				if (num < 7) return new Color(1f, 0.549019f, 0f);
				return Color.red;
			}
		}
	}
}
