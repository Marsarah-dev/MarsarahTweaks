using HarmonyLib;
using System.Collections.Generic;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Balance
{
	internal class ForsakenPowersChanges
	{
		private static readonly LogManager log = new LogManager("Forsaken Powers", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class ForsakenPowers_Patch
		{
			private static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateForsakenPowers(__instance, false);

			}
		}

		// Powers reference
		/* Eikthyr:
		 * Old: Jump stamina usage: -60%. Run stamina usage: -60%.
		 * New: Jump stamina usage: -60%. Run stamina usage: -60%. Swimming stamina usage: -60%.
		 * 
		 * The Elder
		 * Old: Chopping damage: +60%.
		 * New: Chopping damage: +60%. Mining damage: +60%. Health regeneration: +30%.
		 * 
		 * Bonemass:
		 * Old: Blunt resistance: +50%. Slash resistance: +50%. Pierce resistance: +50%.
		 * New: Blunt resistance: +25%. Slash resistance: +25%. Pierce resistance: +25%. Block stamina usage: -100%. Stamina on return per block: +5.
		 * 
		 * Moder:
		 * Old: Forces tailwind when sailing.
		 * New: Always tailwind. Carry weight: +300. Movement speed: +10%. Frost resistance: +50%.
		 * 
		 * Yagluth:
		 * Old: Fire resistance: +50%. Frost resistance: +50%. Lightning resistance: +50%.
		 * New: Lightning resistance: +50%. Farm skill: +25. Damage: +10%.
		 * 
		 * The Queen:
		 * Old: Eitr regeneration: +100%. Mining damage: +60%. 
		 * New: Eitr regeneration: +100%. Sneak stamina cost: -100%. Poison resistance: +50%.
		 * 
		 * Fader:
		 * Old: Carry weight: +300. Movement speed: +10%. 
		 * New: Adrenaline: +100%. Stagger meter: -50%. Fire resistance: +50%.
		 */

		// Dictionaries
		private static readonly Dictionary<string, (float duration, float cooldown)> originalValues = new Dictionary<string, (float duration, float cooldown)>();
		private static readonly Dictionary<string, (float duration, float cooldown)> modifiedValues = new Dictionary<string, (float duration, float cooldown)>()
		{
			{ "GP_Eikthyr", (600f, 900f) }, // 10 mins, 15 mins
			{ "GP_TheElder", (600f, 900f) }, // 10 mins, 15 mins
            { "GP_Bonemass", (450f, 900f) }, // 7.5 mins, 15 mins - 7.5 mins, 17.5 mins (450f, 1050f)
			{ "GP_Moder", (600f, 900f) }, // 10 mins, 15 mins
			{ "GP_Yagluth", (450f, 900f) }, // 7.5 mins, 15 mins - 7.5 mins, 17.5 mins (450f, 1050f)
			{ "GP_Queen", (450f, 900f) }, // 7.5 mins, 15 mins
			{ "GP_Fader", (450f, 900f) } // 7.5 mins, 15 mins
		};

		public static void UpdateForsakenPowers(ObjectDB objDB, bool wasChanged)
		{
			foreach (StatusEffect statusEffect in objDB.m_StatusEffects)
			{
				if (statusEffect == null || !modifiedValues.ContainsKey(statusEffect.name)) continue;

				var (newDuration, newCooldown) = modifiedValues[statusEffect.name];

				if (ConfigManager.LongerForsakenPowersEnabled.Value)
				{
					if (!originalValues.ContainsKey(statusEffect.name))
					{
						originalValues[statusEffect.name] = (statusEffect.m_ttl, statusEffect.m_cooldown);
						log.Info($"Backed up {statusEffect.name}: TTL={statusEffect.m_ttl}, CD={statusEffect.m_cooldown}");
					}

					if (statusEffect.m_ttl != newDuration)
					{
						log.Info($"Updating {statusEffect.name} duration: {statusEffect.m_ttl} -> {newDuration}");
						statusEffect.m_ttl = newDuration;
					}

					if (statusEffect.m_cooldown != newCooldown)
					{
						log.Info($"Updating {statusEffect.name} cooldown: {statusEffect.m_cooldown} -> {newCooldown}");
						statusEffect.m_cooldown = newCooldown;
					}
				}
				else if (wasChanged && originalValues.TryGetValue(statusEffect.name, out var original))
				{
					if (statusEffect.m_ttl != original.duration)
					{
						log.Info($"Restoring {statusEffect.name} duration: {statusEffect.m_ttl} -> {original.duration}");
						statusEffect.m_ttl = original.duration;
					}

					if (statusEffect.m_cooldown != original.cooldown)
					{
						log.Info($"Restoring {statusEffect.name} cooldown: {statusEffect.m_cooldown} -> {original.cooldown}");
						statusEffect.m_cooldown = original.cooldown;
					}

					originalValues.Remove(statusEffect.name);
				}
			}
		}
	}
}
