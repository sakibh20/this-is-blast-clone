using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private float shootRate = 0.5f;
    
    [SerializeField] private float recoilStrength = 0.1f;
    [SerializeField] private float recoilDuration = 0.1f;
    
    [SerializeField] private float _resetRotationDuration = 3f;
    
    [SerializeField] private Transform shootPos;
    
    private float _rotationDuration = 0.5f;
    
    private Player _player;
    
    private void Awake()
    {
        _player = GetComponent<Player>();
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
        
        if (allTargets.Count > 0)
        {
            StartCoroutine(ShootRoutine(allTargets, CheckAndShoot));
        }
        else
        {
            Invoke(nameof(CheckAndShoot), 0.1f);
            ResetRotation();
            return;
        }
    }

    private void ResetRotation()
    {
        DOTween.Kill("playerRotation");

        Quaternion resetRotation = Quaternion.identity;

        transform.DORotateQuaternion(resetRotation, _resetRotationDuration)
            .SetEase(Ease.OutQuad)
            .SetId("playerRotation");
    }

    private void StopShooting()
    {
        StopAllCoroutines();
    }
    
    private IEnumerator ShootRoutine(List<ColorCube> allTargets, Action onComplete)
    {
        //Debug.Log("ShootRoutine");
        foreach (ColorCube cube in allTargets)
        {
            Shoot(cube);
            yield return new WaitForSeconds(shootRate);
        }

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
        
        HandleShoot(target);
    }
    
    private void HandleShoot(ColorCube target)
    {
        RotateAndShoot(target);

        SpawnProjectile(target);

        DoRecoilFeedback(target, recoilDuration / 2f);
    }
    
    private void RotateAndShoot(ColorCube target)
    {
        if (target == null) return;

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.DORotateQuaternion(targetRotation, _rotationDuration)
            .SetEase(Ease.OutQuad)
            .SetId("playerRotation"); 
    }

    private void DoRecoilFeedback(ColorCube target, float duration)
    {
        if (target == null) return;
        
        Vector3 recoilDir = -target.transform.forward * recoilStrength;

        DOTween.Kill("playerRecoil");

        transform.DOPunchPosition(recoilDir, duration, vibrato: 5, elasticity: 0.6f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(UpdateType.Normal, true)
            .SetId("playerRecoil");
    }
    
    private void SpawnProjectile(ColorCube target)
    {
        Projectile projectileObj = ObjectPool.Instance.GetPooledObject();
        projectileObj.transform.position = shootPos.position;
        projectileObj.gameObject.SetActive(true);

        projectileObj.Launch(target);

        _player.UpdateAmmoCount(1);
    }
}
