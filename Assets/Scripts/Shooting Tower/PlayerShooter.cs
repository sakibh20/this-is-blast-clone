using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private float shootRate = 0.5f;
    [SerializeField] private Transform target;
    [SerializeField] private ObjectPool projectilePool;
    
    [SerializeField] private Transform shootPos;
    private bool _isShooting;
    
    private Player _player;
    
    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void Start()
    {
        CurrentStateChanged();
    }

    private void OnDisable()
    {
        GameEventManager.RemoveListener(Player.CurrentStateUpdateEvent, StartShooting);
        StopShooting();
    }

    private void CurrentStateChanged()
    {
        GameEventManager.AddListener(Player.CurrentStateUpdateEvent, StartShooting);
    }
    
    private void StartShooting(GameEventModel model)
    {
        if(_player.CurrentState != PlayerState.ReadyToShoot) return;
        if (_isShooting) return;
        
        _player.CurrentState = PlayerState.Shooting;
        Debug.Log("Started shooting!");
        
        _isShooting = true;
        StartCoroutine(ShootRoutine());
    }

    private void StopShooting()
    {
        _isShooting = false;
        StopAllCoroutines();
    }

    private IEnumerator ShootRoutine()
    {
        while (_isShooting)
        {
            Shoot();
            yield return new WaitForSeconds(shootRate);
        }
    }

    private void Shoot()
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

        Projectile projectileObj = projectilePool.GetPooledObject();
        projectileObj.transform.position = shootPos.position;
        projectileObj.gameObject.SetActive(true);

        projectileObj.Launch(target);

        _player.UpdateAmmoCount(1);
    }
}
