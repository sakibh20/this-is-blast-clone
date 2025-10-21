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

        // Was this cube one of the front-most cubes in the column (lowest Z)?
        float minZ = column.cubes.Min(c => c.pos.z);
        bool wasFrontCube = Mathf.Abs(cube.pos.z - minZ) < 0.01f;

        if (wasFrontCube)
        {
            // Tell the column to move forward based on this destroyed cube's position
            column.OnFrontCubeDestroyed(cube);
        }

        // Now remove it after all logic that depends on its position
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

        float destroyedZ = destroyedCube.pos.z;

        // Get all cubes that are behind (higher Z) than the destroyed one
        var behindCubes = cubes
            .Where(c => c.pos.z > destroyedZ)
            .OrderBy(c => c.pos.z)
            .ToList();

        if (behindCubes.Count == 0)
            return;

        // // Calculate Z offset (distance to move forward)
        // // We use the gap between destroyedZ and the next cube layer as step size
        // float nextZ = behindCubes.First().pos.z;
        // float zStep = nextZ - destroyedZ;
        //
        // // Move each behind cube forward by one step (keep spacing consistent)
        // foreach (var cube in behindCubes)
        // {
        //     float targetZ = cube.pos.z - zStep;
        //     cube.MoveForwardWithDelay(targetZ);
        // }

        for (int i = 0; i < behindCubes.Count; i++)
        {
            if(i == 0) behindCubes[i].MoveForwardWithDelay(destroyedCube.pos.z);
            else behindCubes[i].MoveForwardWithDelay(behindCubes[i-1].pos.z);
        }
    }
}
