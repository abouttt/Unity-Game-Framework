using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public sealed class SoundManager : MonoBehaviourSingleton<SoundManager>
{
    public float MasterVolume
    {
        get
        {
            return GetVolume("Master");
        }
        set
        {
            SetVolume("Master", value);
        }
    }

    [SerializeField, ReadOnly]
    private AudioMixer _audioMixer;

    private readonly List<AudioSource> _audioSources = new();

    protected override void Init()
    {
        base.Init();

        _audioMixer = Resources.Load<AudioMixer>("Audio/AudioMixer");

        foreach (var typeName in Enum.GetNames(typeof(SoundType)))
        {
            var go = new GameObject(typeName);
            go.transform.SetParent(transform);

            var audioSource = go.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = _audioMixer.FindMatchingGroups(typeName)[0];

            _audioSources.Add(audioSource);
        }

        _audioSources[(int)SoundType.BGM].loop = true;
    }

    public void Play2D(string key, SoundType soundType)
    {
        ResourceManager.Instance.LoadAsync<AudioClip>(key, audioClip => Play2D(audioClip, soundType));
    }

    public void Play2D(AudioClip audioClip, SoundType soundType)
    {
        var audioSource = _audioSources[(int)soundType];

        if (soundType == SoundType.BGM)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else
        {
            audioSource.PlayOneShot(audioClip);
        }
    }

    public void Stop2D(SoundType soundType)
    {
        _audioSources[(int)soundType].Stop();
    }

    public void Play3D(string key, Vector3 position, Transform parent = null, float minDistance = 0f, float maxDistance = 15f)
    {
        ResourceManager.Instance.LoadAsync<AudioClip>(key, clip => Play3D(clip, position, parent, minDistance, maxDistance));
    }

    public void Play3D(AudioClip clip, Vector3 position, Transform parent = null, float minDistance = 0f, float maxDistance = 15f)
    {
        var go = new GameObject("3D Sound Player");
        go.transform.SetParent(parent);
        go.transform.localPosition = position;

        var audioSource = go.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = _audioMixer.FindMatchingGroups(SoundType.SFX.ToString())[0];
        audioSource.spatialBlend = 1f;
        audioSource.dopplerLevel = 0f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.Play();

        Destroy(go, clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
    }

    public float GetVolume(SoundType soundType)
    {
        return GetVolume(soundType.ToString());
    }

    public void SetVolume(SoundType soundType, float volume)
    {
        SetVolume(soundType.ToString(), volume);
    }

    public void Clear()
    {
        foreach (var audioSource in _audioSources)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }

    private float GetVolume(string name)
    {
        _audioMixer.GetFloat(name, out float dB);
        return DecibelToLinear(dB);
    }

    private void SetVolume(string name, float volume)
    {
        float linear = Mathf.Clamp(volume, 0f, 1f);
        _audioMixer.SetFloat(name, LinearToDecibel(linear));
    }

    private float LinearToDecibel(float linear)
    {
        return linear != 0f ? Mathf.Log10(linear) * 20f : -144f;
    }

    private float DecibelToLinear(float dB)
    {
        return Mathf.Pow(10f, dB / 20f);
    }
}
