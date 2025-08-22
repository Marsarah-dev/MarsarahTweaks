using HarmonyLib;
using System.Collections.Generic;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Features
{
	internal class CharacterSpeedChanges
	{
		private static readonly LogManager log = new LogManager("Character Speed", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(Character), "Awake")]
		class FasterCharacterSpeed_Patch
		{
			static void Prefix(Character __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (!__instance.IsPlayer())
				{
					log.Info("Character Awake: Not a player, skipping speed modification.");
					return;
				}

				UpdateCharacterSpeed(__instance, false);
			}
		}

		// Backup Dictionary
		private static Dictionary<string, float> originalSpeeds = new Dictionary<string, float>();

		public static void UpdateCharacterSpeed(Character character, bool wasChanged)
		{
			if (!wasChanged)
			{
				// Backup original values only once
				originalSpeeds["crouchSpeed"] = character.m_crouchSpeed;
				originalSpeeds["walkSpeed"] = character.m_walkSpeed;
				originalSpeeds["speed"] = character.m_speed;
				originalSpeeds["swimSpeed"] = character.m_swimSpeed;

				log.Info($"Backed up original speed values - Crouch: {character.m_crouchSpeed}, Walk: {character.m_walkSpeed}, Run: {character.m_speed}, Swim: {character.m_swimSpeed}.");
			}

			if (ConfigManager.FasterCharacterSpeedEnabled.Value)
			{
				// Apply modified values
				character.m_crouchSpeed = 2.2f; // Default: 2
				character.m_walkSpeed = 2.5f;   // Default: 1.6
				character.m_speed = 5f;         // Default: 4
				character.m_swimSpeed = 2.2f;   // Default: 2

				log.Info($"Applied modified speed values - Crouch: {character.m_crouchSpeed}, Walk: {character.m_walkSpeed}, Run: {character.m_speed}, Swim: {character.m_swimSpeed}.");
			}
			else if (wasChanged && originalSpeeds.Count > 0)
			{
				// Restore original values
				character.m_crouchSpeed = originalSpeeds["crouchSpeed"];
				character.m_walkSpeed = originalSpeeds["walkSpeed"];
				character.m_speed = originalSpeeds["speed"];
				character.m_swimSpeed = originalSpeeds["swimSpeed"];

				log.Info($"Restored original speed values - Crouch: {character.m_crouchSpeed}, Walk: {character.m_walkSpeed}, Run: {character.m_speed}, Swim: {character.m_swimSpeed}.");
			}
		}
	}
}
