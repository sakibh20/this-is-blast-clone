using DG.Tweening;
using UnityEngine;

public class ColorCube : MonoBehaviour
{
    [SerializeField] private CubeColors cubeColors;
    private float _hideDuration = 0.15f;
    private float _feedbackStrength = 0.2f;

    private float _wobbleStrengthPos = 0.05f;
    private float _wobbleStrengthRot = 15;
    private float _wobbleDuration = 0.15f;

    private Vector3 pos;
    private Vector3 scale;

    private bool _isMoving;
    private Vector3 _moveTarget;

    public Vector3 Scale => scale;
    public Vector3 Pos => pos;

    private Collider _collider;

    public CubeColors Color => cubeColors;

    [SerializeField] private MeshRenderer meshRenderer;

    public bool IsReserved { get; set; } = false;

    private float _interactionCoolDownTime = 2f;
    private float _lastInteractionTime = -999f;

    private void Awake()
    {
        pos = transform.position;
        scale = transform.localScale;
        _collider = GetComponent<Collider>();
    }

    private void Start()
    {
        UpdateVisibility();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        CancelInvoke();

        transform.DOKill();
    }

    private void UpdateVisibility()
    {
        bool visible = pos.y < 0.3f || pos.z < 5.4;

        gameObject.SetActive(visible);

        if (!visible)
        {
            transform.localScale = Vector3.zero;
        }
        else
        {
            transform.DOScale(scale, 0.1f).SetEase(Ease.OutQuad);
        }
    }

    [ContextMenu("Update ColorCube")]
    private void UpdateColorCube()
    {
        meshRenderer.material = ReferenceManager.Instance.GetMaterialForColor(cubeColors) ?? meshRenderer.material;
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (!other.CompareTag("projectile")) return;
    //     
    //     Projectile projectile = other.GetComponent<Projectile>();
    //     if (projectile == null) return;
    //     
    //     if (Time.time - _lastInteractionTime < _interactionCoolDownTime)
    //         return;
    //     
    //     // Avoid reacting if this cube *is* the target
    //     if (projectile.TargetCube != null && projectile.TargetCube != this)
    //     {
    //         Vector3 hitDir = (transform.position - projectile.transform.position).normalized;
    //         DoCollisionFeedback(hitDir);
    //     }
    // }

    private void DoCollisionFeedback(Vector3 hitDir)
    {
        Vector3 punchPos = hitDir * _wobbleStrengthPos;
        transform.DOPunchPosition(punchPos, _wobbleDuration, vibrato: 1, elasticity: 0.4f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(UpdateType.Normal, true)
            .SetId("cubeImpactPos");
        
        // Use the hit direction projected on the Y axis for a “twist” effect
        Vector3 localHitDir = transform.InverseTransformDirection(hitDir);
        float rotAngle = _wobbleStrengthRot * Mathf.Sign(localHitDir.x); // rotate toward hit direction
        Vector3 punchRot = new Vector3(0f, rotAngle, 0f);

        transform.DOPunchRotation(punchRot, _wobbleDuration, vibrato: 1, elasticity: 0.4f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(UpdateType.Normal, true)
            .SetId("cubeImpactRot");
    }

    [ContextMenu("DestroyCube")]
    public void DestroyCube()
    {
        _collider.enabled = false;
        CubeManager.Instance.OnCubeDestroyed(this);
        LevelManager.Instance.UpdateProgress();

        SoundManager.Instance.PlayPopSound();

        transform.DOKill();

        transform.DOScale(Vector3.zero, _hideDuration).OnComplete(() => { Destroy(gameObject); });
    }

    public void MoveForwardWithDelay(float newZ)
    {
        transform.DOMoveZ(newZ, 0.15f)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                pos.z = newZ;
                Vector3 wobbleDir = transform.forward * 0.1f;
                transform.DOPunchPosition(-wobbleDir, 0.15f, vibrato: 2, elasticity: 0.5f)
                    .SetEase(Ease.OutBounce)
                    .SetId("cubeMoveWobble");
                UpdateVisibility();
            }).SetDelay(_hideDuration);
    }
}

public enum CubeColors
{
    Yellow,
    Red,
    Blue,
    Green,
    Orange,
    Surprise
}