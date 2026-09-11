using HarmonyLib;
using MarsarahTweaks.Managers;
using System.Collections.Generic;

namespace MarsarahTweaks.Patches.QOL
{
	internal static class PlayerLogoutAnnounce
	{
		private static readonly LogManager log = new LogManager("Player Logout Announce", LogManager.LogLevel.Warning);

		private const string LogoutRPC = "MarsarahTweaks_LogoutAnnounce";

		private static HashSet<string> previousPeers = new HashSet<string>();
		private static ZRoutedRpc registeredRpc;

		[HarmonyPatch(typeof(ZNet), "Update")]
		private static class ZNetUpdatePatch
		{
			private static void Postfix()
			{
				if (ZNet.instance == null || !ZNet.instance.IsServer()) return;

				List<ZNetPeer> connectedPeers = ZNet.instance.GetConnectedPeers();
				HashSet<string> currentPeers = new HashSet<string>();

				foreach (ZNetPeer peer in connectedPeers)
				{
					if (!string.IsNullOrEmpty(peer.m_playerName))
					{
						currentPeers.Add(peer.m_playerName);
					}
				}

				if (ConfigManager.AnnouncePlayerLogout.Value)
				{
					foreach (string previousName in previousPeers)
					{
						if (!currentPeers.Contains(previousName))
						{
							log.Info($"{previousName} logged out.");

							ZRoutedRpc.instance?.InvokeRoutedRPC(ZRoutedRpc.Everybody, LogoutRPC, previousName);
						}
					}
				}

				previousPeers = currentPeers;
			}
		}

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		private static class ZNetSceneAwakePatch
		{
			private static void Postfix()
			{
				TryRegisterRPC();
			}
		}

		private static void TryRegisterRPC()
		{
			if (ZRoutedRpc.instance == null || ReferenceEquals(registeredRpc, ZRoutedRpc.instance)) return;

			ZRoutedRpc.instance.Register<string>(LogoutRPC, OnPlayerLogoutRPC);

			registeredRpc = ZRoutedRpc.instance;
			previousPeers.Clear();

			log.Info("Registered Player Logout Announce RPC.");
		}

		private static void OnPlayerLogoutRPC(long sender, string playerName)
		{
			if (!ConfigManager.AnnouncePlayerLogout.Value) return;

			MessageHud.instance?.ShowMessage(MessageHud.MessageType.TopLeft, $"{playerName} logged out.");
			Chat.instance?.AddString("[Server]", $"{playerName} logged out.", Talker.Type.Shout);
		}
	}
}