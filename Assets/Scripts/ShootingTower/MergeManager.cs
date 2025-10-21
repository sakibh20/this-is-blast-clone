using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEditor;

public class MergeManager : MonoBehaviour
{
    [SerializeField] private float liftHeight = 0.5f;
    [SerializeField] private float mergeDuration = 0.4f;
    [SerializeField] private Ease mergeEase = Ease.OutBack;

    public DocManager docManager;

    public void CheckForMerge()
    {
        // Group by color of players currently on doc positions
        var colorGroups = docManager.AllDocs
            .Where(d => d.IsReserved && d.CurrentPlayer != null)
            .GroupBy(d => d.CurrentPlayer.Color);

        foreach (var group in colorGroups)
        {
            var sameColorDocs = group.ToList();
            if (sameColorDocs.Count < 3)
                continue;

            // Sort by X pos so we can find left, center, right
            sameColorDocs = sameColorDocs.OrderBy(d => d.DocTransform.position.x).ToList();

            PerformMerge(sameColorDocs);
        }
    }

    private void PerformMerge(List<Doc> docs)
    {
        if (docs.Count < 3)
            return;
        
        // Choose left, center, right
        Doc left = docs[0];
        Doc center = docs[1];
        Doc right = docs[2];

        Player leftPlayer = left.CurrentPlayer;
        Player centerPlayer = center.CurrentPlayer;
        Player rightPlayer = right.CurrentPlayer;

        leftPlayer.CurrentState = PlayerState.Merging;
        centerPlayer.CurrentState = PlayerState.Merging;
        rightPlayer.CurrentState = PlayerState.Merging;
        
        Invoke(nameof(PlayMergeSound), 0.5f);

        // Lift all three up slightly
        Sequence seq = DOTween.Sequence();
        
        seq.AppendInterval(0.2f);

        seq.Append(leftPlayer.transform.DOMoveY(leftPlayer.transform.position.y + liftHeight, 0.2f).SetEase(Ease.OutSine));
        seq.Join(centerPlayer.transform.DOMoveY(centerPlayer.transform.position.y + liftHeight, 0.2f).SetEase(Ease.OutSine));
        seq.Join(rightPlayer.transform.DOMoveY(rightPlayer.transform.position.y + liftHeight, 0.2f).SetEase(Ease.OutSine));
        
        seq.Join(leftPlayer.transform.DOLocalRotate(Vector3.zero, 0.1f));
        seq.Join(centerPlayer.transform.DOLocalRotate(Vector3.zero,0.1f));
        seq.Join(rightPlayer.transform.DOLocalRotate(Vector3.zero, 0.1f));

        seq.AppendInterval(0.1f);

        // Move left & right toward center
        seq.Append(leftPlayer.transform.DOMove(centerPlayer.transform.position + new Vector3(0, liftHeight, 0), mergeDuration).SetEase(mergeEase));
        seq.Join(rightPlayer.transform.DOMove(centerPlayer.transform.position  + new Vector3(0, liftHeight, 0), mergeDuration).SetEase(mergeEase));

        // Merge ammo counts
        seq.AppendCallback(() =>
        {
            int totalAmmo = leftPlayer.AmmoCount + rightPlayer.AmmoCount;
            centerPlayer.UpdateAmmoCount(totalAmmo);
            
            leftPlayer.Shooter.Release();
            rightPlayer.Shooter.Release();
            centerPlayer.Shooter.Release();

            //EditorApplication.isPaused = true;

            // Optionally play merge FX / SFX
            //centerPlayer.PlayMergeFX();

            // Destroy or disable left and right players
            
            Destroy(leftPlayer.gameObject);
            Destroy(rightPlayer.gameObject);

            // Release doc slots
            left.ReleaseDoc();
            right.ReleaseDoc();
        });
        
        seq.Append(centerPlayer.transform.DOMoveY(0, 0.1f).SetEase(Ease.OutSine));
        
        seq.AppendCallback(() =>
        {
            centerPlayer.CurrentState = PlayerState.ReadyToShoot;
        });

        seq.Play();
    }

    private void PlayMergeSound()
    {
        SoundManager.Instance.PlayMergeSound();
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }
}
