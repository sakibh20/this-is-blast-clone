using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveDuration = 0.75f;
    [SerializeField] private Ease ease;
    
    private Tween _moveTween;
    private Player _player;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }
    
    public void MoveToShootSpot(Transform target)
    {
        _player.CurrentState = PlayerState.Moving;

        _moveTween?.Kill();
        _moveTween = transform.DOMove(target.position, moveDuration)
            .SetEase(ease)
            .OnComplete(OnReachedShootSpot);
    }
    
    private void OnReachedShootSpot()
    {
        Debug.Log("OnReachedShootSpot!");
        _player.CurrentState = PlayerState.ReadyToShoot;
    }
}
