using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CubeManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform levelRoot;

    [Header("Output")]
    [SerializeField] private List<CubeColumn> allColumns = new List<CubeColumn>();

    private int _totalCube;

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
    }

    private void Start()
    {
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

            _totalCube += 1;
            
            float xPos = child.position.x;

            if (!columnMap.ContainsKey(xPos))
                columnMap[xPos] = new CubeColumn();

            columnMap[xPos].cubes.Add(colorCube);
        }
        foreach (var kvp in columnMap)
        {
            kvp.Value.cubes = kvp.Value.cubes.OrderBy(c => c.transform.position.z).ToList();
            allColumns.Add(kvp.Value);
        }
        allColumns = allColumns.OrderBy(col => col.cubes[0].transform.position.x).ToList();
        
        LevelManager.Instance.SetTotalCubes(_totalCube);
    }
    
    public List<ColorCube> GetFrontCubes(CubeColors shooterColor, int targetCount)
    {
        List<ColorCube> selectedCubes = new List<ColorCube>();

        foreach (var column in allColumns)
        {
            if (column.cubes.Count == 0) continue;

            ColorCube frontCube = column.cubes[0];
            if (frontCube.Color != shooterColor || frontCube.IsReserved) continue;

            selectedCubes.Add(frontCube);
            
            frontCube.IsReserved = true;

            if (selectedCubes.Count >= targetCount)
                break;
        }

        return selectedCubes;
    }
    
    public void OnCubeDestroyed(ColorCube cube)
    {
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

        if (cubes.Count > 1)
        {
            for (int i = 1; i < cubes.Count; i++)
            {
                var cube = cubes[i];
                float newZ = cubes[i - 1].pos.z;
                cube.MoveForwardWithDelay(newZ);
            }
        }

        cubes.RemoveAt(0);
    }
}