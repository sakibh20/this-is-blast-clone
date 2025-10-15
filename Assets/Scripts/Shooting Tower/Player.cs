using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerState currentState = PlayerState.ReadyToMove;

    [SerializeField] private int ammoCount;
    public int AmmoCount => ammoCount;
    [SerializeField] private CubeColors cubeColors;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private List<MeshRenderer> allRenderers;
    
    public PlayerState CurrentState
    {
        get => currentState;
        set
        {
            currentState = value;
            OnChangedCurrentState();
        }
    }

    public static string CurrentStateUpdateEvent = "CurrentStateUpdated";

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
        GameEventManager.DispatchEvent(CurrentStateUpdateEvent, null);
    }
}