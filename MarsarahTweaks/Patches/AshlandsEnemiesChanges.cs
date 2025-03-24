using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches
{
	internal class AshlandsEnemiesChanges
	{
		private static bool spawnChangesApplied = false;

		[HarmonyPatch(typeof(SpawnSystem), "Awake")]
		class LessAshlandsEnemiesAwake_Patch
		{
			private static void Postfix(SpawnSystem __instance)
			{
				if (!ZNet.instance || !ZNet.instance.IsServer() || spawnChangesApplied) return; // Do not run on clients or if changes are already applied

				if (__instance != null && ConfigManager.lessAshlandsEnemiesEnabled.Value)
				{
					UpdateAshlandsSpawns(__instance);
					spawnChangesApplied = true;
				}
			}
		}

		private static readonly Dictionary<string, (int? maxSpawned, int? groupMin, int? groupMax, float? spawnChance)> spawnAdjustments = new Dictionary<string, (int? maxSpawned, int? groupMin, int? groupMax, float? spawnChance)>()
		{
			{ "Fallen Valkyrie",			(null, null, null, 15f) },	// 1, 1, 1, 20
			{ "Asksvin [DAY]",				(1, null, 2, 20f) },		// 2, 1, 3, 30
			{ "Asksvin [NIGHT]",			(2, null, null, 35f) },		// 3, 1, 3, 45
			{ "Volture",					(2, null, null, null) },	// 3, 1, 2, 20
			{ "Charred Twitcher [DAY]",		(2, 1, 2, 35f) },			// 3, 2, 4, 40
			{ "Charred Twitcher [NIGHT]",	(3, 2, 3, null) },			// 4, 3, 6, 45
			{ "Charred Archer",				(2, null, null, 30f) },		// 4, 1, 2, 35
			{ "Charred Melee",				(2, null, null, 30f) },		// 4, 1, 2, 35
			{ "Lava Blob",					(1, null, 1, 20f) },		// 2, 1, 2, 25
		};

		private static void UpdateAshlandsSpawns(SpawnSystem spawnSystem)
		{
			foreach (SpawnSystemList spawnList in spawnSystem.m_spawnLists)
			{
				// Super cool log from ChatGPT
				//MarsarahTweaks.MLog($"| {"Enemy",-25} | {"MAX",-3} | {"Group Min",-9} | {"Group Max",-9} | {"Chance",-6} |");
				//MarsarahTweaks.MLog(new string('-', 60)); // Separator line

				foreach (SpawnSystem.SpawnData spawner in spawnList.m_spawners)
				{
					if (spawnAdjustments.TryGetValue(spawner.m_name, out var newValues))
					{
						//MarsarahTweaks.MLog($"| {spawner.m_name,-25} | {spawner.m_maxSpawned,-3} | {spawner.m_groupSizeMin,-9} | {spawner.m_groupSizeMax,-9} | {spawner.m_spawnChance,-6}% |");

						// Apply the new values
						//string logMessage = $"Updating {spawner.m_name}:";

						// Apply and log changes
						if (newValues.maxSpawned.HasValue)
						{
							//logMessage += $" Max Spawned {spawner.m_maxSpawned} → {newValues.maxSpawned.Value},";
							spawner.m_maxSpawned = newValues.maxSpawned.Value;
						}
						if (newValues.groupMin.HasValue)
						{
							//logMessage += $" Group Min {spawner.m_groupSizeMin} → {newValues.groupMin.Value},";
							spawner.m_groupSizeMin = newValues.groupMin.Value;
						}
						if (newValues.groupMax.HasValue)
						{
							//logMessage += $" Group Max {spawner.m_groupSizeMax} → {newValues.groupMax.Value},";
							spawner.m_groupSizeMax = newValues.groupMax.Value;
						}
						if (newValues.spawnChance.HasValue)
						{
							//logMessage += $" Chance {spawner.m_spawnChance}% → {newValues.spawnChance.Value}%,";
							spawner.m_spawnChance = newValues.spawnChance.Value;
						}

						// Remove trailing comma and log the update
						//MarsarahTweaks.MLog(logMessage.TrimEnd(','));
					}
				}
			}
		}
	}
}
