using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static ZNet;

namespace MarsarahTweaks.Patches.UI
{
	internal class UIOnlinePlayers : UIController
	{
		// UI data
		private static List<PlayerInfo> playerInfoList = new List<PlayerInfo>();
		private static int numOnlinePlayerSlots = 21; // 20 (for players) + 1 (for the header)

		// UI elements
		private static GameObject UIPartyArea;
		private static List<Text> UIPlayerTexts = new List<Text>();

		[HarmonyPatch(typeof(ZNet), "Update")]
		class OnlinePartyIndicator_Patch
		{
			static void Prefix(ref List<PlayerInfo> ___m_players)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (ConfigManager.showOnlinePlayers.Value && showUI)
				{
					if (___m_players.Count != 0)
						playerInfoList = ___m_players;
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		static class OnlinePlayers_HUDUpdatePatch
		{
			private static bool lastOnlinePlayersUnderMinimap = ConfigManager.onlinePlayersUnderMinimap.Value;
			private static float UIPartyPlayerTextDistanceV = -25f; // goes down;

			static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.showOnlinePlayers.Value)
				{
					CreateUI(__instance); // Create UI if missing

					// Handle where to display the online list when toggling
					if (ConfigManager.onlinePlayersUnderMinimap.Value != lastOnlinePlayersUnderMinimap)
					{
						lastOnlinePlayersUnderMinimap = ConfigManager.onlinePlayersUnderMinimap.Value;
						UpdatePartyUIPosition(__instance);
					}

					// Special case that prevents the online players area to show during the loadscreen, since we attached to the parent of the minimap
					if (Player.m_localPlayer && UIPartyArea != null && !UIPartyArea.activeSelf)
					{
						//MarsarahTweaks.MLog("Enabling Party UI after load screen");
						UIPartyArea.SetActive(true);
					}

					int numPlayers = (playerInfoList.Count <= numOnlinePlayerSlots - 1) ? playerInfoList.Count : numOnlinePlayerSlots - 1;
					//bool onePlayer = playerInfoList.Count == 1;
					bool onePlayer = false;

					if (!ConfigManager.onlinePlayersUnderMinimap.Value)
					{
						// Header after players
						for (int i = 0; i < numOnlinePlayerSlots; i++)
						{
							if ((i < numPlayers) && !onePlayer)
							{
								UIPlayerTexts[i].enabled = showUI && !Chat.instance.IsChatDialogWindowVisible();
								if (showUI)
								{
									UIPlayerTexts[i].color = Color.white;
									UIPlayerTexts[i].text = playerInfoList[i].m_name;
								}
							}
							else if (i == numPlayers && !onePlayer)
							{
								UIPlayerTexts[i].enabled = showUI && !Chat.instance.IsChatDialogWindowVisible();
								if (showUI)
								{
									UIPlayerTexts[i].color = Color.green;
									UIPlayerTexts[i].text = "Online:";
								}
							}
							else
							{
								UIPlayerTexts[i].color = Color.white;
								UIPlayerTexts[i].text = "";
							}
						}
					}
					else
					{
						// Header before players
						if (!onePlayer)
						{
							UIPlayerTexts[0].enabled = showUI;
							if (showUI)
							{
								UIPlayerTexts[0].color = Color.green;
								UIPlayerTexts[0].text = "Online:";
							}

							for (int i = 1; i < numOnlinePlayerSlots; i++)
							{
								if (i <= numPlayers)
								{
									UIPlayerTexts[i].enabled = showUI;
									if (showUI)
									{
										UIPlayerTexts[i].color = Color.white;
										UIPlayerTexts[i].text = playerInfoList[i - 1].m_name;
									}
								}
								else
								{
									UIPlayerTexts[i].color = Color.white;
									UIPlayerTexts[i].text = "";
								}
							}
						}
					}
				}
				else
				{
					foreach (Text playerText in UIPlayerTexts)
					{
						if (playerText != null)
						{
							playerText.enabled = false;
						}
					}
				}
			}

