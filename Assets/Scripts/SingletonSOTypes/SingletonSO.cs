using System;
using UnityEngine;

[Serializable]
public abstract class SingletonSO<T> : ScriptableObject where T : ScriptableObject
{
	private static T instance = null;

	public static T Instance
	{
		get
		{
			if (IsInUse)
			{
				return instance;
			}

			string typename = typeof(T).Name;
			instance = Resources.Load<T>("SingletonSOs/" + typename);
			if (instance == null)
			{
				Debug.LogError("Error in attempt to get SingletonSO.Instance of type " + typename);
				return null;
			}

			(instance as SingletonSO<T>).InitializeAtRuntime();

			return instance;
		}
	}
#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void AfterAssembliesLoaded()
	{
        if (!Application.isPlaying) return;
	}
#endif

	protected virtual void Quit()
	{
		IsInitialized = false;
		instance = null;
	}

	public static bool IsInUse
	{
		get
		{
			return (instance != null);
		}
	}

	protected static bool IsInitialized { get; set; } = false;

	protected virtual void InitializeAtRuntime()
	{
		IsInitialized = true;
	}
}
