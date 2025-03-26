using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches
{
	public static class BossStateChecker
	{
		// Dictionary to store defeated states for all bosses
		private static readonly Dictionary<string, bool> bossDefeatedStates = new Dictionary<string, bool>()
		{
			{ "defeated_eikthyr", false },
			{ "defeated_gdking", false },
			{ "defeated_bonemass", false },
			{ "defeated_dragon", false },
			{ "defeated_goblinking", false },
			{ "defeated_queen", false },
			{ "defeated_fader", false },
			{ "hildir1", false }, // turning in Brenna chest
			{ "hildir2", false }, // turning in Geirrhafa chest
			{ "hildir3", false }  // turning in ThungrNZil chest
		};

		public static void UpdateDefeatedStates(ZoneSystem zoneSystem)
		{
			List<string> globalKeys = zoneSystem.GetGlobalKeys();
			foreach (var key in bossDefeatedStates.Keys.ToList())
			{
				bool isDefeated = globalKeys.Contains(key);
				if (bossDefeatedStates[key] != isDefeated)
				{
					bossDefeatedStates[key] = isDefeated;
					MarsarahTweaks.MLog($"Boss state: {key} changed: {isDefeated}");
				}
			}
		}

		public static bool IsBossDefeated(string bossKey) => bossDefeatedStates.ContainsKey(bossKey) && bossDefeatedStates[bossKey];
	}
}
