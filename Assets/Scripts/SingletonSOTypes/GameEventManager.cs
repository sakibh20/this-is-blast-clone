using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "SingletonSOs/GameEventManager", fileName = "GameEventManager")]
public class GameEventManager : ContextualSingletonSO<GameEventManager>
{
    private Dictionary<string, GameEvent> eventDictionary;
    private Dictionary<string, HashSet<int>> activeListeners;
    private Dictionary<string, float> exclusiveDelayedEventDictionary;

	protected override void InitializeAtRuntime()
	{
		if (!Application.isPlaying) return;
		if (IsInitialized) return;
        
        base.InitializeAtRuntime();

        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<string, GameEvent>();
            activeListeners = new Dictionary<string, HashSet<int>>();
            exclusiveDelayedEventDictionary = new Dictionary<string, float>();
        }
    }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnBeforeSceneLoad()
	{
        if (!IsInUse) return;
        if (Instance.singletonContext == null) return;

        Instance.singletonContext.StopAllCoroutines();
		Instance.singletonContext.CancelInvoke();
    }

    public static void AddListener(string eventName, UnityAction<GameEventModel> listener)
    {
        if (Instance == null) return;

        if (Instance.eventDictionary.TryGetValue(eventName, out GameEvent thisEvent))
        {
            if (Instance.activeListeners[eventName].Contains(listener.GetHashCode())) return;

            thisEvent.AddListener(listener);
            Instance.activeListeners[eventName].Add(listener.GetHashCode());
        }
        else
        {
            thisEvent = new GameEvent();
            thisEvent.AddListener(listener);
            Instance.eventDictionary.Add(eventName, thisEvent);
            Instance.activeListeners[eventName] = new HashSet<int>() { listener.GetHashCode() };
        }
    }

    public static void RemoveListener(string eventName, UnityAction<GameEventModel> listener)
    {
        if (Instance == null) return;

        if (Instance.eventDictionary.TryGetValue(eventName, out GameEvent thisEvent))
        {
            thisEvent.RemoveListener(listener);

            if (Instance.activeListeners[eventName].Contains(listener.GetHashCode()))
            {
                Instance.activeListeners[eventName].Remove(listener.GetHashCode());
            }

            if (!HasAnyActiveListeners(eventName))
            {
                Instance.eventDictionary.Remove(eventName);
                Instance.activeListeners.Remove(eventName);
            }
        }
    }

    public static void RemoveAllListeners(string eventName)
    {
        if (Instance == null) return;

        if (Instance.eventDictionary.TryGetValue(eventName, out GameEvent thisEvent))
        {
            thisEvent.RemoveAllListeners();
            Instance.eventDictionary.Remove(eventName);

            if (Instance.activeListeners.ContainsKey(eventName))
            {
                Instance.activeListeners[eventName].Clear();
                Instance.activeListeners.Remove(eventName);
            }
        }
    }

    public static void DispatchEvent(string eventName, GameEventModel data = null)
    {
        if (Instance == null) return;

        if (Instance.eventDictionary.TryGetValue(eventName, out GameEvent thisEvent))
        {
            thisEvent.Invoke(data ?? new GameEventModel());
        }
    }

    public static bool HasAnyActiveListeners(string eventName)
    {
        if (Instance == null) return false;

        return (Instance.eventDictionary.TryGetValue(eventName, out GameEvent thisEvent) &&
                Instance.activeListeners.ContainsKey(eventName) &&
                Instance.activeListeners[eventName].Count > 0);
    }

    public static int GetNumActiveListeners(string eventName)
    {
        if (Instance == null) return 0;

        return (Instance.eventDictionary.TryGetValue(eventName, out GameEvent thisEvent) &&
                Instance.activeListeners.ContainsKey(eventName) &&
                Instance.activeListeners[eventName].Count > 0) ? Instance.activeListeners[eventName].Count : 0;
    }

    public static void DispatchDelayedEvent(string eventName, float time, GameEventModel data = null, bool isExclusive = false)
    {
        if (Instance == null) return;

        if (Instance.exclusiveDelayedEventDictionary.ContainsKey(eventName)) return;

        if (isExclusive) Instance.exclusiveDelayedEventDictionary.Add(eventName, time);
        Instance.singletonContext.StartCoroutine(GameEventManager.DelayedEventDispatcher(eventName, time, (data != null ? data : new GameEventModel())));
    }

    private static IEnumerator DelayedEventDispatcher(string eventName, float time, GameEventModel data)
    {
        yield return new WaitForSeconds(time);

        if (Instance.exclusiveDelayedEventDictionary.ContainsKey(eventName)) Instance.exclusiveDelayedEventDictionary.Remove(eventName);

        if (Instance.eventDictionary.TryGetValue(eventName, out GameEvent thisEvent))
        {
            thisEvent.Invoke(data);
        }
    }
}
