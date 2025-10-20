using DG.Tweening;
using UnityEngine;

public class ColorCube : MonoBehaviour
{
    [SerializeField] private CubeColors cubeColors;
    private float _hideDuration = 0.2f;
    private float _feedbackStrength = 0.2f;
    
    private float _wobbleStrengthPos = 0.15f;
    private float _wobbleStrengthRot = 15;
    private float _wobbleDuration = 0.15f;

    private Collider _collider;
    
    public CubeColors Color => cubeColors;
    
    [SerializeField]
    private MeshRenderer meshRenderer;
    
    public bool IsReserved { get; set; } = false;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        CancelInvoke();

        transform.DOKill();
    }

    [ContextMenu("Update ColorCube")]
    private void UpdateColorCube()
    {
        meshRenderer.material = ReferenceManager.Instance.GetMaterialForColor(cubeColors) ?? meshRenderer.material;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("projectile")) return;

        Projectile projectile = other.GetComponent<Projectile>();
        if (projectile == null) return;

        // Avoid reacting if this cube *is* the target
        if (projectile.TargetCube.GetInstanceID() != GetInstanceID())
        {
            Vector3 hitDir = (transform.position - projectile.transform.position).normalized;
            DoCollisionFeedback(hitDir);
        }
    }
    
    // private void DoCollisionFeedback(Transform target)
    // {
    //     //transform.DOKill();
    //
    //     Vector3 recoilDir = target.transform.forward * _feedbackStrength;
    //     transform.DOPunchPosition(recoilDir, 0.2f, vibrato: 1, elasticity: 0.6f);
    // }
    
    private void DoCollisionFeedback(Vector3 hitDir)
    {
        // --- POSITION WOBBLE ---
        Vector3 punchPos = hitDir * _wobbleStrengthPos;
        transform.DOPunchPosition(punchPos, _wobbleDuration, vibrato: 2, elasticity: 0.4f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(UpdateType.Normal, true)
            .SetId("cubeImpactPos");

        // --- ROTATION WOBBLE ---
        // Use the hit direction projected on the Y axis for a “twist” effect
        Vector3 localHitDir = transform.InverseTransformDirection(hitDir);
        float rotAngle = _wobbleStrengthRot * Mathf.Sign(localHitDir.x); // rotate toward hit direction
        Vector3 punchRot = new Vector3(0f, rotAngle, 0f);

        transform.DOPunchRotation(punchRot, _wobbleDuration, vibrato: 2, elasticity: 0.6f)
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
        
        transform.DOScale(Vector3.zero, _hideDuration).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    public void MoveForwardWithDelay(float newZ)
    {
        transform.DOMoveZ(newZ, 0.15f)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                // Small position wobble
                Vector3 wobbleDir = transform.forward * 0.1f;
                transform.DOPunchPosition(-wobbleDir, 0.15f, vibrato: 2, elasticity: 0.5f)
                    .SetEase(Ease.OutBounce)
                    .SetId("cubeMoveWobble");
            }).SetDelay(_hideDuration);
        
        //StartCoroutine(MoveForwardRoutine(newZ));
    }

    // private IEnumerator MoveForwardRoutine(float newZ)
    // {
    //     yield return new WaitForSeconds(_hideDuration);
    //     //transform.DOMoveZ(newZ, 0.2f).SetEase(Ease.OutSine);
    //     
    //     transform.DOMoveZ(newZ, 0.15f)
    //         .SetEase(Ease.OutSine)
    //         .OnComplete(() =>
    //         {
    //             // Small position wobble
    //             Vector3 wobbleDir = transform.forward * 0.1f;
    //             transform.DOPunchPosition(-wobbleDir, 0.15f, vibrato: 2, elasticity: 0.5f)
    //                 .SetEase(Ease.OutQuad)
    //                 .SetId("cubeMoveWobble");
    //         });
    //
    // }
}

public enum CubeColors
{
    Yellow,
    Red,
    Blue,
    Green
}