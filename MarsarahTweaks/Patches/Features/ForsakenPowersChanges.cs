using HarmonyLib;
using System.Collections.Generic;

namespace MarsarahTweaks.Patches.Features
{
	internal class ForsakenPowersChanges
	{
		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class ForsakenPowers_Patch
		{
			static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateForsakenPowers(__instance, false);

			}
		}

		// Dictionaries
		private static readonly Dictionary<string, (float duration, float cooldown)> originalValues = new Dictionary<string, (float duration, float cooldown)>();
		private static readonly Dictionary<string, (float duration, float cooldown)> modifiedValues = new Dictionary<string, (float duration, float cooldown)>()
		{
			{ "GP_Eikthyr", (600f, 900f) }, // 10 mins, 15 mins
			{ "GP_TheElder", (600f, 900f) }, // 10 mins, 15 mins
            { "GP_Bonemass", (450f, 1050f) }, // 7.5 mins, 17.5 mins
			{ "GP_Moder", (600f, 900f) }, // 10 mins, 15 mins
			{ "GP_Yagluth", (450f, 1050f) }, // 7.5 mins, 17.5 mins
			{ "GP_Queen", (450f, 900f) }, // 7.5 mins, 15 mins
			{ "GP_Fader", (450f, 900f) } // 7.5 mins, 15 mins
		};

		public static void UpdateForsakenPowers(ObjectDB objDB, bool wasChanged)
		{
			foreach (StatusEffect statusEffect in objDB.m_StatusEffects)
			{
				if (statusEffect == null || !modifiedValues.ContainsKey(statusEffect.name)) continue;

				var (newDuration, newCooldown) = modifiedValues[statusEffect.name];

				if (ConfigManager.longerForsakenPowersEnabled.Value)
				{
					if (!originalValues.ContainsKey(statusEffect.name))
					{
						originalValues[statusEffect.name] = (statusEffect.m_ttl, statusEffect.m_cooldown);
						//MarsarahTweaks.MLog($"Backed up {statusEffect.name}: TTL={statusEffect.m_ttl}, CD={statusEffect.m_cooldown}");
					}

					if (statusEffect.m_ttl != newDuration)
					{
						//MarsarahTweaks.MLog($"Updating {statusEffect.name} duration: {statusEffect.m_ttl} -> {newDuration}");
						statusEffect.m_ttl = newDuration;
					}

					if (statusEffect.m_cooldown != newCooldown)
					{
						//MarsarahTweaks.MLog($"Updating {statusEffect.name} cooldown: {statusEffect.m_cooldown} -> {newCooldown}");
						statusEffect.m_cooldown = newCooldown;
					}
				}
				else if (wasChanged && originalValues.TryGetValue(statusEffect.name, out var original))
				{
					if (statusEffect.m_ttl != original.duration)
					{
						//MarsarahTweaks.MLog($"Restoring {statusEffect.name} duration: {statusEffect.m_ttl} -> {original.duration}");
						statusEffect.m_ttl = original.duration;
					}

					if (statusEffect.m_cooldown != original.cooldown)
					{
						//MarsarahTweaks.MLog($"Restoring {statusEffect.name} cooldown: {statusEffect.m_cooldown} -> {original.cooldown}");
						statusEffect.m_cooldown = original.cooldown;
					}

					originalValues.Remove(statusEffect.name);
				}
			}
		}
	}
}
