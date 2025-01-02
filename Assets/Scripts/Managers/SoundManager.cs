using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public sealed class SoundManager : MonoBehaviourSingleton<SoundManager>
{
    public static float MasterVolume
    {
        get => Instance.GetVolume("Master");
        set => Instance.SetVolume("Master", value);
    }

    [SerializeField, ReadOnly]
    private AudioMixer _audioMixer;

    private readonly List<AudioSource> _audioSources = new();

    protected override void Init()
    {
        base.Init();

        _audioMixer = Resources.Load<AudioMixer>("AudioMixer");
        if (_audioMixer == null)
        {
            Debug.LogWarning($"[SoundManager.Init] AudioMixer does not exist. Create an audio mixer in the Resources folder.");
        }

        foreach (SoundType type in Enum.GetValues(typeof(SoundType)))
        {
            string typeName = type.ToString();

            var go = new GameObject(typeName);
            go.transform.SetParent(transform);

            var audioSource = go.AddComponent<AudioSource>();
            var group = _audioMixer.FindMatchingGroups(typeName);
            if (group.Length > 0)
            {
                audioSource.outputAudioMixerGroup = group[0];
            }
            else
            {
                Debug.LogWarning($"[SoundManager.Init] {typeName} audio mixer group does not exist.");
            }

            _audioSources.Add(audioSource);
        }

        _audioSources[(int)SoundType.BGM].loop = true;
    }

    protected override void Dispose()
    {
        base.Dispose();
        Clear();

        foreach (var audioSource in _audioSources)
        {
            Destroy(audioSource.gameObject);
        }

        _audioSources.Clear();
    }

    public static void Play2D(string key, SoundType type)
    {
        ResourceManager.LoadAsync<AudioClip>(key, clip => Play2D(clip, type));
    }

    public static void Play2D(AudioClip clip, SoundType type)
    {
        if (clip == null)
        {
            return;
        }

        var audioSource = Instance._audioSources[(int)type];

        if (type == SoundType.BGM)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public static void Stop2D(SoundType type)
    {
        var audioSource = Instance._audioSources[(int)type];
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public static void Play3D(string key, Vector3 position, Transform parent = null, float minDistance = 0f, float maxDistance = 15f)
    {
        ResourceManager.LoadAsync<AudioClip>(key, clip => Play3D(clip, position, parent, minDistance, maxDistance));
    }

    public static void Play3D(AudioClip clip, Vector3 position, Transform parent = null, float minDistance = 0f, float maxDistance = 15f)
    {
        if (clip == null)
        {
            return;
        }

        ResourceManager.InstantiateAsync<DDDSoundPlayer>("DDDSoundPlayer", soundPlayer =>
        {
            soundPlayer.transform.SetParent(parent);
            soundPlayer.transform.localPosition = position;
            soundPlayer.Play(clip, minDistance, maxDistance);
        });
    }

    public static float GetVolume(SoundType type)
    {
        return Instance.GetVolume(type.ToString());
    }

    public static void SetVolume(SoundType type, float volume)
    {
        Instance.SetVolume(type.ToString(), volume);
    }

    public static void Clear()
    {
        foreach (var audioSource in Instance._audioSources)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }

        PoolManager.RemovePool("DDDSoundPlayer");
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
