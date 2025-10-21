using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Random = UnityEngine.Random;

public class UIManager : MonoBehaviour
{
    [Header("Script References")] 
    [SerializeField] private ScoreManager scoreManager;
    
    [Header("HUD Elements")]
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject levelProgress;
    [SerializeField] private GameObject bottomPanel;
    
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject youWinText;
    [SerializeField] private GameObject coinCount;
    [SerializeField] private RectTransform buttons;
    
    [Header("Main Elements")]
    [SerializeField] private CanvasGroup mainPanel;
    [SerializeField] private RectTransform victoryText;
    [SerializeField] private RectTransform nextFeaturePanel;
    [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text percentageText;

    [Header("Coins")]
    [SerializeField] private RectTransform[] coinIcons;
    [SerializeField] private RectTransform hudCoinTarget;
    
    [SerializeField] private TextMeshProUGUI hudCoinText;
    [SerializeField] private float coinPopScale = 1.4f;
    [SerializeField] private float coinPopDuration = 0.25f;
    [SerializeField] private float coinCountTweenDuration = 0.5f;

    private int _currentCoins;
    private int _targetCoins;

    [Header("Tween Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float slideInDuration = 0.5f;
    [SerializeField] private float coinTweenDelay = 0.1f;
    [SerializeField] private float coinMoveDuration = 0.8f;
    [SerializeField] private float coinFloatStrength = 10f;

    private Vector3[] _coinInitialPos;
    private Vector3[] _coinInitialScale;

    private void Awake()
    {
        DOTween.SetTweensCapacity(1500, 50);
        
        // Store initial transforms for coins
        _coinInitialPos = new Vector3[coinIcons.Length];
        _coinInitialScale = new Vector3[coinIcons.Length];
        for (int i = 0; i < coinIcons.Length; i++)
        {
            _coinInitialPos[i] = coinIcons[i].localPosition;
            _coinInitialScale[i] = coinIcons[i].localScale;
        }
    }

    [ContextMenu("ShowGameUi")]
    public void ShowGameUi()
    {
        FadeOutMainPanel();
        
        settings.gameObject.SetActive(true);
        levelProgress.gameObject.SetActive(true);
        bottomPanel.gameObject.SetActive(true);
        buttons.localScale = Vector3.zero;
    }

    #region Main Animations

    [ContextMenu("ShowLevelCompleteUI")]
    public void ShowLevelCompleteUI(int level, int bonus)
    { 
        levelText.SetText("Level " + (level+1));
        SoundManager.Instance.PlayWinSound();
        
        FadeInMainPanel();
        AnimateVictoryText();
        AnimateNextFeaturePanel();
        Invoke(nameof(AnimateCoinsToHUD), 1.5f);
        AnimateHUDCoinFeedback(bonus);
    }

    [ContextMenu("FadeInMainPanel")]
    private void FadeInMainPanel()
    {
        ResetCoins();
        
        settings.gameObject.SetActive(false);
        levelProgress.gameObject.SetActive(false);
        bottomPanel.gameObject.SetActive(false);
        
        mainPanel.alpha = 0f;
        mainPanel.interactable = true;
        mainPanel.blocksRaycasts = true;
        mainPanel.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad);
    }
    
    private void FadeOutMainPanel()
    {
        mainPanel.alpha = 1f;
        mainPanel.interactable = false;
        mainPanel.blocksRaycasts = false;
        mainPanel.DOFade(0f, fadeDuration).SetEase(Ease.OutQuad);
        
        youWinText.SetActive(false);
        coinCount.SetActive(false);
        
        buttons.localScale = Vector3.one;
    }

    [ContextMenu("AnimateVictoryText")]
    private void AnimateVictoryText()
    {
        victoryText.localScale = Vector3.zero;

        // Slightly lower start pos
        Vector3 targetPos = victoryText.anchoredPosition;
        victoryText.anchoredPosition = new Vector2(0, 233);

        Sequence seq = DOTween.Sequence();
        seq.Append(victoryText.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
        seq.Append(victoryText.DOAnchorPos(targetPos, 0.3f).SetEase(Ease.OutQuad));
        seq.Play();
    }
    
    [ContextMenu("AnimateNextFeaturePanel")]
    private void AnimateNextFeaturePanel()
    {
        Vector2 targetPos = nextFeaturePanel.anchoredPosition;
        float screenHeight = Screen.height;
        nextFeaturePanel.anchoredPosition = new Vector2(targetPos.x, -screenHeight);

        nextFeaturePanel.DOAnchorPos(targetPos, 1.5f).SetDelay(0.4f).SetEase(Ease.OutCirc);
        
        AnimateProgressFill(0.25f);
    }

    [ContextMenu("AnimateProgressFill")]
    public void AnimateProgressFill(float targetValue)
    {
        progressFill.fillAmount = 0f;
        progressFill.DOFillAmount(targetValue, 1f).SetDelay(1f).SetEase(Ease.OutCubic);

        DOTween.To(() => 0f, x =>
        {
            percentageText.text = $"{Mathf.RoundToInt(x * 100)}%";
        }, targetValue, 1f).SetDelay(1f).SetEase(Ease.OutCubic);
    }

    [ContextMenu("AnimateCoinsToHUD")]
    private void AnimateCoinsToHUD()
    {
        Invoke(nameof(EnableYouWin), 0.5f);
        for (int i = 0; i < coinIcons.Length; i++)
        {
            RectTransform coin = coinIcons[i];
            
            coin.DOKill();
            coin.localScale = Vector3.zero;
            coin.localPosition = _coinInitialPos[i];
            coin.gameObject.SetActive(true);
            
            Sequence seq = DOTween.Sequence();

            //Small stagger delay between coins
            seq.AppendInterval(i * coinTweenDelay);

            //Scale in & float a bit
            seq.Append(coin.DOScale(_coinInitialScale[i], 0.35f).SetEase(Ease.OutBack));
            seq.Join(coin.DOLocalMoveY(
                coin.localPosition.y + Random.Range(coinFloatStrength * 0.5f, coinFloatStrength),
                0.4f
            ).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine));

            //Idle pause
            seq.AppendInterval(0.5f);

            // Fly to HUD target
            seq.Append(coin.DOMove(hudCoinTarget.transform.position, coinMoveDuration).SetEase(Ease.InBack));

            //Scale out while flying
            seq.Append(coin.DOScale(Vector3.zero, coinMoveDuration * 0.1f).SetEase(Ease.InBack));

            seq.Play();
        }
    }
    
    private void AnimateHUDCoinFeedback(int coinsAdded)
    {
        if (hudCoinText == null) return;


        int startValue = _currentCoins;
        
        scoreManager.AddScore(coinsAdded);
        _targetCoins = scoreManager.CurrentScore;
        int endValue = _targetCoins;

        DOTween.To(() => startValue, x =>
            {
                SoundManager.Instance.PlayCoinSound();
                startValue = x;
                hudCoinText.text = startValue.ToString();
            }, endValue, coinCountTweenDuration)
            .SetDelay(3.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => _currentCoins = _targetCoins);
        
        hudCoinTarget.DOKill();
        hudCoinTarget
            .DOPunchScale(Vector3.one * (coinPopScale - 1f), coinPopDuration, vibrato: 1, elasticity: 0.6f)
            .SetLoops(coinsAdded, LoopType.Restart)
            .SetDelay(3.5f)
            .SetEase(Ease.OutBack)
            .SetLink(hudCoinTarget.gameObject);
    }

    private void ResetCoins()
    {
        for (int i = 0; i < coinIcons.Length; i++)
        {
            coinIcons[i].gameObject.SetActive(false);
            coinIcons[i].localPosition = _coinInitialPos[i];
            coinIcons[i].localScale = _coinInitialScale[i];
        }
    }

    private void EnableYouWin()
    {
        youWinText.SetActive(true);
        coinCount.SetActive(true);

        buttons.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
    }

    #endregion
}
