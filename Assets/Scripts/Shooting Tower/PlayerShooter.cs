using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private float shootRate = 0.5f;
    [SerializeField] private ObjectPool projectilePool;
    
    [SerializeField] private float recoilStrength = 0.1f;
    [SerializeField] private float recoilDuration = 0.1f;
    [SerializeField] private float rotationThreshold = 20f;
    
    [SerializeField] private Transform shootPos;
    
    private Player _player;
    
    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void Start()
    {
        
    }

    private void OnDisable()
    {
        CancelInvoke();
        StopShooting();
    }

    public void OnReadyToShoot()
    {
        _player.CurrentState = PlayerState.Shooting;
        
        CheckAndShoot();
    }

    private void CheckAndShoot()
    {
        int targetCount = _player.AmmoCount >= 5 ? 5 : _player.AmmoCount;
            
        List<ColorCube> allTargets = CubeManager.Instance.GetFrontCubes(_player.Color, targetCount);
        StartShooting(allTargets);
        
        // if (allTargets.Count > 0)
        // {
        //     StartShooting(allTargets);
        // }
        // else
        // {
        //     Invoke(nameof(CheckAndShoot), 0.1f);
        //     return;
        // }
    }

    private void StartShooting(List<ColorCube> allTargets)
    {
        //Debug.Log("Started shooting!");

        StartCoroutine(ShootRoutine(allTargets, CheckAndShoot));
    }

    private void StopShooting()
    {
        StopAllCoroutines();
    }
    
    private IEnumerator ShootRoutine(List<ColorCube> allTargets, Action onComplete)
    {
        foreach (ColorCube cube in allTargets)
        {
            Shoot(cube);
            yield return new WaitForSeconds(shootRate);
        }
        if(allTargets.Count == 0)
            yield return new WaitForSeconds(shootRate);
        
        onComplete?.Invoke();
    }

    private void Shoot(ColorCube target)
    {
        if (_player == null)
        {
            Debug.LogWarning("Player reference missing in PlayerShooter!");
            return;
        }

        if (_player.AmmoCount <= 0)
        {
            Debug.Log("Out of ammo!");
            
            StopShooting();
            return;
        }
        
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f; // keep rotation only on Y axis

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

        float rotationDuration = shootRate * 0.3f;

        // if (angleDifference > rotationThreshold)
        // {
            // Rotate towards target
            transform.DORotateQuaternion(targetRotation, rotationDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    HandleShoot(target);
                });
        // }
        // else
        // {
        //     HandleShoot(target);
        // }
    }

    private void HandleShoot(ColorCube target)
    {
        SpawnProjectile(target);
        DoRecoilFeedback(target, recoilDuration / 2f);
    }
    
    private void SpawnProjectile(ColorCube target)
    {
        Projectile projectileObj = projectilePool.GetPooledObject();
        projectileObj.transform.position = shootPos.position;
        projectileObj.gameObject.SetActive(true);

        projectileObj.Launch(target);

        _player.UpdateAmmoCount(1);
    }

    private void DoRecoilFeedback(ColorCube target, float duration)
    {
        transform.DOKill();

        Vector3 recoilDir = -target.transform.forward * recoilStrength;
        transform.DOPunchPosition(recoilDir, duration, vibrato: 5, elasticity: 0.6f);
    }
}
