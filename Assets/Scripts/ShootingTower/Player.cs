using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerState currentState = PlayerState.ReadyToMove;

    [SerializeField] private int ammoCount;
    [SerializeField] private CubeColors cubeColors;
    
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private List<MeshRenderer> allRenderers;
    
    public int AmmoCount => ammoCount;
    public CubeColors Color => cubeColors;
    
    private PlayerShooter _shooter;
    private PlayerMerger _merger;
    
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
        _merger = GetComponent<PlayerMerger>();
    }

    public void UpdateAmmoCount(int count = 0)
    {
        ammoCount -= count;
        
        ammoText.SetText(ammoCount.ToString());
    }
    
    [ContextMenu("Update Player")]
    private void UpdateColorCube()
    {
        foreach (MeshRenderer meshRenderer in allRenderers)
        {
            meshRenderer.material = ReferenceManager.Instance.GetMaterialForColor(cubeColors) ?? meshRenderer.material;
        }

        UpdateAmmoCount();
    }

    private void OnChangedCurrentState()
    {
        if (currentState == PlayerState.ReadyToShoot)
        {
            _shooter.OnReadyToShoot();
        }
    }
}