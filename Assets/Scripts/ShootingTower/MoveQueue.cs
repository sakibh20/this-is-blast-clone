using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveQueue : MonoBehaviour
{
    [SerializeField] private List<PlayerQueue> allQueues = new List<PlayerQueue>();
    [SerializeField] private DocManager docManager; 
    
    public void OnPlayerClicked(PlayerShooter clickedPlayer)
    {
        PlayerQueue queue = GetPlayerQueue(clickedPlayer);
        if (queue == null)
        {
            Debug.LogWarning("Clicked player not found in any queue!");
            return;
        }

        if (!queue.IsFrontPlayer(clickedPlayer))
        {
            Debug.Log("Clicked player is NOT at the front of its queue.");
            return;
        }

        Doc targetDoc = docManager.GetTargetDoc();
        if (targetDoc == null)
        {
            Debug.LogWarning("No available Doc position found!");
            return;
        }

        targetDoc.ReserveDoc(clickedPlayer.Player);
        
        clickedPlayer.Player.SetTargetDoc(targetDoc); 
        
        clickedPlayer.Player.PlayerMovement.MoveToShootSpot(targetDoc.DocTransform);

        queue.playerQueue.Remove(clickedPlayer);

        MoveQueueForward(queue, clickedPlayer);
    }
    
    private void MoveQueueForward(PlayerQueue queue, PlayerShooter clickedPlayer)
    {
        if (queue.playerQueue.Count == 0)
            return;

        for (int i = 0; i < queue.playerQueue.Count; i++)
        {
            PlayerShooter player = queue.playerQueue[i];

            Vector3 newTargetPos;

            if (i == 0)
            {
                newTargetPos = clickedPlayer.transform.position;
            }
            else
            {
                newTargetPos = queue.playerQueue[i - 1].transform.position;
            }

            player.Player.PlayerMovement.MoveForward(newTargetPos);
            if (i == 0)
            {
                player.Player.CurrentState = PlayerState.ReadyToMove;
            }
        }
    }

    private PlayerQueue GetPlayerQueue(PlayerShooter player)
    {
        foreach (var queue in allQueues)
        {
            if (queue.playerQueue.Contains(player))
                return queue;
        }
        return null;
    }
}

[Serializable]
public class PlayerQueue
{
    public List<PlayerShooter> playerQueue = new List<PlayerShooter>();
    
    public bool IsFrontPlayer(PlayerShooter player)
    {
        return playerQueue.Count > 0 && playerQueue[0] == player;
    }
}