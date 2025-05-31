using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Features
{
	internal class AshlandsEnemiesChanges
	{
		[HarmonyPatch(typeof(SpawnSystem), "Awake")]
		class LessAshlandsEnemiesAwake_Patch
		{
			private static void Postfix(SpawnSystem __instance)
			{
				// Run on both server and client - no checks made

				if (__instance == null) return;

				UpdateAshlandsSpawns(__instance);
			}
		}

		// Dictionaries
		private static readonly Dictionary<string, (int maxSpawned, int groupMin, int groupMax, float spawnChance)> originalSpawnData = new Dictionary<string, (int maxSpawned, int groupMin, int groupMax, float spawnChance)>();
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
			{ "Lava Blob",					(1, null, 1, 20f) }			// 2, 1, 2, 25
		};

		public static void UpdateAshlandsSpawns(SpawnSystem spawnSystem)
		{
			foreach (SpawnSystemList spawnList in spawnSystem.m_spawnLists)
			{
				// Super cool log
				//MarsarahTweaks.LogInfo($"| {"Enemy",-25} | {"MAX",-3} | {"Group Min",-9} | {"Group Max",-9} | {"Chance",-6} |");
				//MarsarahTweaks.LogInfo(new string('-', 60)); // Separator line

				foreach (SpawnSystem.SpawnData spawner in spawnList.m_spawners)
				{
					if (spawnAdjustments.TryGetValue(spawner.m_name, out var newValues))
					{
						//MarsarahTweaks.LogInfo($"| {spawner.m_name,-25} | {spawner.m_maxSpawned,-3} | {spawner.m_groupSizeMin,-9} | {spawner.m_groupSizeMax,-9} | {spawner.m_spawnChance,-6}% |");

						if (ConfigManager.LessAshlandsEnemiesEnabled.Value)
						{
							// Backup
							if (!originalSpawnData.ContainsKey(spawner.m_name))
							{
								//MarsarahTweaks.LogInfo($"Backing up: | {spawner.m_name,-25} | {spawner.m_maxSpawned,-3} | {spawner.m_groupSizeMin,-9} | {spawner.m_groupSizeMax,-9} | {spawner.m_spawnChance,-6}% |");
								originalSpawnData[spawner.m_name] = (spawner.m_maxSpawned, spawner.m_groupSizeMin, spawner.m_groupSizeMax, spawner.m_spawnChance);
							}

							//string updateLogMessage = $"Updating {spawner.m_name}:";

							// Apply the new values
							if (newValues.maxSpawned.HasValue && spawner.m_maxSpawned != newValues.maxSpawned.Value)
							{
								//updateLogMessage += $" Max Spawned {spawner.m_maxSpawned} → {newValues.maxSpawned.Value},";
								spawner.m_maxSpawned = newValues.maxSpawned.Value;
							}
							if (newValues.groupMin.HasValue && spawner.m_groupSizeMin != newValues.groupMin.Value)
							{
								//updateLogMessage += $" Group Min {spawner.m_groupSizeMin} → {newValues.groupMin.Value},";
								spawner.m_groupSizeMin = newValues.groupMin.Value;
							}
							if (newValues.groupMax.HasValue && spawner.m_groupSizeMax != newValues.groupMax.Value)
							{
								//updateLogMessage += $" Group Max {spawner.m_groupSizeMax} → {newValues.groupMax.Value},";
								spawner.m_groupSizeMax = newValues.groupMax.Value;
							}
							if (newValues.spawnChance.HasValue && spawner.m_spawnChance != newValues.spawnChance.Value)
							{
								//updateLogMessage += $" Chance {spawner.m_spawnChance}% → {newValues.spawnChance.Value}%,";
								spawner.m_spawnChance = newValues.spawnChance.Value;
							}

							//MarsarahTweaks.LogInfo($"Updated: | {spawner.m_name,-25} | {spawner.m_maxSpawned,-3} | {spawner.m_groupSizeMin,-9} | {spawner.m_groupSizeMax,-9} | {spawner.m_spawnChance,-6}% |");

							// Remove trailing comma and log the update
							//MarsarahTweaks.LogInfo(updateLogMessage.TrimEnd(','));
						}
						else if (originalSpawnData.TryGetValue(spawner.m_name, out var originalSpawn))
						{
							// Restore
							spawner.m_maxSpawned = originalSpawn.maxSpawned;
							spawner.m_groupSizeMin = originalSpawn.groupMin;
							spawner.m_groupSizeMax = originalSpawn.groupMax;
							spawner.m_spawnChance = originalSpawn.spawnChance;

							//MarsarahTweaks.LogInfo($"Restored: | {spawner.m_name,-25} | {spawner.m_maxSpawned,-3} | {spawner.m_groupSizeMin,-9} | {spawner.m_groupSizeMax,-9} | {spawner.m_spawnChance,-6}% |");

							originalSpawnData.Remove(spawner.m_name);
						}
					}
				}
			}
		}
	}
}
