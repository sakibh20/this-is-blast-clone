using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CubeManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform levelRoot;

    [Header("Output")]
    [SerializeField] private List<CubeColumn> allColumns = new List<CubeColumn>();

    public static CubeManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        PopulateColumns();
    }

    private void PopulateColumns()
    {
        allColumns.Clear();

        Dictionary<float, CubeColumn> columnMap = new Dictionary<float, CubeColumn>();

        foreach (Transform child in levelRoot)
        {
            ColorCube colorCube = child.GetComponent<ColorCube>();
            if (colorCube == null)
                continue;

            float xPos = child.position.x;

            if (!columnMap.ContainsKey(xPos))
                columnMap[xPos] = new CubeColumn();

            columnMap[xPos].cubes.Add(colorCube);
        }

        // Sort cubes in each column by Z (front to back)
        foreach (var kvp in columnMap)
        {
            kvp.Value.cubes = kvp.Value.cubes.OrderBy(c => c.transform.position.z).ToList();
            allColumns.Add(kvp.Value);
        }

        // Optional: sort columns by X for consistent left-to-right order
        allColumns = allColumns.OrderBy(col => col.cubes[0].transform.position.x).ToList();
    }
    
    // public List<ColorCube> GetFrontCubes(CubeColors shooterColor, int targetCount, Vector3 position)
    // {
    //     List<ColorCube> frontCubes = new List<ColorCube>();
    //
    //     foreach(var column in allColumns)
    //     {
    //         // Front cube of this column matching color
    //         ColorCube frontCube = column.cubes.FirstOrDefault(c => c.Color == shooterColor);
    //         if(frontCube != null)
    //             frontCubes.Add(frontCube);
    //     }
    //
    //     return frontCubes;
    // }
    
    public List<ColorCube> GetFrontCubes(CubeColors shooterColor, int targetCount)
    {
        List<ColorCube> selectedCubes = new List<ColorCube>();

        foreach (var column in allColumns)
        {
            if (column.cubes.Count == 0) continue;

            ColorCube frontCube = column.cubes[0]; // always the front cube
            if (frontCube.Color != shooterColor || frontCube.IsReserved) continue;

            selectedCubes.Add(frontCube);

            // Mark as reserved to avoid picking it again
            frontCube.IsReserved = true;

            if (selectedCubes.Count >= targetCount)
                break;
        }

        return selectedCubes;
    }
    
    public void OnCubeDestroyed(ColorCube cube)
    {
        // Find column containing this cube
        CubeColumn column = allColumns.FirstOrDefault(col => col.cubes.Any(c => c.transform == cube.transform));
        if (column != null)
        {
            column.OnFrontCubeDestroyed();
        }
    }
}

[System.Serializable]
public class CubeColumn
{
    public List<ColorCube> cubes = new List<ColorCube>();

    public void OnFrontCubeDestroyed()
    {
        if (cubes.Count == 0) return;

        // Destroyed cube is at index 0
        // CubeColors destroyedColor = cubes[0].Color;
        // cubes[0].IsReserved = false; // reset just in case

        if (cubes.Count > 1)
        {
            for (int i = 1; i < cubes.Count; i++)
            {
                var cube = cubes[i];
                float newZ = cubes[i - 1].transform.position.z;
                cube.transform.DOMoveZ(newZ, 0.3f).SetEase(Ease.OutSine);
            }
        }

        // Remove the destroyed cube from the list
        cubes.RemoveAt(0);
    }
}