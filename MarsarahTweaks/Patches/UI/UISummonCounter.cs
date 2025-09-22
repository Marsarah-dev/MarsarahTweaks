using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Heightmap;

namespace MarsarahTweaks.Patches.UI
{
	internal class UISummonCounter : UIController
	{
		private static readonly LogManager log = new LogManager("UI Summon Counter", LogManager.LogLevel.Warning);

		// UI data
		public static int numSummons;

		// UI elements
		private static GameObject UISummonsArea = null;
		private static Text UISummonsText = null;

		private static GameObject UISummonsArea2 = null;
		private static Text UISummonsText2 = null;
		private static TMPro.TextMeshProUGUI UISummonsTextTMP = null;

		[HarmonyPatch(typeof(Player), "Update")]
		class SummonCounters_PlayerPatch
		{
			private static void Prefix(ref Player ___m_localPlayer)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (___m_localPlayer == null) return;

				if (ConfigManager.ShowSummonCounter.Value && showUI)
				{
					List<Character> allCharacters = Character.GetAllCharacters();
					int numSummonedSkeletons = 0;

					foreach (Character character in allCharacters)
					{
						if (character.IsTamed())
						{
							MonsterAI thisMonsterAI = character.GetComponent<MonsterAI>();
							if (thisMonsterAI != null)
							{
								GameObject followTarget = thisMonsterAI.GetFollowTarget();
								if (followTarget != null)
								{
									Player targetPlayer = followTarget.GetComponent<Player>();
									if (targetPlayer != null)
									{
										if (targetPlayer.GetPlayerName() == ___m_localPlayer.GetPlayerName())
										{
											if (character.name.Contains("Skeleton_Friendly"))
											{
												numSummonedSkeletons++;
											}
										}
									}
								}
							}
						}
					}

					numSummons = numSummonedSkeletons;
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		class InventoryWeightAndSlots_HUDAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.ShowSummonCounter.Value)
				{
					CreateTextBasedUI(__instance);
					CreateSymbolBasedUI(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		class SummonCounter_HUDUpdatePatch
		{
			private static bool lastUseSymbolInsteadOfWords = ConfigManager.UseSymbolsForUI.Value;
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.ShowSummonCounter.Value)
				{
					CreateTextBasedUI(__instance);
					CreateSymbolBasedUI(__instance);

					bool showSummonsCounter = showUI && (numSummons != 0);

					if (!ConfigManager.UseSymbolsForUI.Value)
					{
						UISummonsArea?.SetActive(showSummonsCounter);
						UISummonsArea2?.SetActive(false);

						if (showSummonsCounter)
						{
							UISummonsText.color = GetColorFromNum(numSummons);
							UISummonsText.text = "Summons " + numSummons.ToString();
						}
					}
					else
					{
						UISummonsArea?.SetActive(false);
						UISummonsArea2?.SetActive(showSummonsCounter);

						if (showSummonsCounter)
						{
							UISummonsText2.color = GetColorFromNum(numSummons);
							UISummonsTextTMP.color = GetColorFromNum(numSummons);

							UISummonsText2.text = numSummons.ToString();
							UISummonsTextTMP.text = "💀";
						}
					}
				}
				else
				{
					// We're calling create here to make sure objects are safe to access
					CreateTextBasedUI(__instance);
					CreateSymbolBasedUI(__instance);

					UISummonsArea?.SetActive(false);
					UISummonsArea2?.SetActive(false);
				}
			}

			private static Color GetColorFromNum(int num)
			{
				if (num < 2) return new Color(1f, 0.549019f, 0f); // orange
				if (num == 2) return Color.yellow;
				if (num >= 3) return Color.green;
				return Color.white;
			}
		}

		private static void CreateTextBasedUI(Hud hud)
		{
			if (UISummonsArea != null && UISummonsText != null)
				return;  // UI already exists

			int UITextFontSize = 16;
			string UITextFontName = "AveriaSansLibre-Bold";
			Vector2 UISumonsAreaSize = new Vector2(100f, 30f); // width, height

			// Summons area object
			UISummonsArea = new GameObject("SummonsArea");
			UISummonsArea.layer = 5;
			UISummonsArea.transform.SetParent(hud.m_healthPanel.transform);

			RectTransform summonsAreaTransform = UISummonsArea.AddComponent<RectTransform>();
			summonsAreaTransform.anchorMin = new Vector2(1f, 1f);
			summonsAreaTransform.anchorMax = new Vector2(1f, 1f);
			summonsAreaTransform.anchoredPosition = new Vector2(63f, -85f); // above the boss buff, to the right of hp bar
			summonsAreaTransform.sizeDelta = UISumonsAreaSize;
			UISummonsArea.transform.localScale = Vector3.one;  // Ensure correct scale

			// Background texture
			Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((Sprite tempSprite) => tempSprite.name == "InputFieldBackground");
			Image SummonsAreaBackground = UISummonsArea.AddComponent<Image>();
			SummonsAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
			SummonsAreaBackground.sprite = sprite;
			SummonsAreaBackground.type = Image.Type.Sliced;

			UISummonsText = CreateTextObject("SummonsText", UISummonsArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleCenter, new Vector2(0f, 0f), UISumonsAreaSize);
		}

		private static void CreateSymbolBasedUI(Hud hud)
		{
			if (UISummonsArea2 != null && UISummonsText2 != null && UISummonsTextTMP != null)
				return;  // UI already exists

			int UITextFontSize = 16;
			string UITextFontName = "AveriaSansLibre-Bold";
			string UIEmojiFontName = "NotoEmoji-Regular SDF"; // NotoEmoji-Regular
			Vector2 UISumonsAreaSize = new Vector2(49f, 30f);

			// Summons area object
			UISummonsArea2 = new GameObject("SummonsArea2");
			UISummonsArea2.layer = 5;
			UISummonsArea2.transform.SetParent(hud.m_healthPanel.transform);

			RectTransform summonsAreaTransform = UISummonsArea2.AddComponent<RectTransform>();
			summonsAreaTransform.anchorMin = new Vector2(1f, 1f);
			summonsAreaTransform.anchorMax = new Vector2(1f, 1f);
			summonsAreaTransform.anchoredPosition = new Vector2(38f, -85f); // above the boss buff, to the right of hp bar
			summonsAreaTransform.sizeDelta = UISumonsAreaSize;
			UISummonsArea.transform.localScale = Vector3.one;  // Ensure correct scale

			// Background texture
			Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((Sprite tempSprite) => tempSprite.name == "InputFieldBackground");
			Image SummonsAreaBackground2 = UISummonsArea2.AddComponent<Image>();
			SummonsAreaBackground2.color = new Color(0f, 0f, 0f, 0.4f);
			SummonsAreaBackground2.sprite = sprite;
			SummonsAreaBackground2.type = Image.Type.Sliced;

			UISummonsText2 = CreateTextObject("SummonsText2", UISummonsArea2, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(-5f, 0f), UISumonsAreaSize);
			UISummonsTextTMP = CreateTMPTextObject("SummonsEmojiTMP", UISummonsArea2, Color.white, UIEmojiFontName, UITextFontSize, TextAlignmentOptions.MidlineLeft, new Vector2(5f, 0f), UISumonsAreaSize, log);
		}
	}
}
