using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using static Heightmap;

namespace MarsarahTweaks.Patches.UI
{
	internal class UISummonCounter : UIController
	{
		// UI data
		public static int numSummons;

		// UI elements
		private static Text UISummonsText;
		private static Image summonsAreaBackground;

		[HarmonyPatch(typeof(Player), "Update")]
		class SummonCounters_PlayerPatch
		{
			static void Prefix(ref Player ___m_localPlayer)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (___m_localPlayer == null) return;

				if (ConfigManager.showSummonCounter.Value && showUI)
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
		public static class SummonCounter_HUDUpdatePatch
		{
			public static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.showSummonCounter.Value)
				{
					CreateUI(__instance); // Create UI if missing

					UISummonsText.enabled = showUI && (numSummons != 0);
					summonsAreaBackground.enabled = showUI && (numSummons != 0);

					if (showUI)
					{
						if (numSummons != 0)
						{
							UISummonsText.color = GetColorFromNum(numSummons);
							UISummonsText.text = "Summons " + numSummons.ToString();
						}
						else
						{
							UISummonsText.text = "";
						}
					}
				}
				else
				{
					if (UISummonsText != null)
						UISummonsText.enabled = false;
					if (summonsAreaBackground != null)
						summonsAreaBackground.enabled = false;
				}
			}

			private static void CreateUI(Hud hud)
			{
				if (UISummonsText != null && summonsAreaBackground != null)
					return;  // UI already exists, no need to create again

				int UITextFontSize = 16;
				string UITextFontName = "AveriaSansLibre-Bold";
				Vector2 UISumonsAreaSize = new Vector2(100f, 30f); // width, height

				// Summons area object
				GameObject UISummonsArea = new GameObject("SummonsArea");
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
				summonsAreaBackground = UISummonsArea.AddComponent<Image>();
				summonsAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
				summonsAreaBackground.sprite = sprite;
				summonsAreaBackground.type = Image.Type.Sliced;
				summonsAreaBackground.enabled = showUI;

				UISummonsText = CreateTextObject("SummonsText", UISummonsArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleCenter, new Vector2(0f, 0f), UISumonsAreaSize);
			}

			private static Color GetColorFromNum(int num)
			{
				if (num < 2) return new Color(1f, 0.549019f, 0f); // orange
				if (num == 2) return Color.yellow;
				if (num >= 3) return Color.green;
				return Color.white;
			}
		}
	}
}
