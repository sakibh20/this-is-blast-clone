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
        
        PopulateColumns();
    }

    private void PopulateColumns()
    {
        allColumns.Clear();

        Dictionary<float, CubeColumn> columnMap = new Dictionary<float, CubeColumn>();

        foreach (Transform child in levelRoot)
        {
            ColorCube cube = child.GetComponent<ColorCube>();
            if (cube == null)
                continue;

            _totalCube++;

            float xPos = Mathf.Round(child.position.x * 100f) / 100f; // reduce float precision errors

            if (!columnMap.ContainsKey(xPos))
                columnMap[xPos] = new CubeColumn();

            columnMap[xPos].cubes.Add(cube);
        }

        foreach (var kvp in columnMap)
        {
            // Sort by Z ascending (front → back)
            kvp.Value.cubes = kvp.Value.cubes.OrderBy(c => c.transform.position.z).ToList();

            // Store initial Z positions for deterministic forward movement
            kvp.Value.InitializePositions();

            allColumns.Add(kvp.Value);
        }

        // Sort columns left → right
        allColumns = allColumns.OrderBy(col => col.cubes[0].transform.position.x).ToList();

        LevelManager.Instance.SetTotalCubes(_totalCube);
    }

    /// <summary>Get front cubes for shooting</summary>
    public List<ColorCube> GetFrontCubes(CubeColors shooterColor, int targetCount)
    {
        List<ColorCube> selectedCubes = new List<ColorCube>();

        // Step 1: Get only cubes from columns that are front-most (lowest Z)
        var frontCubes = allColumns
            .SelectMany(col =>
            {
                if (col.cubes.Count == 0)
                    return new List<ColorCube>();

                // find all cubes in the front-most row of this column (lowest Z)
                float minZ = col.InitialZPositions[0]; // front-most Z from stored positions
                return col.cubes
                    .Where(c => Mathf.Abs(c.Pos.z - minZ) < 0.01f)
                    .ToList();
            })
            .ToList();

        // Step 2: Sort by X ascending, then Y descending
        var sortedCubes = frontCubes
            .OrderBy(c => c.transform.position.x)
            .ThenByDescending(c => c.transform.position.y)
            .ToList();

        // Step 3: Filter by color and reserve state
        foreach (var cube in sortedCubes)
        {
            if (cube.Color != shooterColor || cube.IsReserved)
                continue;

            cube.IsReserved = true;
            selectedCubes.Add(cube);

            if (selectedCubes.Count >= targetCount)
                break;
        }

        return selectedCubes;
    }

    /// <summary>Called when a cube is destroyed</summary>
    public void OnCubeDestroyed(ColorCube cube)
    {
        CubeColumn column = allColumns.FirstOrDefault(col => col.cubes.Contains(cube));
        if (column == null)
            return;

        // Find true front cube in this column
        ColorCube frontCube = column.cubes
            .OrderBy(c => c.Pos.z)
            .ThenBy(c => c.Pos.y)
            .FirstOrDefault();

        bool wasFrontCube = (cube == frontCube);

        if (wasFrontCube)
        {
            column.OnFrontCubeDestroyed(cube);
        }

        // Remove destroyed cube after movement logic
        column.cubes.Remove(cube);
    }
}

[System.Serializable]
public class CubeColumn
{
    public List<ColorCube> cubes = new List<ColorCube>();

    /// <summary>Stores the original Z positions of cubes (front → back)</summary>
    public List<float> InitialZPositions { get; private set; } = new List<float>();

    public float GetFrontX()
    {
        return cubes.Count > 0 ? cubes[0].transform.position.x : float.MaxValue;
    }

    public void InitializePositions()
    {
        InitialZPositions = cubes
            .OrderBy(c => c.transform.position.z)
            .Select(c => c.transform.position.z)
            .ToList();
    }

    public void OnFrontCubeDestroyed(ColorCube destroyedCube)
    {
        if (cubes.Count == 0) return;

        // Find destroyed cube index in the list
        int destroyedIndex = cubes.IndexOf(destroyedCube);
        if (destroyedIndex == -1) return;

        // Move all cubes behind (higher Z) from back to front
        for (int i = cubes.Count - 1; i > destroyedIndex; i--)
        {
            float targetZ = InitialZPositions[i - 1]; // always use stored positions
            cubes[i].MoveForwardWithDelay(targetZ);
        }
    }
}