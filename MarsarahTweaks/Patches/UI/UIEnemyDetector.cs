using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.UI
{
	internal class UIEnemyDetector : UIController
	{
		private static readonly LogManager log = new LogManager("UI Enemy Detector", LogManager.LogLevel.Warning);

		// UI data
		public static int numEnemies;
		public static int numEnemiesPassive;

		// UI elements
		internal static GameObject UIEnemyArea = null;
		private static Text UIEnemyText = null;

		internal static GameObject UIEnemyArea2 = null;
		private static Text UIEnemyText2 = null;
		private static TMPro.TextMeshProUGUI UIEnemyEmojiTMP = null;

		internal static GameObject UIFriendlyArea = null;
		private static Text UIFriendlyText = null;
		private static TMPro.TextMeshProUGUI UIFriendlyEmojiTMP = null;

		[HarmonyPatch(typeof(Player), "Update")]
		class EnemyDetector_PlayerPatch
		{
			private static void Prefix(ref Player ___m_localPlayer)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (___m_localPlayer == null) return;

				if (ConfigManager.ShowEnemyDetector.Value)
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
		class EnemyDetector_HUDUpdatePatch
		{
			private static bool lastUseSymbolInsteadOfWords = ConfigManager.UseSymbolsForUI.Value;

			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.ShowEnemyDetector.Value)
				{
					CreateTextBasedUI(__instance);
					CreateSymbolBasedUI(__instance);

					if (!ConfigManager.UseSymbolsForUI.Value)
					{
						UIEnemyArea?.SetActive(showUI);
						UIEnemyArea2?.SetActive(false);
						UIFriendlyArea?.SetActive(false);

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
						UIEnemyArea?.SetActive(false);
						UIEnemyArea2?.SetActive(showUI);
						UIFriendlyArea?.SetActive(showUI && numEnemiesPassive != 0);

						if (showUI)
						{
							Color enemyColor = GetColorFromNum(numEnemies);

							UIEnemyText2.color = enemyColor;
							UIEnemyText2.text = numEnemies.ToString();

							UIEnemyEmojiTMP.color = enemyColor;
							if (numEnemies == 0)
								UIEnemyEmojiTMP.text = "👁"; // 👁 - 👀
							else if (numEnemies < 5)
								UIEnemyEmojiTMP.text = "😈";
							else if (numEnemies < 7)
								UIEnemyEmojiTMP.text = "👿";
							else
								UIEnemyEmojiTMP.text = "☠"; // 👻

							Color friendlyColor = GetColorFromNum(numEnemiesPassive);

							UIFriendlyText.color = friendlyColor;
							UIFriendlyText.text = numEnemiesPassive.ToString();

							UIFriendlyEmojiTMP.color = friendlyColor;
							UIFriendlyEmojiTMP.text = "🧔‍";

							// 👹 👺 😈 👻 - 🧔‍♂️ 🧔‍♀️ - ⚔️ ☠
						}
					}
				}
				else
				{
					// We're calling create here to make sure objects are safe to access
					CreateTextBasedUI(__instance);
					CreateSymbolBasedUI(__instance);

					UIEnemyArea?.SetActive(false);
					UIEnemyArea2?.SetActive(false);
					UIFriendlyArea?.SetActive(false);
				}
			} 

			private static void CreateTextBasedUI(Hud hud)
			{
				if (UIEnemyArea != null && UIEnemyText != null)
					return;  // UI already exists, no need to create again

				int UITextFontSize = 16;
				string UITextFontName = "AveriaSansLibre-Bold";
				Vector2 UIEnemyAreaSize = new Vector2(100f, 30f); // width, height;

				// Enemy area object
				UIEnemyArea = new GameObject("EnemyArea");
				UIEnemyArea.layer = 5;
				UIEnemyArea.transform.SetParent(hud.m_healthPanel.transform);
				RectTransform enemyAreaTransform = UIEnemyArea.AddComponent<RectTransform>();
				enemyAreaTransform.anchorMin = new Vector2(1f, 1f);
				enemyAreaTransform.anchorMax = new Vector2(1f, 1f);
				enemyAreaTransform.anchoredPosition = new Vector2(ConfigManager.ShowInventoryWeightAndSlots.Value ? 70f : -40f, -230f);
				enemyAreaTransform.sizeDelta = UIEnemyAreaSize;
				UIEnemyArea.transform.localScale = Vector3.one;  // Ensure correct scale

				// Background texture
				Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((Sprite tempSprite) => tempSprite.name == "InputFieldBackground");
				Image enemyAreaBackground = UIEnemyArea.AddComponent<Image>();
				enemyAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
				enemyAreaBackground.sprite = sprite;
				enemyAreaBackground.type = Image.Type.Sliced;

				// Enemy area text object
				UIEnemyText = CreateTextObject("EnemyText", UIEnemyArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleCenter, new Vector2(0f, 0f), UIEnemyAreaSize);
			}

			private static void CreateSymbolBasedUI(Hud hud)
			{
				if (UIEnemyArea2 != null && UIEnemyText2 != null && UIEnemyEmojiTMP != null) return;

				int UITextFontSize = 16;
				string UITextFontName = "AveriaSansLibre-Bold";
				Vector2 UIEnemyAreaSize = new Vector2(50f, 30f); // width, height
				float xOffset = ConfigManager.ShowInventoryWeightAndSlots.Value ? 122f : -65f; // 92 / 65
				float yOffset = -230f;

				// ==== Enemy Counter ====

				// Enemy Area Object
				UIEnemyArea2 = new GameObject("EnemyArea2");
				UIEnemyArea2.layer = 5;
				UIEnemyArea2.transform.SetParent(hud.m_healthPanel.transform); // health

				RectTransform enemyAreaTransform = UIEnemyArea2.AddComponent<RectTransform>();
				enemyAreaTransform.anchorMin = new Vector2(1f, 1f);
				enemyAreaTransform.anchorMax = new Vector2(1f, 1f);
				enemyAreaTransform.anchoredPosition = new Vector2(xOffset, yOffset); // 92f
				enemyAreaTransform.sizeDelta = UIEnemyAreaSize;
				UIEnemyArea2.transform.localScale = Vector3.one; // Ensure correct scale

				// Background texture
				Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((Sprite tempSprite) => tempSprite.name == "InputFieldBackground");
				Image enemyAreaBackground = UIEnemyArea2.AddComponent<Image>();
				enemyAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
				enemyAreaBackground.sprite = sprite;
				enemyAreaBackground.type = Image.Type.Sliced;

				// Text overlay
				UIEnemyText2 = CreateTextObject("EnemyText2", UIEnemyArea2, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(-4f, 0f), UIEnemyAreaSize);

				// Enemy icon
				UIEnemyEmojiTMP = CreateTMPTextObject("EnemyEmojiTMP", UIEnemyArea2, Color.green, UITextFontName, UITextFontSize + 4, TextAlignmentOptions.MidlineLeft, new Vector2(4f, 0f), UIEnemyAreaSize);

				// ==== Friendly Counter ====

				// Friendly Area Object
				UIFriendlyArea = new GameObject("FriendlyArea");
				UIFriendlyArea.layer = 5;
				UIFriendlyArea.transform.SetParent(hud.m_healthPanel.transform); // health
				xOffset += 54f;

				RectTransform friendlyAreaTransform = UIFriendlyArea.AddComponent<RectTransform>();
				friendlyAreaTransform.anchorMin = new Vector2(1f, 1f);
				friendlyAreaTransform.anchorMax = new Vector2(1f, 1f);
				friendlyAreaTransform.anchoredPosition = new Vector2(xOffset, yOffset); // 146f
				friendlyAreaTransform.sizeDelta = UIEnemyAreaSize;
				UIFriendlyArea.transform.localScale = Vector3.one;

				// Background texture
				Sprite spriteFriendly = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((Sprite tempSprite) => tempSprite.name == "InputFieldBackground");
				Image friendlyAreaBackground = UIFriendlyArea.AddComponent<Image>();
				friendlyAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
				friendlyAreaBackground.sprite = sprite;
				friendlyAreaBackground.type = Image.Type.Sliced;

				// Text overlay
				UIFriendlyText = CreateTextObject("FriendlyText", UIFriendlyArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(-4f, 0f), UIEnemyAreaSize);

				// Friendly icon
				UIFriendlyEmojiTMP = CreateTMPTextObject("FriendlyEmojiTMP", UIFriendlyArea, Color.green, UITextFontName, UITextFontSize + 4, TextAlignmentOptions.MidlineLeft, new Vector2(4f, 0f), UIEnemyAreaSize);
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
