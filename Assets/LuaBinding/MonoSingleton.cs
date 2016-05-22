using UnityEngine;

/// <summary>
/// 需要使用Unity生命周期的单例模式
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
	
	protected static T g_instance = null;

	public static T getInstance ()
	{
		if (g_instance == null) {
			T[] instances = FindObjectsOfType<T> ();
			if (instances.Length == 1) {
				g_instance = instances [0];
			} else if (instances.Length > 1) {
				Debug.LogError (typeof(T).Name + " more than one!");
				g_instance = instances [0];
			}

			if (g_instance == null) {
				string instanceName = "_" + typeof(T).Name;
				GameObject instanceGO = GameObject.Find (instanceName);

				if (instanceGO == null) {
					instanceGO = new GameObject (instanceName);
					instanceGO.hideFlags = HideFlags.DontSave;
				}
				g_instance = instanceGO.AddComponent<T> ();
				DontDestroyOnLoad (instanceGO);
				Debug.Log ("Add New Singleton " + g_instance.name + " in Game!");
			}
		}

		return g_instance;
	}


	protected virtual void OnDestroy ()
	{
		g_instance = null;
	}

}
