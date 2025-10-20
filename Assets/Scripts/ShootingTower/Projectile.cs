using System;
using UnityEngine;
using DG.Tweening;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private GameObject sphere;
    [SerializeField] private ParticleSystem _particleSystem;
    //[SerializeField] private float lifeTime = 2f;

    [HideInInspector] public ObjectPool objectPool;
    
    private Tween _moveTween;
    private TrailRenderer _trail;
    
    private ColorCube _target;
    public ColorCube TargetCube => _target;

    private void Awake()
    {
        _trail = GetComponent<TrailRenderer>();
    }

    public void Launch(ColorCube target)
    {
        _target = target;
        _moveTween?.Kill();
        
        sphere.SetActive(true);
        UpdateTrail(true);
        
        float duration = Vector3.Distance(transform.position, target.transform.position) / speed;
        _moveTween = transform.DOMove(target.transform.position - new Vector3(0,0,0.4f), duration)
            .SetEase(Ease.Linear)
            .OnComplete(OnReachedTarget);
        
        //Invoke(nameof(ReturnToPool), lifeTime);
    }

    private void OnReachedTarget()
    {
        _target.DestroyCube();
        UpdateTrail(false); 
        OnHit();
        sphere.SetActive(false);
        Invoke(nameof(ReturnToPool), 3f);
        //ReturnToPool();
    }

    private void UpdateTrail(bool isOn)
    {
        _trail.enabled = isOn;
    }

    private void OnHit()
    {
        _particleSystem.Play();
    }

    private void ReturnToPool()
    {
        CancelInvoke();
        _moveTween?.Kill();
        objectPool.ReturnToPool(this);
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }
}