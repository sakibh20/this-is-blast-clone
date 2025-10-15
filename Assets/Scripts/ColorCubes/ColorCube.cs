using UnityEngine;

public class ColorCube : MonoBehaviour
{
    [SerializeField]
    private CubeColors cubeColors;
    
    [SerializeField]
    private MeshRenderer meshRenderer;

    [ContextMenu("Update ColorCube")]
    private void UpdateColorCube()
    {
        meshRenderer.material = ReferenceManager.Instance.GetMaterialForColor(cubeColors) ?? meshRenderer.material;
    }
}

public enum CubeColors
{
    Yellow,
    Red,
    Blue,
    Green
}