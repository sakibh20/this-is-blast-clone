using DG.Tweening;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private Transform leftTarget;

    [SerializeField] private Transform rightTarget;
    [SerializeField] private float rotationDuration = 0.3f;
    [SerializeField] private float jumpPower = 1f;
    [SerializeField] private int numJumps = 3;
    [SerializeField] private float moveOutDuration = 0.75f;
    [SerializeField] private float wobbleStrength = 0.2f;

    [Header("Settings")] [SerializeField] private float moveDuration = 0.75f;

    private Tween _moveTween;
    private Player _player;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    public void MoveToShootSpot(Transform target)
    {
        SoundManager.Instance.PlayShooterPopSound();
        _player.CurrentState = PlayerState.Moving;

        _moveTween?.Kill();
        // _moveTween = transform.DOMove(target.position, moveDuration)
        //     .SetEase(ease)
        //     .OnComplete(OnReachedShootSpot);        

        _moveTween = transform.DOJump(
                target.position,
                0.3f,
                1,
                moveDuration
            )
            .SetEase(Ease.OutQuad)
            .OnComplete(OnReachedShootSpot);
    }
    
    public void MoveForward(Vector3 targetPos)
    {
        //SoundManager.Instance.PlayShooterPopSound();
        _player.CurrentState = PlayerState.Moving;

        _moveTween?.Kill();
        // _moveTween = transform.DOMove(target.position, moveDuration)
        //     .SetEase(ease)
        //     .OnComplete(OnReachedShootSpot);        

        _moveTween = transform.DOJump(
                targetPos,
                0.3f,
                1,
                moveDuration
            )
            .SetDelay(0.2f)
            .SetEase(Ease.OutQuad);
    }

    public void MoveOut()
    {
        if (leftTarget == null || rightTarget == null) return;
        
        _player.TargetDoc.ReleaseDoc();

        // 1️⃣ Find closest target
        Transform target = (Vector3.Distance(transform.position, leftTarget.position) <
                            Vector3.Distance(transform.position, rightTarget.position))
            ? leftTarget
            : rightTarget;

        // 2️⃣ Rotate towards target
        Vector3 direction = target.position - transform.position;
        direction.y = 0f; // keep horizontal rotation
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.DORotateQuaternion(targetRotation, rotationDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // 3️⃣ Jump move to target with multiple jumps
                Sequence moveSeq = DOTween.Sequence();

                moveSeq.Append(transform.DOJump(
                    target.position,
                    jumpPower,
                    numJumps,
                    moveOutDuration
                ).SetEase(Ease.OutQuad));

                // 4️⃣ Add subtle wobble during movement (Y rotation)
                moveSeq.Join(transform.DOPunchRotation(
                    new Vector3(0f, Random.Range(-wobbleStrength, wobbleStrength), 0f),
                    moveOutDuration,
                    vibrato: 3,
                    elasticity: 0.6f
                ));

                moveSeq.Play();
            });
    }

    [ContextMenu("OnReachedShootSpot")]
    private void OnReachedShootSpot()
    {
        _player.CurrentState = PlayerState.ReadyToShoot;
        _player.MergeManager.CheckForMerge();
        Debug.Log("OnReachedShootSpot!");
        //
    }
}