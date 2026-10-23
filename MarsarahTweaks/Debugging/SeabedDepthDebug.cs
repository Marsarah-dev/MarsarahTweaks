using MarsarahTweaks.Managers;
using MarsarahTweaks.Patches;
using System.Collections.Generic;
using UnityEngine;

namespace MarsarahTweaks.Debugging
{
	// Temporary client-side diagnostic overlay. Do not ship as a permanent Tweaks UI feature.
	internal class SeabedDepthDebug : MonoBehaviour
	{
		private static readonly LogManager log = new LogManager("Seabed Debug", LogManager.LogLevel.Info);

		private const float RefreshInterval = 0.2f;
		private const float SpawnDataRefreshInterval = 3f;
		private const string SmallSerpentPrefabName = "MSmallSerpent";

		private float nextRefresh;
		private float nextSpawnDataRefresh;
		private float seabedAltitude;
		private Heightmap.Biome currentBiome;
		private Heightmap.BiomeArea? currentBiomeArea;
		private string currentWeather = "Unknown";
		private bool isNight;
		private bool hasReading;
		private bool visible = true;

		private ZNetScene cachedScene;
		private SpawnSystem.SpawnData smallSerpentSpawner;
		private SpawnSystem.SpawnData nightSerpentSpawner;
		private SpawnSystem.SpawnData stormSerpentSpawner;

		private void Awake()
		{
			log.Info("Serpent spawn debug overlay enabled. Press F8 to show/hide.");
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F8))
			{
				visible = !visible;
				log.Info($"Serpent spawn debug overlay {(visible ? "shown" : "hidden")}.");
			}

			if (!visible || Time.unscaledTime < nextRefresh) return;
			nextRefresh = Time.unscaledTime + RefreshInterval;

			Player player = Player.m_localPlayer;
			if (player == null || ZoneSystem.instance == null || EnvMan.instance == null)
			{
				hasReading = false;
				return;
			}

			Vector3 groundPosition = player.transform.position;
			ZoneSystem.instance.GetGroundData(ref groundPosition, out Vector3 groundNormal, out Heightmap.Biome biome, out Heightmap.BiomeArea biomeArea, out Heightmap heightmap);

			if (heightmap == null)
			{
				hasReading = false;
				return;
			}

			seabedAltitude = groundPosition.y - 30f;
			currentBiome = biome;
			currentBiomeArea = biomeArea;
			isNight = EnvMan.IsNight();
			EnvSetup environment = EnvMan.instance.GetCurrentEnvironment();
			currentWeather = environment != null ? environment.m_name : "Unknown";
			hasReading = true;

			if (cachedScene != ZNetScene.instance)
			{
				cachedScene = ZNetScene.instance;
				smallSerpentSpawner = null;
				nightSerpentSpawner = null;
				stormSerpentSpawner = null;
				nextSpawnDataRefresh = 0f;
			}

			if (Time.unscaledTime >= nextSpawnDataRefresh)
			{
				nextSpawnDataRefresh = Time.unscaledTime + SpawnDataRefreshInterval;
				RefreshSpawnerData();
			}
		}

		private void RefreshSpawnerData()
		{
			smallSerpentSpawner = null;
			nightSerpentSpawner = null;
			stormSerpentSpawner = null;

			foreach (SpawnSystem spawnSystem in UnityEngine.Object.FindObjectsByType<SpawnSystem>(FindObjectsSortMode.None))
			{
				if (spawnSystem == null || spawnSystem.m_spawnLists == null) continue;

				foreach (SpawnSystemList spawnList in spawnSystem.m_spawnLists)
				{
					if (spawnList == null || spawnList.m_spawners == null) continue;

					foreach (SpawnSystem.SpawnData spawner in spawnList.m_spawners)
					{
						if (spawner == null || spawner.m_prefab == null) continue;

						if (spawner.m_prefab.name == SmallSerpentPrefabName)
						{
							smallSerpentSpawner = spawner;
						}
						else if (spawner.m_prefab.name == "Serpent" && spawner.m_biome == Heightmap.Biome.Ocean)
						{
							if (!spawner.m_spawnAtDay && spawner.m_spawnAtNight &&
								(spawner.m_requiredEnvironments == null || spawner.m_requiredEnvironments.Count == 0))
							{
								nightSerpentSpawner = spawner;
							}
							else if (spawner.m_spawnAtDay && spawner.m_spawnAtNight &&
								spawner.m_requiredEnvironments != null &&
								spawner.m_requiredEnvironments.Contains("Rain") &&
								spawner.m_requiredEnvironments.Contains("ThunderStorm"))
							{
								stormSerpentSpawner = spawner;
							}
						}

						if (smallSerpentSpawner != null && nightSerpentSpawner != null && stormSerpentSpawner != null) return;
					}
				}
			}
		}

		private string GetSpawnerStatus(SpawnSystem.SpawnData spawner)
		{
			if (spawner == null) return "Not found (spawn data not loaded)";

			List<string> reasons = new List<string>();
			if (!spawner.m_enabled || spawner.m_spawnChance <= 0f) reasons.Add("disabled");
			if (spawner.m_requiredGlobalKey == "defeated_bonemass" && !GlobalKeyChecker.BonemassDefeated) reasons.Add("Bonemass key");
			if ((spawner.m_biome & currentBiome) == 0) reasons.Add("biome");
			if (currentBiomeArea.HasValue && (spawner.m_biomeArea & currentBiomeArea.Value) == 0) reasons.Add("biome area");
			if (seabedAltitude < spawner.m_minAltitude || seabedAltitude > spawner.m_maxAltitude) reasons.Add("depth");
			if (isNight && !spawner.m_spawnAtNight) reasons.Add("night");
			if (!isNight && !spawner.m_spawnAtDay) reasons.Add("day");
			if (spawner.m_requiredEnvironments != null && spawner.m_requiredEnvironments.Count > 0 &&
				!spawner.m_requiredEnvironments.Contains(currentWeather)) reasons.Add("weather");

			if (reasons.Count > 0) return $"NO ({string.Join(", ", reasons)})";
			return $"YES ({spawner.m_spawnChance:0.#}% / {spawner.m_spawnInterval:0}s)";
		}

		private void OnGUI()
		{
			if (!visible || !hasReading) return;

			float waterDepth = Mathf.Max(0f, -seabedAltitude);
			string biomeArea = currentBiomeArea.HasValue ? currentBiomeArea.Value.ToString() : "Unknown";
			string display = $"Seabed altitude: {seabedAltitude:F1} m\n" +
				$"Water depth: {waterDepth:F1} m\n" +
				$"Biome: {currentBiome} ({biomeArea})\n" +
				$"Time: {(isNight ? "Night" : "Day")} | Weather: {currentWeather}\n" +
				$"Small Serpent: {GetSpawnerStatus(smallSerpentSpawner)}\n" +
				$"Night Serpent: {GetSpawnerStatus(nightSerpentSpawner)}\n" +
				$"Rain/Storm Serpent: {GetSpawnerStatus(stormSerpentSpawner)}";

			float boxWidth = Mathf.Min(550f, Screen.width - 32f);
			GUI.Box(new Rect(16f, 160f, boxWidth, 176f), display);
		}
	}
}
