//using HarmonyLib;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;
//using static HarmonyLib.AccessTools;
//using UnityObject = UnityEngine.Object;
//using MarsarahTweaks.Managers;


//namespace MarsarahTweaks.Patches.UI
//{
//	internal class UIEnemyNameplates
//	{
//		private static readonly FieldInfo _mHudsField = typeof(EnemyHud).GetField("m_huds", BindingFlags.Instance | BindingFlags.NonPublic);
//		private static readonly FieldInfo _mNameField;
//		private static readonly FieldInfo _mCharacterField;

//		private static readonly Dictionary<object, TextMeshProUGUI> _hpTextCache = new Dictionary<object, TextMeshProUGUI>();

//		static UIEnemyNameplates()
//		{
//			// Get the private nested HudData type
//			var hudDataType = typeof(EnemyHud).GetNestedType("HudData", BindingFlags.NonPublic);
//			_mNameField = hudDataType?.GetField("m_name", BindingFlags.Instance | BindingFlags.NonPublic);
//			_mCharacterField = hudDataType?.GetField("m_character", BindingFlags.Instance | BindingFlags.NonPublic);
//		}

//		private static bool TryGetHudData(EnemyHud instance, Character c, out object hudData)
//		{
//			hudData = null;

//			var huds = _mHudsField?.GetValue(instance) as IDictionary;
//			if (huds == null) return false;

//			if (!huds.Contains(c))
//				return false;

//			hudData = huds[c];
//			return hudData != null;
//		}

//		[HarmonyPatch(typeof(EnemyHud), "ShowHud")]
//		public static class EnemyHud_ShowHud_Patch
//		{
//			public static void Postfix(EnemyHud __instance, Character c)
//			{
//				if (!TryGetHudData(__instance, c, out var hudData)) return;
//				if (_hpTextCache.ContainsKey(hudData)) return;

//				var nameField = _mNameField?.GetValue(hudData) as TextMeshProUGUI;
//				if (nameField == null) return;

//				var hpText = UnityObject.Instantiate(nameField, nameField.transform.parent);
//				hpText.name = "MT_enemyHpText";
//				hpText.rectTransform.anchoredPosition = new Vector2(hpText.rectTransform.anchoredPosition.x, 7f);
//				hpText.fontSize = 15;
//				hpText.color = Color.white;

//				if (_mCharacterField?.GetValue(hudData) is Character character)
//				{
//					hpText.text = $"{character.GetHealth():0}/{character.GetMaxHealth():0}";
//				}

//				UnityObject.Destroy(hpText.GetComponent<Outline>());
//				_hpTextCache[hudData] = hpText;
//			}
//		}


//		[HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
//		public static class EnemyHud_UpdateHud_Patch
//		{
//			[HarmonyPostfix]
//			public static void Postfix(EnemyHud __instance)
//			{
//				var m_hudsRaw = _mHudsField?.GetValue(__instance);
//				if (!(m_hudsRaw is IDictionary m_huds)) return;

//				foreach (DictionaryEntry entry in m_huds)
//				{
//					var character = entry.Key as Character;
//					var hudData = entry.Value;

//					if (character == null || !_hpTextCache.TryGetValue(hudData, out var hpText)) continue;

//					var c = _mCharacterField?.GetValue(hudData) as Character;
//					if (c == null) continue;

//					hpText.text = $"{c.GetHealth():0}/{c.GetMaxHealth():0}";
//				}
//			}
//		}
//	}
//}
