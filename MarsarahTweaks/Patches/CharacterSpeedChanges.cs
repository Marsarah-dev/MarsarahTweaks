using HarmonyLib;
using System.Collections.Generic;

namespace MarsarahTweaks.Patches
{
	internal class CharacterSpeedChanges
	{
		[HarmonyPatch(typeof(Character), "Awake")]
		class FasterCharacterSpeed_Patch
		{
			//static void Prefix(ref float ___m_crouchSpeed, ref float ___m_walkSpeed, ref float ___m_speed, ref float ___m_swimSpeed)
			static void Prefix(Character __instance)
			{
				if (ZNet.instance != null)
				{
					bool isDedicatedServer = ZNet.instance.IsDedicated();
					if (!isDedicatedServer)
					{
						if (!__instance.IsPlayer())
						{
							//MarsarahTweaks.MLog("Character Awake: Not a player, skipping speed modification.");
							return;
						}

						MarsarahTweaks.MLog($"Character Awake: Updating {ConfigManager.Configs.CharacterSpeedModifications.Name}...");
						//UpdateCharacterSpeed(___m_crouchSpeed, ___m_walkSpeed, ___m_speed, ___m_swimSpeed, false);
						UpdateCharacterSpeed(__instance, false);
					}
					else
					{
						MarsarahTweaks.MLog($"Character Awake: I am a server. No changes made to {ConfigManager.Configs.CharacterSpeedModifications.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"Character Awake: Too early to do anything. No changes made to {ConfigManager.Configs.CharacterSpeedModifications.Name}...");
				}
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

			if (ConfigManager.fasterCharacterSpeedEnabled.Value)
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
