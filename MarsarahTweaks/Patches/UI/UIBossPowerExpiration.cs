using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.UI
{
	internal class UIBossPowerExpiration
	{
		[HarmonyPatch(typeof(StatusEffect), nameof(StatusEffect.Stop))]
		public class StatusEffectStopPatch
		{
			public static void Postfix(StatusEffect __instance)
			{
				if (!ConfigManager.ShowBossExpirationMessage.Value) return;

				Character character = __instance.m_character;
				if (character == null || !character.IsPlayer() || !character.IsOwner()) return;

				//string seName = __instance.name.ToLowerInvariant();
				string seName = __instance.name;

				//MarsarahTweaks.LogInfo($"SE Name: {seName}");

				if (seName.Contains("GP_"))
				{
					string powerName = Localization.instance.Localize(__instance.m_name);

					//MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, $"{powerName} Power Expired"); // This prints where loot messages print at the top left
					character.Message(MessageHud.MessageType.Center, $"{powerName} Power Expired"); // This prints in the very center
				}
			}
		}
	}
}
