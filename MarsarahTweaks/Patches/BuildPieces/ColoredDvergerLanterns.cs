using HarmonyLib;
using Jotunn.Configs;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches.BuildPieces
{
	internal class ColoredDvergerLanterns
	{
		private static readonly LogManager log = new LogManager("Colored Dverger Lanterns", LogManager.LogLevel.Warning);

		private static bool initialized = false;
		private static GameObject DvergrLanternPrefabBlue;
		private static GameObject DvergrLanternPrefabGreen;
		private static GameObject DvergrLanternPolePrefabBlue;
		private static GameObject DvergrLanternPolePrefabGreen;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		public static class ZNetScene_Awake_Patch
		{
			static void Postfix(ZNetScene __instance)
			{
				if (__instance == null || initialized)
					return;

				initialized = true;

				CreateColoredDvergrLanterns();
			}
		}

		private static void CreateColoredDvergrLanterns()
		{
			if (MPrefabManager.GetPrefab("piece_dvergr_lantern_blue") != null && MPrefabManager.GetPrefab("piece_dvergr_lantern_green") != null && 
				MPrefabManager.GetPrefab("piece_dvergr_lantern_pole_blue") != null && MPrefabManager.GetPrefab("piece_dvergr_lantern_pole_green") != null) return;

			// Clone dverger lanterns
			DvergrLanternPrefabBlue = CloneDvergrLanternPrefab("piece_dvergr_lantern", "piece_dvergr_lantern_blue");
			if (DvergrLanternPrefabBlue == null) return;
			DvergrLanternPrefabGreen = CloneDvergrLanternPrefab("piece_dvergr_lantern", "piece_dvergr_lantern_green");
			if (DvergrLanternPrefabGreen == null) return;
			DvergrLanternPolePrefabBlue = CloneDvergrLanternPrefab("piece_dvergr_lantern_pole", "piece_dvergr_lantern_pole_blue");
			if (DvergrLanternPolePrefabBlue == null) return;
			DvergrLanternPolePrefabGreen = CloneDvergrLanternPrefab("piece_dvergr_lantern_pole", "piece_dvergr_lantern_pole_green");
			if (DvergrLanternPolePrefabGreen == null) return;

			// Apply dverger lantern specific data
			SetupColoredDvergrLanternDefaults(DvergrLanternPrefabBlue, "Blue-colored Dvergr Wall Lantern", "blackforge");
			SetupColoredDvergrLanternDefaults(DvergrLanternPrefabGreen, "Green-colored Dvergr Wall Lantern", "blackforge");
			SetupColoredDvergrLanternDefaults(DvergrLanternPolePrefabBlue, "Blue-colored Dvergr Pole Lantern", "blackforge");
			SetupColoredDvergrLanternDefaults(DvergrLanternPolePrefabGreen, "Green-colored Dvergr Pole Lantern", "blackforge");

			// Modify dverger lantern blue and green lights
			Color blueEmission = new Color(0f / 255f, 60f / 255f, 255f / 255f) * 21.0f;
			ModifyLightSettings(DvergrLanternPrefabBlue, new Color(0.2f, 0.6f, 1f), blueEmission);
			ModifyLightSettings(DvergrLanternPolePrefabBlue, new Color(0.2f, 0.6f, 1f), blueEmission);

			Color greenEmission = new Color(0f / 255f, 191f / 255f, 20f / 255f) * 4.5f;
			ModifyLightSettings(DvergrLanternPrefabGreen, new Color(0.4f, 1f, 0.4f), greenEmission);
			ModifyLightSettings(DvergrLanternPolePrefabGreen, new Color(0.4f, 1f, 0.4f), greenEmission);

			// Modify dvergr lantern icons
			ModifyColoredDvergrLanternIcon(DvergrLanternPrefabBlue, new Color(0.3f, 0.85f, 1f));
			ModifyColoredDvergrLanternIcon(DvergrLanternPrefabGreen, new Color(0.2f, 1f, 0.4f));
			ModifyColoredDvergrLanternIcon(DvergrLanternPolePrefabBlue, new Color(0.3f, 0.85f, 1f));
			ModifyColoredDvergrLanternIcon(DvergrLanternPolePrefabGreen, new Color(0.2f, 1f, 0.4f));

			// Add colored dvergr lanterns to ZNetScene
			MPrefabManager.RegisterToZNetScene(DvergrLanternPrefabBlue);
			MPrefabManager.RegisterToZNetScene(DvergrLanternPrefabGreen);
			MPrefabManager.RegisterToZNetScene(DvergrLanternPolePrefabBlue);
			MPrefabManager.RegisterToZNetScene(DvergrLanternPolePrefabGreen);

			// Configure the Piece data and add to buiild menu
			ConfigureColoredDvergrLanternPieceData(DvergrLanternPrefabBlue);
			ConfigureColoredDvergrLanternPieceData(DvergrLanternPrefabGreen);
			ConfigureColoredDvergrLanternPieceData(DvergrLanternPolePrefabBlue);
			ConfigureColoredDvergrLanternPieceData(DvergrLanternPolePrefabGreen);

			// Toggle visibility
			ToggleColoredDvergrLanternsVisibility();

			DvergrLanternPrefabBlue.SetActive(true);
			DvergrLanternPrefabGreen.SetActive(true);
			DvergrLanternPolePrefabBlue.SetActive(true);
			DvergrLanternPolePrefabGreen.SetActive(true);
			log.Info("Colored Dvergr Lanterns registered and ready.");
		}

		private static GameObject CloneDvergrLanternPrefab(string sourcePrefabName, string newPrefabName)
		{
			GameObject prefab = MPrefabManager.ClonePrefab(sourcePrefabName, newPrefabName);
			if (prefab == null)
			{
				log.Error($"Cloning of {sourcePrefabName} failed.");
			}

			return prefab;
		}

		private static void SetupColoredDvergrLanternDefaults(GameObject prefab, string name, string craftingStation = null)
		{
			ZNetView znet = prefab.GetComponent<ZNetView>();
			if (znet != null)
			{
				znet.m_persistent = true;
				znet.m_distant = false;
				znet.m_type = ZDO.ObjectType.Default;
				znet.m_syncInitialScale = true;
			}

			Piece piece = prefab.GetComponent<Piece>();
			if (piece != null)
			{
				piece.m_enabled = true;
				piece.m_name = name;
				piece.m_description = "";

				GameObject craftingStationPrefab = MPrefabManager.GetPrefab(craftingStation);
				if (craftingStationPrefab != null)
				{
					CraftingStation newCraftingStation = craftingStationPrefab.GetComponent<CraftingStation>();
					if (newCraftingStation != null)
					{
						piece.m_craftingStation = newCraftingStation;
						log.Info($"Crafting station set to {newCraftingStation.name} for piece {piece.name}");
					}
				}
			}
		}

		private static void ModifyLightSettings(GameObject prefab, Color targetColor, Color emissionColor)
		{
			if (prefab == null)
			{
				log.Warn("ModifyLightSettings called with null prefab");
				return;
			}

			log.Info($"Modifying light settings for {prefab.name}...");

			// Loop through New, Worn, Broken
			foreach (string state in new[] { "New", "Worn", "Broken" })
			{
				Transform stateRoot = prefab.transform.Find(state);
				if (stateRoot == null)
				{
					log.Warn($"State '{state}' not found on {prefab.name}");
					continue;
				}

				// --- Default mesh emissive ---
				Transform defaultChild = stateRoot.Find("default");
				if (defaultChild != null && defaultChild.TryGetComponent(out Renderer rend))
				{
					foreach (Material mat in rend.materials)
					{
						if (mat.name.Contains("DvergrTownLantern_mat"))
						{
							// Base color 
							if (mat.HasProperty("_Color"))
							{
								mat.SetColor("_Color", targetColor);
							}

							// Emission color
							if (mat.HasProperty("_EmissionColor"))
							{
								mat.SetColor("_EmissionColor", emissionColor);
								mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
								DynamicGI.SetEmissive(rend, emissionColor);
							}

							log.Info($"Changed emissive material for {state} on {prefab.name}");
						}
					}
				}

				// --- Point Light ---
				Transform pointLight = stateRoot.Find("Point Light");
				if (pointLight != null && pointLight.TryGetComponent(out Light lightComp))
				{
					lightComp.color = targetColor;
					log.Info($"Set Point Light color for {state} on {prefab.name}");
				}
				else
				{
					log.Warn($"No Point Light found under {state} on {prefab.name}");
				}

				// --- Flare (ParticleSystem or Renderer) ---
				Transform flare = stateRoot.Find("flare");
				if (flare != null)
				{
					// ParticleSystem flare
					if (flare.TryGetComponent(out ParticleSystem ps))
					{
						var main = ps.main;
						Color flareColor = targetColor;
						flareColor.a = 0.1f; // soften intensity
						main.startColor = flareColor;

						log.Info($"Set ParticleSystem flare color for {state} on {prefab.name} (alpha reduced)");
					}

					// Renderer flare
					if (flare.TryGetComponent(out Renderer renderer))
					{
						foreach (Material mat in renderer.materials)
						{
							if (mat.HasProperty("_Color"))
								mat.SetColor("_Color", targetColor);
							if (mat.HasProperty("_EmissionColor"))
								mat.SetColor("_EmissionColor", targetColor);
						}
						log.Info($"Set Renderer flare color for {state} on {prefab.name}");
					}
				}
				else
				{
					log.Warn($"No flare found under {state} on {prefab.name}");
				}
			}

			UpdateColoredDvergrLanternsIntensity();
		}

		private static Sprite ModifyColoredDvergrLanternIcon(GameObject prefab, Color targetTint)
		{
			Piece sconcePiece = prefab.GetComponent<Piece>();
			if (sconcePiece == null)
			{
				log.Warn("Piece prefab is null.");
				return null;
			}

			Sprite originalIcon = sconcePiece.m_icon;
			if (originalIcon == null)
			{
				log.Warn($"Piece '{sconcePiece.name}' has no icon assigned.");
				return null;
			}
			if (originalIcon.texture == null)
			{
				log.Warn($"Original icon texture is null for piece '{sconcePiece.name}'.");
				return null;
			}

			Rect atlasRect = new Rect(2136, 72, 64, 64); // 2136, 160

			if (prefab.name.Contains("pole"))
			{
				atlasRect = new Rect(1872, 214, 64, 64); // 1880, 288
			}

			// Copy only the sub-region from the atlas
			Texture2D croppedTex = new Texture2D((int)atlasRect.width, (int)atlasRect.height, TextureFormat.RGBA32, false);
			RenderTexture rt = RenderTexture.GetTemporary(originalIcon.texture.width, originalIcon.texture.height, 0, RenderTextureFormat.ARGB32);
			Graphics.Blit(originalIcon.texture, rt);
			RenderTexture.active = rt;

			croppedTex.ReadPixels(atlasRect, 0, 0);
			croppedTex.Apply();

			Color[] pixels = croppedTex.GetPixels();
			for (int i = 0; i < pixels.Length; i++)
			{
				Color c = pixels[i];

				// Detect bright red/orange pixels
				bool isGlow = false;

				// Option 1: yellow/orange
				if (c.r > 0.6f && c.g > 0.3f && c.b < 0.4f)
					isGlow = true;

				// Option 2: deep red
				else if (c.r > 0.6f && c.g < 0.35f && c.b < 0.35f)
					isGlow = true;

				if (isGlow)
				{
					// Luminance-based grayscale (better than average for perception)
					float intensity = c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;

					// Apply target tint scaled by intensity
					pixels[i] = new Color(
						targetTint.r * intensity,
						targetTint.g * intensity,
						targetTint.b * intensity,
						c.a
					);

					/*float maxChannel = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
					float intensity = maxChannel; // use strongest channel as brightness

					pixels[i] = new Color(
						targetTint.r * intensity,
						targetTint.g * intensity,
						targetTint.b * intensity,
						c.a
					);*/

					/*float intensity = (c.r * 0.299f + c.g * 0.587f + c.b * 0.114f);
					float brightnessBoost = 1.2f; // tweak until matches originals

					pixels[i] = targetTint * (intensity * brightnessBoost);
					pixels[i].a = c.a;*/
				}
				else
				{
					pixels[i] = c;
				}
			}
			croppedTex.SetPixels(pixels);
			croppedTex.Apply();

			RenderTexture.ReleaseTemporary(rt);
			RenderTexture.active = null;

			// Create new Sprite from cropped region
			Sprite newIcon = Sprite.Create(
				croppedTex,
				new Rect(0, 0, croppedTex.width, croppedTex.height),
				new Vector2(0.5f, 0.5f),
				originalIcon.pixelsPerUnit
			);

			// Replace the icon on the Piece
			sconcePiece.m_icon = newIcon;

			log.Info($"Icon changed for {prefab.name}");

			return newIcon;
		}

		private static void ConfigureColoredDvergrLanternPieceData(GameObject prefab)
		{
			string resourceMetal = "Copper";
			string resourceFuel = "Lantern";
			string resourceChain = "Chain";

			log.Info($"Prefab name: {prefab.name}");
			int metalAmount = prefab.name.Contains("pole") ? 3 : 2;
			log.Info($"Metal amount: {metalAmount}");

			var pieceConfig = new PieceConfig
			{
				PieceTable = "Hammer",
				Category = "Furniture",
				Requirements = new[]
				{
					new RequirementConfig(resourceMetal, ConfigManager.BuildPieceAmountsEnabled.Value? metalAmount - 1 : metalAmount, recover: true),
					new RequirementConfig(resourceFuel, 1, recover: true),
					new RequirementConfig(resourceChain, 1, recover: true)
				}
			};

			MPrefabManager.AddToBuildMenu(prefab, pieceConfig);
		}

		public static void ToggleColoredDvergrLanternsVisibility()
		{
			Piece dvergrLanternPieceBlue = DvergrLanternPrefabBlue?.GetComponent<Piece>();
			Piece dvergrLanternPieceGreen = DvergrLanternPrefabGreen?.GetComponent<Piece>();
			Piece dvergrLanternPolePieceBlue = DvergrLanternPolePrefabBlue?.GetComponent<Piece>();
			Piece dvergrLanternPolePieceGreen = DvergrLanternPolePrefabGreen?.GetComponent<Piece>();

			if (dvergrLanternPieceBlue == null || dvergrLanternPieceGreen == null || dvergrLanternPolePieceBlue == null || dvergrLanternPolePieceGreen == null)
			{
				log.Warn($"Piece component does not exist. No toggle made.");
				return;
			}

			if (ConfigManager.BuildPiecesLightingEnabled.Value)
			{
				dvergrLanternPieceBlue.m_enabled = true;
				dvergrLanternPieceGreen.m_enabled = true;
				dvergrLanternPolePieceBlue.m_enabled = true;
				dvergrLanternPolePieceGreen.m_enabled = true;
			}
			else
			{
				dvergrLanternPieceBlue.m_enabled = false;
				dvergrLanternPieceGreen.m_enabled = false;
				dvergrLanternPolePieceBlue.m_enabled = false;
				dvergrLanternPolePieceGreen.m_enabled = false;
			}
		}

		public static void RefreshColoredDvergrLanternsRequirements()
		{
			RefreshColoredDvergrLanternRequirements(DvergrLanternPrefabBlue);
			RefreshColoredDvergrLanternRequirements(DvergrLanternPrefabGreen);
			RefreshColoredDvergrLanternRequirements(DvergrLanternPolePrefabBlue);
			RefreshColoredDvergrLanternRequirements(DvergrLanternPolePrefabGreen);
		}

		private static void RefreshColoredDvergrLanternRequirements(GameObject coloredDvergrLantern)
		{
			if (coloredDvergrLantern == null) return;

			string resourceMetal = "Copper";
			string resourceFuel = "Lantern";
			string resourceChain = "Chain";

			int metalAmount = coloredDvergrLantern.name.Contains("pole") ? 3 : 2;

			var piece = coloredDvergrLantern.GetComponent<Piece>();
			if (piece == null) return;

			// Clear & rebuild requirements
			var metal = MPrefabManager.GetPrefab(resourceMetal)?.GetComponent<ItemDrop>();
			var fuel = MPrefabManager.GetPrefab(resourceFuel)?.GetComponent<ItemDrop>();
			var chain = MPrefabManager.GetPrefab(resourceChain)?.GetComponent<ItemDrop>();

			if (metal == null || fuel == null || chain == null)
			{
				log.Error($"Missing one or more resource prefabs ({resourceMetal}, {resourceFuel}, {resourceChain}).");
				return;
			}

			piece.m_resources = new[]
			{
				new Piece.Requirement { m_resItem = metal, m_amount = ConfigManager.BuildPieceAmountsEnabled.Value? metalAmount - 1 : metalAmount, m_recover = true },
				new Piece.Requirement { m_resItem = fuel, m_amount = 1, m_recover = true },
				new Piece.Requirement { m_resItem = chain, m_amount = 1, m_recover = true }
			};

			log.Info($"Refreshed build requirements: Metal={piece.m_resources[0].m_amount}");
		}

		public static void UpdateColoredDvergrLanternsIntensity()
		{
			UpdateLightIntensity(DvergrLanternPrefabBlue);
			UpdateLightIntensity(DvergrLanternPrefabGreen);
			UpdateLightIntensity(DvergrLanternPolePrefabBlue);
			UpdateLightIntensity(DvergrLanternPolePrefabGreen);
		}

		private static void UpdateLightIntensity(GameObject prefab)
		{
			float defaultIntensity = 1.5f;
			float defaultRange = 6f;
			float defaultFlickerIntensity = 0.1f;
			float defaultFlickerSpeed = 10f;

			float brighterIntensity = 2f;
			float brighterRange = prefab.name.Contains("pole") ? 15f : 9f;
			float brighterFlickerIntensity = 0.05f;
			float brighterFlickerSpeed = 5f;

			Light lightComponent = prefab.GetComponentInChildren<Light>();
			LightFlicker flicker = prefab.GetComponentInChildren<LightFlicker>();

			if (ConfigManager.BrighterLanternsEnabled.Value)
			{
				if (lightComponent != null)
				{
					lightComponent.intensity = brighterIntensity;
					lightComponent.range = brighterRange;
					log.Info($"Set intensity {brighterIntensity} and range {brighterRange} for {prefab.name}");
				}

				if (flicker != null)
				{
					flicker.m_flickerIntensity = brighterFlickerIntensity;
					flicker.m_flickerSpeed = brighterFlickerSpeed;
					log.Info($"Set flicker intensity {brighterFlickerIntensity} and speed {brighterFlickerSpeed} for {prefab.name}");
				}
			}
			else
			{
				if (lightComponent != null)
				{
					lightComponent.intensity = defaultIntensity;
					lightComponent.range = defaultRange;
					log.Info($"Reverted intensity {defaultIntensity} and range {defaultRange} for {prefab.name}");
				}

				if (flicker != null)
				{
					flicker.m_flickerIntensity = defaultFlickerIntensity;
					flicker.m_flickerSpeed = defaultFlickerSpeed;
					log.Info($"Reverted flicker intensity {defaultFlickerIntensity} and speed {defaultFlickerSpeed} for {prefab.name}");
				}
			}
		}
	}
}
