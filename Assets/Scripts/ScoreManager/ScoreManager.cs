using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private const string SCORE_KEY = "PLAYER_SCORE";

    [Header("UI (Optional)")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Settings")]
    [SerializeField] private int defaultScore = 300;

    private int _currentScore;
    public int CurrentScore => _currentScore;

    private void Awake()
    {
        LoadScore();
        UpdateScoreUI();
    }
    
    private void LoadScore()
    {
        _currentScore = PlayerPrefs.GetInt(SCORE_KEY, defaultScore);
    }
    
    private void SaveScore()
    {
        PlayerPrefs.SetInt(SCORE_KEY, _currentScore);
        PlayerPrefs.Save();
    }
    
    public void AddScore(int amount)
    {
        _currentScore += amount;
        SaveScore();
    }
    
    public void SubtractScore(int amount)
    {
        _currentScore = Mathf.Max(0, _currentScore - amount);
        SaveScore();
    }
    
    public void ResetScore()
    {
        _currentScore = defaultScore;
        SaveScore();
    }
    
    public int GetScore()
    {
        return _currentScore;
    }
    
    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = _currentScore.ToString();
    }
}