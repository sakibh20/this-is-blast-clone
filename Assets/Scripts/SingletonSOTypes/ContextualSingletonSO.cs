using UnityEngine;

public abstract class ContextualSingletonSO<T> : SingletonSO<T> where T : ScriptableObject
{
	[SerializeField]
	protected GameObject singletonContextPrefab;

	[SerializeField]
	protected bool dontDestroySingletonContextOnLoad = true;

	protected SingletonContext singletonContext;

	protected override void InitializeAtRuntime()
	{
		if (!Application.isPlaying) return;

		base.InitializeAtRuntime();

		if (singletonContext == null)
		{
			GameObject go = (singletonContextPrefab != null) ?
				Instantiate(singletonContextPrefab) :
				new GameObject("NewGO", typeof(SingletonContext));

			go.name = typeof(T).Name + "Context";
			singletonContext = go.GetComponent<SingletonContext>();

			if (dontDestroySingletonContextOnLoad) DontDestroyOnLoad(go);
		}
	}
}
