using DG.Tweening;
using UnityEngine;

public class ColorCube : MonoBehaviour
{
    [SerializeField] private CubeColors cubeColors;
    [SerializeField] private float hideDuration = 0.3f;
    
    public CubeColors Color => cubeColors;
    
    [SerializeField]
    private MeshRenderer meshRenderer;
    
    public bool IsReserved { get; set; } = false;
    
    [ContextMenu("Update ColorCube")]
    private void UpdateColorCube()
    {
        meshRenderer.material = ReferenceManager.Instance.GetMaterialForColor(cubeColors) ?? meshRenderer.material;
    }

    [ContextMenu("DestroyCube")]
    public void DestroyCube()
    {
        // Notify manager
        CubeManager.Instance.OnCubeDestroyed(this);

        // Destroy gameobject
        Destroy(gameObject);
        
        // transform.DOScale(Vector3.zero, hideDuration).OnComplete(() =>
        // {
        //     Destroy(gameObject);
        // });
    }
}

public enum CubeColors
{
    Yellow,
    Red,
    Blue,
    Green
}