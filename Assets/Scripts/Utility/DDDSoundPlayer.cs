using System.Collections;
using UnityEngine;

public class DDDSoundPlayer : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void Play(AudioClip clip, float minDistance, float maxDistance)
    {
        _audioSource.clip = clip;
        _audioSource.minDistance = minDistance;
        _audioSource.maxDistance = maxDistance;
        _audioSource.Play();

        StartCoroutine(AutoRelease());
    }

    private IEnumerator AutoRelease()
    {
        float timeScale = Time.timeScale;
        float time = _audioSource.clip.length * ((timeScale < 0.01f) ? 0.01f : timeScale);
        yield return YieldCache.WaitForSeconds(time);

        _audioSource.clip = null;
        PoolManager.Release(gameObject);
    }
}
