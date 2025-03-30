using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches
{
	internal class LessFallDamage
	{
		[HarmonyPatch(typeof(SEMan), "ModifyFallDamage")]
		public class FallDamage_Patch
		{
			public static void Prefix(ref float damage)
			{
				if (ZNet.instance != null)
				{
					bool isDedicatedServer = ZNet.instance.IsDedicated();
					if (!isDedicatedServer)
					{
						//MarsarahTweaks.MLog($"SEMan: Updating {ConfigManager.Configs.LessFallDamage.Name}...");

						if (ConfigManager.lessFallDamageEnabled.Value)
						{
							//MarsarahTweaks.MLog($"Reducing fall damage...");
							damage = damage * 0.6f;
						}
					}
					else
					{
						MarsarahTweaks.MLog($"SEMan: I am a server. No changes made to {ConfigManager.Configs.LessFallDamage.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"SEMan: Too early to do anything. No changes made to {ConfigManager.Configs.LessFallDamage.Name}...");
				}
			}
		}
	}
}
