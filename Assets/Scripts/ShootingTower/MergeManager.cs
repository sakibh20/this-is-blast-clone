using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class MergeManager : MonoBehaviour
{
    [SerializeField] private float liftHeight = 0.5f;
    [SerializeField] private float mergeDuration = 0.4f;
    [SerializeField] private Ease mergeEase = Ease.OutBack;

    public DocManager docManager;

    public void CheckForMerge()
    {
        // Group by color among docs with a player
        var colorGroups = docManager.AllDocs
            .Where(d => d.IsReserved && d.CurrentPlayer != null && d.CurrentPlayer.CurrentState != PlayerState.Merging)
            .GroupBy(d => d.CurrentPlayer.Color);

        foreach (var group in colorGroups)
        {
            var sameColorDocs = group
                .OrderBy(d => d.DocTransform.position.x)
                .ToList();

            if (sameColorDocs.Count < 3)
                continue;

            HandleMergesForColor(sameColorDocs);
        }
    }

    private void HandleMergesForColor(List<Doc> sameColorDocs)
    {
        // Example: if we have 4 or 5 same color players, merge in sets of 3 that are closest together
        // We’ll handle overlapping groups gracefully.
        List<List<Doc>> mergeSets = new List<List<Doc>>();

        int index = 0;
        while (index + 2 < sameColorDocs.Count)
        {
            // Always take 3 consecutive docs
            mergeSets.Add(sameColorDocs.Skip(index).Take(3).ToList());
            index += 3;
        }

        // If leftover docs < 3, ignore them this round.
        foreach (var mergeSet in mergeSets)
        {
            PerformMerge(mergeSet);
        }
    }

    private void PerformMerge(List<Doc> docs)
    {
        // safety
        if (docs.Count < 3) return;

        Doc left = docs[0];
        Doc center = docs[1];
        Doc right = docs[2];

        if (left.CurrentPlayer == null || center.CurrentPlayer == null || right.CurrentPlayer == null)
            return;

        Player leftPlayer = left.CurrentPlayer;
        Player centerPlayer = center.CurrentPlayer;
        Player rightPlayer = right.CurrentPlayer;

        // lock their states
        leftPlayer.CurrentState = PlayerState.Merging;
        centerPlayer.CurrentState = PlayerState.Merging;
        rightPlayer.CurrentState = PlayerState.Merging;

        Invoke(nameof(PlayMergeSound), 0.3f);

        // animation sequence
        Sequence seq = DOTween.Sequence();

        // Lift all three up
        seq.AppendInterval(0.1f);
        seq.AppendCallback(() =>
        {
            leftPlayer.transform.DOMoveY(leftPlayer.transform.position.y + liftHeight, 0.2f).SetEase(Ease.OutSine);
            centerPlayer.transform.DOMoveY(centerPlayer.transform.position.y + liftHeight, 0.2f).SetEase(Ease.OutSine);
            rightPlayer.transform.DOMoveY(rightPlayer.transform.position.y + liftHeight, 0.2f).SetEase(Ease.OutSine);
        });

        seq.AppendInterval(0.2f);

        // Move left & right toward center
        seq.AppendCallback(() =>
        {
            Vector3 centerTarget = centerPlayer.transform.position + new Vector3(0, liftHeight, 0);
            leftPlayer.transform.DOMove(centerTarget, mergeDuration).SetEase(Ease.InExpo);
            rightPlayer.transform.DOMove(centerTarget, mergeDuration).SetEase(Ease.InExpo);
        });

        // merge effect
        seq.AppendInterval(mergeDuration);
        seq.AppendCallback(() =>
        {
            int totalAmmo = leftPlayer.AmmoCount + centerPlayer.AmmoCount + rightPlayer.AmmoCount;
            centerPlayer.UpdateAmmoCount(totalAmmo);

            // Cleanup
            leftPlayer.Shooter.Release();
            rightPlayer.Shooter.Release();
            centerPlayer.Shooter.Release();

            left.ReleaseDoc();
            right.ReleaseDoc();

            Destroy(leftPlayer.gameObject);
            Destroy(rightPlayer.gameObject);

            // bring center back down
            centerPlayer.transform.DOMoveY(0, 0.2f).SetEase(Ease.OutSine);
            centerPlayer.CurrentState = PlayerState.ReadyToShoot;
        });

        seq.Play();
    }

    private void PlayMergeSound()
    {
        SoundManager.Instance?.PlayMergeSound();
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }
}
