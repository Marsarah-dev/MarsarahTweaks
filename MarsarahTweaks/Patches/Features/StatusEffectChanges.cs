using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.Features
{
	internal class StatusEffectChanges
	{
		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class StatusEffect_Patch
		{
			static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null)
				{
					if (!ZNet.instance.IsDedicated())
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: Updating {ConfigManager.Configs.StatusEffectsModifications.Name}...");
						UpdateStatusEffects(__instance, false);
					}
					else
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: I am a server. No changes made to {ConfigManager.Configs.StatusEffectsModifications.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"ObjectDB Awake: Too early to do anything. No changes made to {ConfigManager.Configs.StatusEffectsModifications.Name}...");
				}
			}
		}

		// Dictionaries
		private static Dictionary<string, float> originalDurations = new Dictionary<string, float>();
		private static readonly Dictionary<string, float> customDurations = new Dictionary<string, float>()
		{
			{ "Potion_eitr_minor", 60f },
			{ "Potion_health_major", 75f },
			{ "Potion_health_medium", 60f },
			{ "Potion_health_minor", 45f },
			{ "Potion_stamina_medium", 60f },
			{ "Potion_stamina_minor", 45f },
			{ "Wet", 60f }
		};

		public static void UpdateStatusEffects(ObjectDB objDB, bool wasChanged)
		{
			foreach (StatusEffect statusEffect in objDB.m_StatusEffects)
			{
				if (customDurations.TryGetValue(statusEffect.name, out float newDuration))
				{
					if (ConfigManager.shorterStatusEffectsEnabled.Value)
					{
						if (!originalDurations.ContainsKey(statusEffect.name))
						{
							//MarsarahTweaks.MLog($"Backing up Status Effect for: {statusEffect.name}. Value: {statusEffect.m_ttl}");
							originalDurations[statusEffect.name] = statusEffect.m_ttl;
						}

						//MarsarahTweaks.MLog($"Applying Status Effect for: {statusEffect.name}. Old value: {statusEffect.m_ttl}, New value: {newDuration}");
						statusEffect.m_ttl = newDuration;
					}
					else if (wasChanged && originalDurations.TryGetValue(statusEffect.name, out float originalValue))
					{
						//MarsarahTweaks.MLog($"Restoring Status Effect for: {statusEffect.name}. Restored value: {originalValue}");
						statusEffect.m_ttl = originalValue;
						originalDurations.Remove(statusEffect.name);
					}
				}
			}
		}
	}
}
