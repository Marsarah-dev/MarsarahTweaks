//using System.Collections;
//using UnityEngine;

//namespace MarsarahTweaks
//{
//	public class ComponentLogger : MonoBehaviour
//	{
//		public void StartLogging(GameObject go)
//		{
//			StartCoroutine(LogComponentsLater(go));
//		}

//		private IEnumerator LogComponentsLater(GameObject go)
//		{
//			yield return new WaitForSeconds(0.5f); // Give time for initialization
//			log.Info($"[Delayed Check] {go.name} Components:");

//			foreach (Component comp in go.GetComponents<Component>())
//			{
//				log.Info($"{go.name} - {comp}");
//			}

//			Destroy(this); // Cleanup after logging
//		}

//		private void Start()
//		{
//			StartCoroutine(LogComponents());
//		}

//		private IEnumerator LogComponents()
//		{
//			yield return new WaitForSeconds(1f); // Give time for initialization
//			foreach (var comp in GetComponents<Component>())
//			{
//				log.Info($"[{gameObject.name}] - {comp.GetType().Name}");
//			}
//		}
//	}
//}
