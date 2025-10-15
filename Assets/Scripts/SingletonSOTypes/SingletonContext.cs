using System.Collections;
using UnityEngine;

public class SingletonContext : MonoBehaviour
{
	public delegate void OnUpdateStateMachine();
	public OnUpdateStateMachine UpdateStateMachine { get; set; } = () => { };

	private bool stopAllCoroutines;
	private IEnumerator exclusiveCoroutine;

	private void Awake()
	{
		stopAllCoroutines = false;
		exclusiveCoroutine = null;
	}

	void Update()
	{
		if (stopAllCoroutines)
		{
			StopAllCoroutines();
			stopAllCoroutines = false;
		}

		if (exclusiveCoroutine != null)
		{
			StartCoroutine(exclusiveCoroutine);
			exclusiveCoroutine = null;
		}

		UpdateStateMachine();
	}

	public void SignalStopAllCoroutines()
	{
		stopAllCoroutines = true;
	}

	public void ExclusiveStartCoroutine(IEnumerator enumerator)
	{
		SignalStopAllCoroutines();
		exclusiveCoroutine = enumerator;
	}
}
