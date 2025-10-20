using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("Script References")] 
    [SerializeField] private UIManager uiManager;
    
    [Header("Level Setup")]
    [SerializeField] private List<GameObject> levelPrefabs = new List<GameObject>();
    [SerializeField] private Transform levelRoot;

    [Header("UI References")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Slider progressBar;

    [Header("Settings")]
    [SerializeField] private string levelPrefKey = "CurrentLevel";

    [SerializeField] private bool test;
    [SerializeField] private int testLevel;
    
    private int _currentLevelIndex;
    
    private GameObject _currentLevelInstance;
    
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

        GameObject prefab = levelPrefabs[_currentLevelIndex];
        _currentLevelInstance = Instantiate(prefab, levelRoot);
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
        uiManager.ShowLevelCompleteUI(_currentLevelIndex);
        
        _currentLevelIndex++;
        if (_currentLevelIndex >= levelPrefabs.Count)
        {
            _currentLevelIndex = 0; 
        }

        PlayerPrefs.SetInt(levelPrefKey, _currentLevelIndex);
        PlayerPrefs.Save();
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