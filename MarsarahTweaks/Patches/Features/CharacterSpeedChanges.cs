using HarmonyLib;
using System.Collections.Generic;

namespace MarsarahTweaks.Patches.Features
{
	internal class CharacterSpeedChanges
	{
		[HarmonyPatch(typeof(Character), "Awake")]
		class FasterCharacterSpeed_Patch
		{
			static void Prefix(Character __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (!__instance.IsPlayer())
				{
					//MarsarahTweaks.MLog("Character Awake: Not a player, skipping speed modification.");
					return;
				}

				UpdateCharacterSpeed(__instance, false);
			}
		}

		// Backup Dictionary
		private static Dictionary<string, float> originalSpeeds = new Dictionary<string, float>();

		//private static void UpdateCharacterSpeed(float crouchSpeed, float walkSpeed, float speed, float swimSpeed, bool wasChanged)
		public static void UpdateCharacterSpeed(Character character, bool wasChanged)
		{
			if (!wasChanged)
			{
				// Backup original values only once
				originalSpeeds["crouchSpeed"] = character.m_crouchSpeed;
				originalSpeeds["walkSpeed"] = character.m_walkSpeed;
				originalSpeeds["speed"] = character.m_speed;
				originalSpeeds["swimSpeed"] = character.m_swimSpeed;

				//MarsarahTweaks.MLog($"CharacterSpeedChanges: Backed up original speed values - Crouch: {character.m_crouchSpeed}, Walk: {character.m_walkSpeed}, Run: {character.m_speed}, Swim: {character.m_swimSpeed}.");
			}

			if (ConfigManager.FasterCharacterSpeedEnabled.Value)
			{
				// Apply modified values
				character.m_crouchSpeed = 2.2f; // Default: 2
				character.m_walkSpeed = 2.5f;   // Default: 1.6
				character.m_speed = 5f;         // Default: 4
				character.m_swimSpeed = 2.2f;   // Default: 2

				//MarsarahTweaks.MLog($"CharacterSpeedChanges: Applied modified speed values - Crouch: {character.m_crouchSpeed}, Walk: {character.m_walkSpeed}, Run: {character.m_speed}, Swim: {character.m_swimSpeed}.");
			}
			else if (wasChanged && originalSpeeds.Count > 0)
			{
				// Restore original values
				character.m_crouchSpeed = originalSpeeds["crouchSpeed"];
				character.m_walkSpeed = originalSpeeds["walkSpeed"];
				character.m_speed = originalSpeeds["speed"];
				character.m_swimSpeed = originalSpeeds["swimSpeed"];

				//MarsarahTweaks.MLog($"CharacterSpeedChanges: Restored original speed values - Crouch: {character.m_crouchSpeed}, Walk: {character.m_walkSpeed}, Run: {character.m_speed}, Swim: {character.m_swimSpeed}.");
			}
		}

		/*public static void UpdateCharacterSpeedExternal(Character character, bool wasChanged)
		{
			UpdateCharacterSpeed(character.m_crouchSpeed, character.m_walkSpeed, character.m_speed, character.m_swimSpeed, wasChanged);
		}*/
	}
}
