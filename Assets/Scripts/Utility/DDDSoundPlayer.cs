using System.Collections;
using UnityEngine;

public class DDDSoundPlayer : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void Play(AudioClip clip, Vector3 position, Transform parent, float minDistance, float maxDistance)
    {
        if (parent != null)
        {
            transform.SetParent(parent);
        }

        transform.localPosition = position;

        _audioSource.clip = clip;
        _audioSource.minDistance = minDistance;
        _audioSource.maxDistance = maxDistance;
        _audioSource.Play();

        StartCoroutine(AutoRelease());
    }

    private IEnumerator AutoRelease()
    {
        float time = _audioSource.clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale);
        yield return YieldCache.WaitForSeconds(time);

        _audioSource.clip = null;
        PoolManager.Instance.Release(gameObject);
    }
}