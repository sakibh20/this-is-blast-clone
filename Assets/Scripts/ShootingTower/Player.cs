using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private MoveQueue moveQueue;
    [SerializeField] private DocManager docManager;
    
    [SerializeField] private PlayerState currentState = PlayerState.ReadyToMove;

    [SerializeField] private int ammoCount;
    [SerializeField] private CubeColors cubeColors;
    
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private List<MeshRenderer> allRenderers;
    
    [SerializeField] private float inactiveDarkenAmount = 0.4f;
    [SerializeField] private Color32 activeAmmoColor;
    [SerializeField] private Color32 inactiveAmmoColor;

    [Space]
    [SerializeField] private bool isSurpriseShooter;
    
    public int AmmoCount => ammoCount;
    public CubeColors Color => cubeColors;
    
    private PlayerShooter _shooter;
    private PlayerMovement _playMovement;
    private Doc _targetDoc;
    
    private Camera _mainCamera;
    public PlayerMovement PlayerMovement => _playMovement;
    public PlayerShooter Shooter => _shooter;
    
    private MergeManager _mergeManager;
    public MergeManager MergeManager => _mergeManager;
    public Doc TargetDoc => _targetDoc;
    
    public PlayerState CurrentState
    {
        get => currentState;
        set
        {
            currentState = value;
            OnChangedCurrentState();
        }
    }

    private void Awake()
    {
        _shooter = GetComponent<PlayerShooter>();
        _playMovement = GetComponent<PlayerMovement>();
        _mainCamera = Camera.main;

        _mergeManager = FindFirstObjectByType<MergeManager>();
        _mergeManager.docManager = docManager;
    }

    private void Start()
    {
        UpdateColorCube();
        
        OnChangedCurrentState();

        if (isSurpriseShooter)
        {
            HideSelf();
        }
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
        moveQueue.OnPlayerClicked(GetComponent<PlayerShooter>());
    }
    
    public void SetTargetDoc(Doc doc)
    {
        _targetDoc = doc;
    }

    public void UpdateAmmoCount(int count = 0)
    {
        ammoCount += count;
        
        ammoText.SetText(ammoCount.ToString());
    }
    
    [ContextMenu("Update Player")]
    private void UpdateColorCube()
    {
        Reveal();
        ammoText.gameObject.SetActive(true);
        UpdateAmmoCount();
    }

    [ContextMenu("HideSelf")]
    private void HideSelf()
    {
        ammoText.gameObject.SetActive(false);
        foreach (MeshRenderer meshRenderer in allRenderers)
        {
            meshRenderer.material = ReferenceManager.Instance.GetMaterialForColor(CubeColors.Surprise) ?? meshRenderer.material;
        }
    }

    private void Reveal()
    {
        foreach (MeshRenderer meshRenderer in allRenderers)
        {
            meshRenderer.material = ReferenceManager.Instance.GetMaterialForColor(cubeColors) ?? meshRenderer.material;
        }
    }

    private void OnChangedCurrentState()
    {
        if (currentState == PlayerState.ReadyToMove)
        {
            ShootActive();
            UpdateColorCube();
        }
        else if (currentState == PlayerState.ReadyToShoot)
        {
            _shooter.OnReadyToShoot();
        }
        else if (currentState == PlayerState.Merging)
        {
            _shooter.StopShooting();
        }
        else if(currentState == PlayerState.Undefined)
        {
            IdleActive();
        }
    }

    [ContextMenu("ShootActive")]
    private void ShootActive()
    {
        ammoText.color = activeAmmoColor;
    }
    
    [ContextMenu("IdleActive")]
    private void IdleActive()
    {
        ammoText.color = inactiveAmmoColor;
    }
}


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