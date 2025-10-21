using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip popClip;
    [SerializeField] private AudioClip shooterPopClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip mergeClip;
    [SerializeField] private AudioClip gameDefaultSound;
    [SerializeField] private AudioClip gameHardSound;

    [SerializeField] private AudioSource audioSource;
    
    private float _popCooldown = 0.08f; 
    private float _lastPopTime = -999f;
    
    private float _coinCooldown = 0.08f;
    private float _lastCoinTime = -999f;

    public static SoundManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        PlayGameDefaultSound();
    }

    public void PlayGameDefaultSound()
    {
        audioSource.clip = gameDefaultSound;
        audioSource.Play();
    } 
    
    public void PlayGameHardSound()
    {
        audioSource.clip = gameHardSound;
        audioSource.Play();
    } 
        
    public void PlayPopSound()
    {
        if (Time.time - _lastPopTime < _popCooldown)
            return;

        audioSource.PlayOneShot(popClip);

        _lastPopTime = Time.time;
    }   
    
    public void PlayShooterPopSound()
    {
        audioSource.PlayOneShot(shooterPopClip);
    }
    
    public void PlayWinSound()
    {
        audioSource.PlayOneShot(winClip);
    }    
    
    public void PlayMergeSound()
    {
        audioSource.PlayOneShot(mergeClip);
    }
    
    public void PlayCoinSound()
    {
        if (Time.time - _lastCoinTime < _coinCooldown)
            return;
        
        audioSource.PlayOneShot(coinClip);
        
        _lastCoinTime = Time.time;
    }
}
