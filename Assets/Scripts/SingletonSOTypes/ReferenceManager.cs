using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "SingletonSOs/ReferenceManager", fileName = "ReferenceManager")]
public class ReferenceManager : SingletonSO<ReferenceManager>
{
    [SerializeField] private List<ColorCubeConfig> colorCubeConfig;

    public Material GetMaterialForColor(CubeColors cubeColor)
    {
        foreach (ColorCubeConfig cubeConfig in colorCubeConfig)
        {
            if (cubeConfig.cubeColor == cubeColor) return cubeConfig.colorMat;
        }

        return null;
    }

    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    // public static void OnBeforeSceneLoad()
    // {
    //     
    // }
}

[Serializable]
public class ColorCubeConfig
{
    public CubeColors cubeColor;
    public Material colorMat;
}