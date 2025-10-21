using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("Script References")] 
    [SerializeField] private UIManager uiManager;
    
    [Header("Level Setup")]
    [SerializeField] private List<Level> levelPrefabs = new List<Level>();
    [SerializeField] private Transform levelRoot;

    [SerializeField] private List<Renderer> levelThemesRenderer;
    [SerializeField] private List<SpriteRenderer> levelThemesSprites;
    [SerializeField] private List<Image> levelThemesImages;

    [Header("UI References")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Slider progressBar;

    [Header("Settings")]
    [SerializeField] private string levelPrefKey = "CurrentLevel";

    [SerializeField] private bool test;
    [SerializeField] private int testLevel;
    
    private int _currentLevelIndex;
    
    private Level _currentLevelInstance;
    
    private int _totalCubes;
    private int _destroyedCubes;

    public static LevelManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(this);
            return;
        }

        LoadNextLevel();
    }

    private void UpdateLevel()
    {
        if (_currentLevelInstance.UseCustomTheme)
        {
            foreach (Image image in levelThemesImages)
            {
                Color baseColor = image.color;
                image.color = baseColor * _currentLevelInstance.CustomColor;
                
                //ColorUtils.ApplyThemeTint(renderer1, _currentLevelInstance.CustomColor, 0.1f);
            }            
            
            foreach (SpriteRenderer renderer1 in levelThemesSprites)
            {
                ColorUtils.ApplyThemeTint(renderer1, _currentLevelInstance.CustomColor, 0.3f);
            }
            
            foreach (SpriteRenderer renderer1 in _currentLevelInstance.LevelThemesSprites)
            {
                ColorUtils.ApplyThemeTint(renderer1, _currentLevelInstance.CustomColor, 0.5f);
            }

            foreach (Renderer renderer1 in levelThemesRenderer)
            {
                ColorUtils.ApplyThemeTint(renderer1.material, _currentLevelInstance.CustomColor, 1f);
            }
        }
    }

    private void LoadLevelIndex()
    {
        if (test)
        {
            _currentLevelIndex = testLevel;
            return;
        }
        
        _currentLevelIndex = PlayerPrefs.GetInt(levelPrefKey, 0);
        _currentLevelIndex = Mathf.Clamp(_currentLevelIndex, 0, levelPrefabs.Count - 1);
    }

    [ContextMenu("SpawnCurrentLevel")]
    private void SpawnCurrentLevel()
    {
        if (_currentLevelInstance != null)
            DestroyImmediate(_currentLevelInstance);

        if (levelPrefabs.Count == 0)
        {
            Debug.LogWarning("No level prefabs assigned in LevelManager!");
            return;
        }

        Level prefab = levelPrefabs[_currentLevelIndex];
        _currentLevelInstance = Instantiate(prefab, levelRoot);

        UpdateLevel();
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
            levelText.text = $"Level <size=40>{_currentLevelIndex + 1}</size>";
    }

    private void ResetProgressBar()
    {
        if (progressBar != null)
            progressBar.value = 0f;
    }

    public void SetTotalCubes(int total)
    {
        _totalCubes = Mathf.Max(1, total);
        _destroyedCubes = 0;
        if (progressBar != null)
            progressBar.value = 0f;
    }

    public void UpdateProgress()
    {
        _destroyedCubes = Mathf.Clamp(_destroyedCubes + 1, 0, _totalCubes);

        float normalizedValue = (float)_destroyedCubes / _totalCubes;

        if (progressBar != null)
            progressBar.value = normalizedValue;
        
        if (_destroyedCubes >= _totalCubes)
        {
            OnLevelComplete();
        }
    }
    
    private void OnLevelComplete()
    {
        uiManager.ShowLevelCompleteUI(_currentLevelIndex, _currentLevelInstance.LevelBonus);
        
        _currentLevelIndex++;
        if (_currentLevelIndex >= levelPrefabs.Count)
        {
            _currentLevelIndex = 0; 
        }

        PlayerPrefs.SetInt(levelPrefKey, _currentLevelIndex);
        PlayerPrefs.Save();
        
        Invoke(nameof(ClearCurrentLevel), 1.0f);
    }

    private void ClearCurrentLevel()
    {
        if (_currentLevelInstance != null)
            Destroy(_currentLevelInstance.gameObject);
    }
    
    public void LoadNextLevel()
    {
        uiManager.ShowGameUi();
        
        LoadLevelIndex();
        SpawnCurrentLevel();
        UpdateLevelText();
        ResetProgressBar();
    }
}