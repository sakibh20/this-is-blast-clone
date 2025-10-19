using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Random = UnityEngine.Random;

public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject levelProgress;
    
    [Header("Main Elements")]
    [SerializeField] private CanvasGroup mainPanel;
    [SerializeField] private RectTransform victoryText;
    [SerializeField] private RectTransform nextFeaturePanel;
    [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text percentageText;

    [Header("Coins")]
    [SerializeField] private RectTransform[] coinIcons;
    [SerializeField] private Transform hudCoinTarget;

    [Header("Tween Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float scaleInDuration = 0.4f;
    [SerializeField] private float slideUpDuration = 0.4f;
    [SerializeField] private float slideInDuration = 0.5f;
    [SerializeField] private float coinTweenDelay = 0.1f;
    [SerializeField] private float coinMoveDuration = 0.8f;
    [SerializeField] private float coinFloatStrength = 10f;

    private Vector3[] _coinInitialPos;
    private Vector3[] _coinInitialScale;

    private void Awake()
    {
        // Store initial transforms for coins
        _coinInitialPos = new Vector3[coinIcons.Length];
        _coinInitialScale = new Vector3[coinIcons.Length];
        for (int i = 0; i < coinIcons.Length; i++)
        {
            _coinInitialPos[i] = coinIcons[i].localPosition;
            _coinInitialScale[i] = coinIcons[i].localScale;
        }
    }

    private void Start()
    {
        
    }

    [ContextMenu("ShowGameUi")]
    public void ShowGameUi()
    {
        FadeOutMainPanel();
        
        settings.gameObject.SetActive(true);
        levelProgress.gameObject.SetActive(true);
    }

    #region Main Animations

    [ContextMenu("ShowLevelCompleteUI")]
    public void ShowLevelCompleteUI()
    {
        FadeInMainPanel();
        AnimateVictoryText();
        AnimateNextFeaturePanel();
        AnimateCoinsToHUD();
    }

    [ContextMenu("FadeInMainPanel")]
    private void FadeInMainPanel()
    {
        settings.gameObject.SetActive(false);
        levelProgress.gameObject.SetActive(false);
        
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
    }

    [ContextMenu("AnimateVictoryText")]
    private void AnimateVictoryText()
    {
        Vector3 targetScale = victoryText.localScale;
        victoryText.localScale = Vector3.zero;

        // Slightly lower start pos
        Vector3 targetPos = victoryText.anchoredPosition;
        victoryText.anchoredPosition -= new Vector2(0, 50);

        victoryText.DOScale(targetScale, scaleInDuration).SetEase(Ease.OutBack);
        victoryText.DOAnchorPos(targetPos, slideUpDuration).SetEase(Ease.OutQuad);
    }

    [ContextMenu("AnimateNextFeaturePanel")]
    private void AnimateNextFeaturePanel()
    {
        Vector2 targetPos = nextFeaturePanel.anchoredPosition;
        float screenHeight = Screen.height;
        nextFeaturePanel.anchoredPosition = new Vector2(targetPos.x, -screenHeight);

        nextFeaturePanel.DOAnchorPos(targetPos, slideInDuration)
            .SetEase(Ease.OutBack)
            .SetDelay(0.2f);
    }

    [ContextMenu("AnimateProgressFill")]
    public void AnimateProgressFill(float targetValue)
    {
        progressFill.fillAmount = 0f;
        progressFill.DOFillAmount(targetValue, 0.8f).SetEase(Ease.OutCubic);

        DOTween.To(() => 0f, x =>
        {
            percentageText.text = $"{Mathf.RoundToInt(x * 100)}%";
        }, targetValue, 0.8f).SetEase(Ease.OutCubic);
    }

    [ContextMenu("AnimateCoinsToHUD")]
    private void AnimateCoinsToHUD()
    {
        for (int i = 0; i < coinIcons.Length; i++)
        {
            RectTransform coin = coinIcons[i];

            // reset
            coin.localScale = Vector3.zero;
            coin.localPosition = _coinInitialPos[i];

            Sequence seq = DOTween.Sequence();

            seq.AppendInterval(i * coinTweenDelay);
            seq.Append(coin.DOScale(_coinInitialScale[i], 0.3f).SetEase(Ease.OutBack));
            seq.Join(coin.DOLocalMoveY(coin.localPosition.y + Random.Range(coinFloatStrength * 0.5f, coinFloatStrength), 0.3f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutSine));

            // Fly to HUD
            seq.Append(coin.DOMove(hudCoinTarget.position, coinMoveDuration)
                .SetEase(Ease.InBack));

            seq.Join(coin.DOScale(Vector3.zero, coinMoveDuration).SetEase(Ease.InBack));

            seq.Play();
        }
    }

    #endregion
}
