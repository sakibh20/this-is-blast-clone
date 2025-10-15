using UnityEngine;
using DG.Tweening;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 2f;

    [HideInInspector] public ObjectPool objectPool;

    private Tween _moveTween;

    public void Launch(Transform target)
    {
        _moveTween?.Kill();
        
        float duration = Vector3.Distance(transform.position, target.position) / speed;
        _moveTween = transform.DOMove(target.position, duration)
            .SetEase(Ease.Linear)
            .OnComplete(ReturnToPool);
        
        Invoke(nameof(ReturnToPool), lifeTime);
    }

    private void ReturnToPool()
    {
        _moveTween?.Kill();
        CancelInvoke();
        objectPool.ReturnToPool(this);
    }
}