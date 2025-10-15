using UnityEngine;

public enum PlayerState
{
    Undefined,
    ReadyToMove,
    Moving,
    ReadyToShoot,
    Shooting,
    Merging,
    NoAmmo,
    GoingOut
}

[RequireComponent(typeof(Player))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform shootSpot;

    private PlayerMovement _movement;
    private PlayerShooter _shooter;
    private PlayerMerger _merger;
    private Player _player;

    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
        
        _player = GetComponent<Player>();
        _movement = GetComponent<PlayerMovement>();
        _shooter = GetComponent<PlayerShooter>();
        _merger = GetComponent<PlayerMerger>();
    }
    
    private void Update()
    {
        HandleClickDetection();
    }

    private void HandleClickDetection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    OnPlayerClicked();
                }
            }
        }
    }

    private void OnPlayerClicked()
    {
        if (_player.CurrentState == PlayerState.ReadyToMove)
        {
            _movement.MoveToShootSpot(shootSpot);
        }
    }
}
