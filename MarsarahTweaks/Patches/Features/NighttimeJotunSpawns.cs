using HarmonyLib;
using MarsarahTweaks.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace MarsarahTweaks.Patches.Features
{
	internal class NighttimeJotunSpawns
	{
		private static readonly LogManager log = new LogManager("Nighttime Jotun Spawns", LogManager.LogLevel.Info);

		private const float NighttimeSpawnChance = 7f;
		private const float NighttimeSpawnInterval = 2700f;
		private const string NighttimeSuffix = " [MarsarahTweaks NIGHT]";

		private static readonly Dictionary<string, string> jotunPatrols = new Dictionary<string, string>()
		{
			{ "Jotun Melee Patrol", "JotunWarrior" },
			{ "Jotun Witch Patrol", "JotunWitch" }
		};

		[HarmonyPatch(typeof(SpawnSystem), "Awake")]
		class NighttimeJotunSpawns_Patch
		{
			private static void Postfix(SpawnSystem __instance)
			{
				UpdateNighttimeJotunSpawns(__instance);
			}
		}

		public static void UpdateNighttimeJotunSpawns(SpawnSystem spawnSystem)
		{
			if (spawnSystem == null) return;

			bool enabled = ConfigManager.NighttimeJotunSpawnsEnabled.Value;

			foreach (SpawnSystemList spawnList in spawnSystem.m_spawnLists)
			{
				if (spawnList == null) continue;

				foreach (var patrol in jotunPatrols)
				{
					SpawnSystem.SpawnData daySpawner = spawnList.m_spawners.Find(spawner =>
						spawner.m_name == patrol.Key &&
						spawner.m_prefab != null &&
						spawner.m_prefab.name == patrol.Value &&
						spawner.m_biome == Heightmap.Biome.DeepNorth &&
						spawner.m_requiredGlobalKey == "jotun_killed");

					if (daySpawner == null) continue;

					string nightSpawnerName = daySpawner.m_name + NighttimeSuffix;
					SpawnSystem.SpawnData nightSpawner = spawnList.m_spawners.Find(spawner => spawner.m_name == nightSpawnerName);

					// Do not duplicate spawners that already support nighttime spawning
					if (!daySpawner.m_spawnAtDay || daySpawner.m_spawnAtNight)
					{
						if (nightSpawner != null)
							nightSpawner.m_enabled = false;

						continue;
					}

					bool shouldEnable = enabled && daySpawner.m_enabled;

					// Register the nighttime spawner if it does not already exist
					if (nightSpawner == null)
					{
						nightSpawner = daySpawner.Clone();

						nightSpawner.m_name = nightSpawnerName;
						nightSpawner.m_spawnAtDay = false;
						nightSpawner.m_spawnAtNight = true;
						nightSpawner.m_spawnChance = NighttimeSpawnChance;
						nightSpawner.m_spawnInterval = NighttimeSpawnInterval;
						nightSpawner.m_enabled = shouldEnable;

						spawnList.m_spawners.Add(nightSpawner);

						log.Info($"Registered: {nightSpawner.m_name}", true);
						log.Info($"Enabled: {nightSpawner.m_enabled}");
						log.Info($"Prefab: {nightSpawner.m_prefab.name}");
						log.Info($"Biome: {nightSpawner.m_biome}");
						log.Info($"Required Key: {nightSpawner.m_requiredGlobalKey}");
						log.Info($"Spawn At Day: {daySpawner.m_spawnAtDay} -> {nightSpawner.m_spawnAtDay}");
						log.Info($"Spawn At Night: {daySpawner.m_spawnAtNight} -> {nightSpawner.m_spawnAtNight}");
						log.Info($"Spawn Chance: {daySpawner.m_spawnChance}% -> {nightSpawner.m_spawnChance}%");
						log.Info($"Spawn Interval: {daySpawner.m_spawnInterval}s -> {nightSpawner.m_spawnInterval}s");
						log.Info($"Max Spawned: {nightSpawner.m_maxSpawned}");
						log.Info($"Group Size: {nightSpawner.m_groupSizeMin}-{nightSpawner.m_groupSizeMax}");
						log.Info($"Max Level: {nightSpawner.m_maxLevel}");
						log.Info($"Hunt Player: {nightSpawner.m_huntPlayer}", false, true);
					}

					// Handle mid-game configuration changes
					else if (nightSpawner.m_enabled != shouldEnable)
					{
						nightSpawner.m_enabled = shouldEnable;

						log.Info($"{nightSpawner.m_name} | Enabled: {nightSpawner.m_enabled}");
					}
				}
			}
		}

		// Debug logging for actual nighttime Jotun spawns
		[HarmonyPatch(typeof(SpawnSystem), "Spawn")]
		class NighttimeJotunSpawnLog_Patch
		{
			private static void Postfix(SpawnSystem.SpawnData critter, Vector3 spawnPoint, bool eventSpawner)
			{
				if (eventSpawner || SpawnSystem.m_nospawn) return;
				if (!critter.m_name.EndsWith(NighttimeSuffix)) return;

				log.Info($"Spawned: {critter.m_prefab.name} | Spawner: {critter.m_name} | Position: {spawnPoint} | Night: {EnvMan.IsNight()}");
			}
		}
	}
}