using UnityEngine.Events;

public class GameEventModel : System.Collections.Generic.Dictionary<string, object>
{
}

[System.Serializable]
public class GameEvent : UnityEvent<GameEventModel>
{
}
