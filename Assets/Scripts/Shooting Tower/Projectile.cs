using UnityEngine;
using DG.Tweening;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 2f;

    [HideInInspector] public ObjectPool objectPool;

    private Tween _moveTween;

    private ColorCube _target;

    public void Launch(ColorCube target)
    {
        _target = target;
        _moveTween?.Kill();
        
        float duration = Vector3.Distance(transform.position, target.transform.position) / speed;
        _moveTween = transform.DOMove(target.transform.position, duration)
            .SetEase(Ease.Linear)
            .OnComplete(OnReachedTarget);
        
        //Invoke(nameof(ReturnToPool), lifeTime);
    }

    private void OnReachedTarget()
    {
        _target.DestroyCube();
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        _moveTween?.Kill();
        CancelInvoke();
        objectPool.ReturnToPool(this);
    }
}