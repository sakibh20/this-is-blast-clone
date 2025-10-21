using System;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private int levelBonus = 10;
    public int LevelBonus => levelBonus;
    
    [SerializeField] private bool isHard;
    public bool IsHard => isHard;
    
    [SerializeField] private bool customTheme;
    public bool UseCustomTheme => customTheme;
    [SerializeField] private Color32 customColor;
    
    [SerializeField] private List<TargetSpriteRenderers> levelThemesSprites;

    public List<TargetSpriteRenderers> LevelThemesSprites => levelThemesSprites;

    public Color32 CustomColor => customColor;
}
