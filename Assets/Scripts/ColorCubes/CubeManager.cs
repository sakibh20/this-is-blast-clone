using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CubeManager : MonoBehaviour
{
    [Header("Settings")] [SerializeField] private Transform levelRoot;

    [Header("Output")] [SerializeField] private List<CubeColumn> allColumns = new List<CubeColumn>();

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
            allColumns.Add(kvp.Value);
        }

        // Sort columns left → right
        allColumns = allColumns.OrderBy(col => col.cubes[0].transform.position.x).ToList();

        LevelManager.Instance.SetTotalCubes(_totalCube);
    }
    
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
                float minZ = col.cubes.Min(c => c.transform.position.z);
                return col.cubes
                    .Where(c => Mathf.Abs(c.transform.position.z - minZ) < 0.01f)
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
    
    public void OnCubeDestroyed(ColorCube cube)
    {
        // Find which column this cube belongs to
        CubeColumn column = allColumns.FirstOrDefault(col => col.cubes.Contains(cube));
        if (column == null)
            return;

        // Find the true front cube in this column
        ColorCube frontCube = column.cubes
            .OrderBy(c => c.Pos.z)
            .ThenBy(c => c.Pos.y)
            .FirstOrDefault();

        bool wasFrontCube = (cube == frontCube);

        if (wasFrontCube)
        {
            // Tell the column to move forward based on this destroyed cube's position
            column.OnFrontCubeDestroyed(cube);
        }

        // Now remove it after logic that depends on it
        column.cubes.Remove(cube);
    }
}

[System.Serializable]
public class CubeColumn
{
    public List<ColorCube> cubes = new List<ColorCube>();

    public float GetFrontX()
    {
        return cubes.Count > 0 ? cubes[0].transform.position.x : float.MaxValue;
    }
    
    public void OnFrontCubeDestroyed(ColorCube destroyedCube)
    {
        if (cubes.Count == 0)
            return;

        float destroyedZ = destroyedCube.Pos.z;

        // Group cubes by Z layer (same Z → same column depth)
        var groupedByZ = cubes
            .GroupBy(c => Mathf.Round(c.Pos.z * 100f) / 100f)
            .OrderBy(g => g.Key)
            .ToList();

        // Find the destroyed Z layer index
        int destroyedLayerIndex = groupedByZ.FindIndex(g => Mathf.Abs(g.Key - destroyedZ) < 0.01f);
        if (destroyedLayerIndex == -1)
            return;

        // Get groups that are BEHIND (higher Z)
        var behindGroups = groupedByZ.Skip(destroyedLayerIndex + 1).ToList();
        if (behindGroups.Count == 0)
            return;

        // Move from BACK to FRONT (reverse order)
        for (int i = behindGroups.Count - 1; i >= 0; i--)
        {
            float targetZ = i == 0 ? destroyedCube.TargetPos.z : behindGroups[i - 1].Key;

            foreach (var cube in behindGroups[i])
            {
                cube.MoveForwardWithDelay(targetZ);
            }
        }
    }


    // public void OnFrontCubeDestroyed(ColorCube destroyedCube)
    // {
    //     if (cubes.Count == 0)
    //         return;
    //
    //     float destroyedZ = destroyedCube.Pos.z;
    //
    //     // Group cubes by their Z positions
    //     var groupedByZ = cubes
    //         .GroupBy(c => c.Pos.z)
    //         .OrderBy(g => g.Key)
    //         .ToList();
    //
    //     // Find which Z layer was destroyed
    //     int destroyedLayerIndex = groupedByZ.FindIndex(g => Mathf.Abs(g.Key - destroyedZ) < 0.01f);
    //     if (destroyedLayerIndex == -1)
    //         return;
    //
    //     // Get all groups behind (higher Z)
    //     var behindGroups = groupedByZ.Skip(destroyedLayerIndex + 1).ToList();
    //     if (behindGroups.Count == 0)
    //         return;
    //
    //     // Move each layer of cubes forward to the previous layer's target position
    //     for (int i = 0; i < behindGroups.Count; i++)
    //     {
    //         float targetZ = (i == 0)
    //             ? destroyedCube.TargetPos.z
    //             : groupedByZ[destroyedLayerIndex + i].Key - (groupedByZ[destroyedLayerIndex + i].Key - groupedByZ[destroyedLayerIndex + i - 1].Key);
    //
    //         foreach (var cube in behindGroups[i])
    //         {
    //             cube.MoveForwardWithDelay(targetZ);
    //         }
    //     }
    // }
    
    // public void OnFrontCubeDestroyed(ColorCube destroyedCube)
    // {
    //     if (cubes.Count == 0)
    //         return;
    //
    //     float destroyedZ = destroyedCube.Pos.z;
    //
    //     // Get all cubes that are behind (higher Z) than the destroyed one
    //     var behindCubes = cubes
    //         .Where(c => c.Pos.z > destroyedZ)
    //         .OrderBy(c => c.Pos.z)
    //         .ToList();
    //
    //     if (behindCubes.Count == 0)
    //         return;
    //     
    //     for (int i = behindCubes.Count - 1; i >= 0; i--)
    //     {
    //         float targetZ = i == 0 ? destroyedCube.Pos.z : behindCubes[i - 1].Pos.z;
    //     
    //         behindCubes[i].MoveForwardWithDelay(targetZ);
    //     }
    //     
    //     // for (int i = behindCubes.Count - 1; i >= 0; i--)
    //     // {
    //     //     float targetZ = i == 0 ? destroyedCube.TargetPos.z : behindCubes[i - 1].TargetPos.z;
    //     //
    //     //     behindCubes[i].MoveForwardWithDelay(targetZ);
    //     // }
    // }
}
