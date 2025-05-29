using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches.UI
{
	internal static class PlayerLogoutAnnounce
	{
		private static HashSet<string> _previousPeers = new HashSet<string>();
		private const string LogoutRPC = "MarsarahTweaks_LogoutAnnounce";
		private static bool _registered = false;

		[HarmonyPatch(typeof(ZNet), "Update")]
		public static class ZNet_Update_Patch
		{
			public static void Postfix()
			{
				if (!ZNet.instance || !ZNet.instance.IsServer()) return; // Only run on server
				if (!ConfigManager.AnnouncePlayerLogout.Value) return;

				List<ZNetPeer> connectedPeers = ZNet.instance.GetConnectedPeers();
				HashSet<string> currentPeers = new HashSet<string>();

				foreach (var peer in connectedPeers)
				{
					if (!string.IsNullOrEmpty(peer.m_playerName))
					{
						currentPeers.Add(peer.m_playerName);
					}
				}

				foreach (var prevName in _previousPeers)
				{
					if (!currentPeers.Contains(prevName))
					{
						//MarsarahTweaks.LogInfo($"{prevName} logged out.");
						// Send to all clients
						ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, LogoutRPC, prevName);
					}
				}

				_previousPeers = currentPeers;
			}
		}

		[HarmonyPatch(typeof(ZNet), "Awake")]
		public static class ZNet_Awake_LogoutAnnounce_Patch
		{
			public static void Postfix()
			{
				PlayerLogoutAnnounce.TryRegisterRPC();
			}
		}

		public static void TryRegisterRPC()
		{
			if (_registered || ZRoutedRpc.instance == null) return;

			ZRoutedRpc.instance.Register<string>(LogoutRPC, OnPlayerLogoutRPC);
			_registered = true;
		}

		private static void OnPlayerLogoutRPC(long sender, string playerName)
		{
			if (!ConfigManager.AnnouncePlayerLogout.Value) return;

			MessageHud.instance?.ShowMessage(MessageHud.MessageType.TopLeft, $"{playerName} logged out.");
			Chat.instance?.AddString(title: "[Server]", text: $"{playerName} logged out.", type: Talker.Type.Shout);

			/*if (Chat.instance != null)
			{
				var chatFocusedField = typeof(Chat).GetField("m_focused", BindingFlags.NonPublic | BindingFlags.Instance);
				if (chatFocusedField != null && Chat.instance != null)
				{
					chatFocusedField.SetValue(Chat.instance, true);
				}

				//Chat.instance.m_chatWindow.SetActive(true);  // Activate the chat window GameObject
				//Chat.instance.m_input.ActivateInputField();  // Focus the chat input field so it's ready for typing
			}*/
		}
	}
}
