using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Heightmap;

namespace MarsarahTweaks.Patches.UI
{
	internal class UISummonCounter : UIController
	{
		// UI data
		public static int numSummons;

		// UI elements
		private static Text UISummonsText;
		private static TMPro.TextMeshProUGUI UISummonsTextTMP = null;
		private static Image summonsAreaBackground;
		private static GameObject UISummonsArea = null;

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
					CreateUI(__instance); // Create UI if missing

					// Handle summon counter position according to the use of symbols or not
					if (lastUseSymbolInsteadOfWords != ConfigManager.UseSymbolsForUI.Value)
					{
						lastUseSymbolInsteadOfWords = ConfigManager.UseSymbolsForUI.Value;
						UpdateSummonCounterPosition();
					}

					UISummonsText.enabled = showUI && (numSummons != 0);
					UISummonsTextTMP.enabled = showUI && (numSummons != 0);
					summonsAreaBackground.enabled = showUI && (numSummons != 0);

					if (showUI)
					{
						if (numSummons != 0)
						{
							UISummonsText.color = GetColorFromNum(numSummons);
							UISummonsTextTMP.color = GetColorFromNum(numSummons);
							if (!ConfigManager.UseSymbolsForUI.Value)
							{
								UISummonsText.text = "Summons " + numSummons.ToString();
								UISummonsTextTMP.text = "";
							}
							else
							{
								UISummonsText.text = "";
								UISummonsTextTMP.text = "💀 " + numSummons.ToString();
							}
						}
						else
						{
							UISummonsText.text = "";
							UISummonsTextTMP.text = "";
						}
					}
				}
				else
				{
					if (UISummonsText != null)
						UISummonsText.enabled = false;
					if (UISummonsTextTMP != null)
						UISummonsTextTMP.enabled = false;
					if (summonsAreaBackground != null)
						summonsAreaBackground.enabled = false;
				}
			}

			private static void CreateUI(Hud hud)
			{
				if (UISummonsText != null && UISummonsTextTMP != null && summonsAreaBackground != null)
					return;  // UI already exists, no need to create again

				int UITextFontSize = 16;
				string UITextFontName = "AveriaSansLibre-Bold";
				Vector2 UISumonsAreaSize;
				Vector2 UISumonsAreaSizeDefault = new Vector2(100f, 30f); // width, height

				if (!ConfigManager.UseSymbolsForUI.Value)
					UISumonsAreaSize = UISumonsAreaSizeDefault; 
				else
					UISumonsAreaSize = new Vector2(49f, 30f);

				// Summons area object
				UISummonsArea = new GameObject("SummonsArea");
				UISummonsArea.layer = 5;
				UISummonsArea.transform.SetParent(hud.m_healthPanel.transform);
				RectTransform summonsAreaTransform = UISummonsArea.AddComponent<RectTransform>();
				summonsAreaTransform.anchorMin = new Vector2(1f, 1f);
				summonsAreaTransform.anchorMax = new Vector2(1f, 1f);
				if (!ConfigManager.UseSymbolsForUI.Value)
					summonsAreaTransform.anchoredPosition = new Vector2(63f, -85f); // above the boss buff, to the right of hp bar
				else
					summonsAreaTransform.anchoredPosition = new Vector2(38f, -85f); // above the boss buff, to the right of hp bar
				summonsAreaTransform.sizeDelta = UISumonsAreaSize;
				UISummonsArea.transform.localScale = Vector3.one;  // Ensure correct scale

				// Background texture
				Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((Sprite tempSprite) => tempSprite.name == "InputFieldBackground");
				summonsAreaBackground = UISummonsArea.AddComponent<Image>();
				summonsAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
				summonsAreaBackground.sprite = sprite;
				summonsAreaBackground.type = Image.Type.Sliced;
				summonsAreaBackground.enabled = showUI;

				UISummonsText = CreateTextObject("SummonsText", UISummonsArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleCenter, new Vector2(0f, 0f), UISumonsAreaSizeDefault);
				UISummonsTextTMP = CreateTMPTextObject("SummonsText", UISummonsArea, Color.white, UITextFontName, UITextFontSize, TextAlignmentOptions.Midline, new Vector2(0f, 0f), UISumonsAreaSize);
			}

			private static Color GetColorFromNum(int num)
			{
				if (num < 2) return new Color(1f, 0.549019f, 0f); // orange
				if (num == 2) return Color.yellow;
				if (num >= 3) return Color.green;
				return Color.white;
			}

			private static void UpdateSummonCounterPosition()
			{
				if (UISummonsArea == null) return;

				Vector2 UISumonsAreaSize;

				if (!ConfigManager.UseSymbolsForUI.Value)
					UISumonsAreaSize = new Vector2(100f, 30f); // width, height
				else
					UISumonsAreaSize = new Vector2(49f, 30f); // width, height

				RectTransform summonsAreaTransform = UISummonsArea.GetComponent<RectTransform>();
				summonsAreaTransform.sizeDelta = UISumonsAreaSize;

				if (!ConfigManager.UseSymbolsForUI.Value)
					summonsAreaTransform.anchoredPosition = new Vector2(63f, -85f);				
				else
					summonsAreaTransform.anchoredPosition = new Vector2(38f, -85f);
			}
		}
	}
}