			private static void CreateUI(Hud hud)
			{
				// Check if UI already exists (list not empty and all elements are valid)
				if (UIPlayerTexts.Count > 0 && UIPlayerTexts.All(t => t != null))
				{
					//MarsarahTweaks.MLog("[Warning] UI is already created");
					return; // UI is already created, no need to recreate
				}

				//MarsarahTweaks.MLog($"Executing CreateUI");

				int UITextFontSize = 16;
				string UITextFontName = "AveriaSansLibre-Bold";
				Vector2 UIPartyAreaSize = new Vector2(120f, 30f); // width, height

				// Party area object
				UIPartyArea = new GameObject("PartyArea");
				UIPartyArea.SetActive(false); // Hide the UI initially
				UIPartyArea.layer = 5;
				if (!ConfigManager.onlinePlayersUnderMinimap.Value)
					UIPartyArea.transform.SetParent(hud.m_rootObject.transform.parent); // Attach to the HUD root parent (entire UI)
				else
					UIPartyArea.transform.SetParent(hud.m_rootObject.transform); // Attach to the Mnimap

				RectTransform partyAreaTransform = UIPartyArea.AddComponent<RectTransform>();
				
				if (!ConfigManager.onlinePlayersUnderMinimap.Value)
				{
					partyAreaTransform.anchorMin = new Vector2(1f, 0f);
					partyAreaTransform.anchorMax = new Vector2(1f, 0f);
					partyAreaTransform.pivot = new Vector2(1f, 0f); // Set pivot to bottom-right of the screen
					partyAreaTransform.anchoredPosition = new Vector2(-20f, 5f);  // Offset from screen edge (bottom-right)
				}
				else
				{
					partyAreaTransform.anchorMin = new Vector2(1f, 1f);
					partyAreaTransform.anchorMax = new Vector2(1f, 1f);
					partyAreaTransform.pivot = new Vector2(0.5f, 0.5f); // Reset pivot to default
					partyAreaTransform.anchoredPosition = new Vector2(-170f, -255f); // Offset from minimap (bottom-left)
				}
				partyAreaTransform.sizeDelta = UIPartyAreaSize;
				UIPartyArea.transform.localScale = Vector3.one;  // Ensure correct scale

				// Background texture
				// skip...

				List<GameObject> partyTextObjects = new List<GameObject>();

				// Clear previous UI elements before recreating
				UIPlayerTexts.Clear();
				partyTextObjects.Clear();

				// Party area text objects & UI text
				for (int i = 0; i < numOnlinePlayerSlots; i++)
				{
					// Determine text alignment and row growth direction
					TextAnchor alignment = ConfigManager.onlinePlayersUnderMinimap.Value ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
					Vector2 anchoredPosition = ConfigManager.onlinePlayersUnderMinimap.Value ? new Vector2(0f, UIPartyPlayerTextDistanceV * i) : new Vector2(0f, -UIPartyPlayerTextDistanceV * i);

					// Create the text object using the helper function
					Text UIPlayerText = CreateTextObject(
						$"PartyText_{i}",
						UIPartyArea,
						Color.white,
						UITextFontName,
						UITextFontSize,
						alignment,
						anchoredPosition,
						UIPartyAreaSize
					);

					// Store the text object
					UIPlayerTexts.Add(UIPlayerText);
					partyTextObjects.Add(UIPlayerText.gameObject);
				}
			}

			private static void UpdatePartyUIPosition(Hud hud)
			{
				MarsarahTweaks.MLog("[Info] UpdatePartyUICalled");
				if (UIPartyArea == null) return;

				//MarsarahTweaks.MLog($"Executing UpdatePartyUIPosition");

				// Set new parent first, keeping world position to avoid undesired shifts
				if (!ConfigManager.onlinePlayersUnderMinimap.Value)
				{
					UIPartyArea.transform.SetParent(hud.m_rootObject.transform.parent, false); // Attach to the HUD root parent (entire UI)
				}
				else
				{
					UIPartyArea.transform.SetParent(hud.m_rootObject.transform, false); // Attach to the Mnimap
				}

				RectTransform partyAreaTransform = UIPartyArea.GetComponent<RectTransform>();

				if (!ConfigManager.onlinePlayersUnderMinimap.Value)
				{
					// Move to absolute bottom-right of the screen
					partyAreaTransform.anchorMin = new Vector2(1f, 0f);
					partyAreaTransform.anchorMax = new Vector2(1f, 0f);
					partyAreaTransform.pivot = new Vector2(1f, 0f); // Set pivot to bottom-right of the screen
					partyAreaTransform.anchoredPosition = new Vector2(-20f, 5f);  // Offset from screen edge
				}
				else
				{
					// Move to bottom-left near minimap
					partyAreaTransform.anchorMin = new Vector2(1f, 1f);
					partyAreaTransform.anchorMax = new Vector2(1f, 1f);
					partyAreaTransform.pivot = new Vector2(0.5f, 0.5f); // Reset pivot to default
					partyAreaTransform.anchoredPosition = new Vector2(-170f, -255f); // Offset from minimap																			 
				}

				// Update Text Objects (Alignment & Position)
				for (int i = 0; i < UIPlayerTexts.Count; i++)
				{
					Text UIPlayerText = UIPlayerTexts[i];

					if (UIPlayerText != null)
					{
						RectTransform textTransform = UIPlayerText.GetComponent<RectTransform>();

						// Update alignment
						UIPlayerText.alignment = ConfigManager.onlinePlayersUnderMinimap.Value ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;

						// Update text position within container
						textTransform.anchoredPosition = ConfigManager.onlinePlayersUnderMinimap.Value
							? new Vector2(0f, UIPartyPlayerTextDistanceV * i) // Move down from top
							: new Vector2(0f, -UIPartyPlayerTextDistanceV * i); // Move up from bottom
					}
				}
			}
		}
	}
}
