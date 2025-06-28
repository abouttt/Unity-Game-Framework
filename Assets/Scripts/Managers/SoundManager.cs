using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundType
{
    BGM,
    SFX,
    UI,
    Voice,
}

public class SoundManager : MonoSingleton<SoundManager>
{
    public float MasterVolume
    {
        get => GetVolume("Master");
        set => SetVolume("Master", value);
    }

    private AudioMixer _audioMixer;
    private GameObject _3dAudioPrefab;
    private readonly Dictionary<SoundType, AudioSource> _audioSources = new();

    protected override void Awake()
    {
        base.Awake();

        var gameSettings = Resources.Load<GameSettings>("GameSettings");
        _audioMixer = gameSettings.AudioMixer;
        _3dAudioPrefab = gameSettings.DDDAudioPrefab;

        foreach (SoundType type in System.Enum.GetValues(typeof(SoundType)))
        {
            var typeName = type.ToString();
            var go = new GameObject(typeName);
            var audioSource = go.AddComponent<AudioSource>();
            var group = _audioMixer.FindMatchingGroups(typeName);

            if (group.Length > 0)
            {
                audioSource.outputAudioMixerGroup = group[0];
            }
            else
            {
                Debug.LogWarning($"[SoundManager] {typeName} audio mixer group does not exist.");
            }

            _audioSources[type] = audioSource;
            go.transform.SetParent(transform);
        }

        _audioSources[SoundType.BGM].loop = true;
    }

    public void Play2D(AudioClip clip, SoundType type, bool playOneShot = true)
    {
        if (clip == null)
        {
            return;
        }

        var audioSource = _audioSources[type];
        if (playOneShot)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public void Stop2D(SoundType type)
    {
        var audioSource = _audioSources[type];
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void StopAll2D()
    {
        foreach (var audioSource in _audioSources.Values)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    public async void Play3D(AudioClip clip, SoundType type, Vector3 position, Transform parent = null,
        float minDistance = 0f, float maxDistance = 15f)
    {
        if (clip == null)
        {
            return;
        }

        var go = PoolManager.Instance.Get(_3dAudioPrefab, parent, true);
        go.transform.position = position;

        var audioSource = go.GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = _audioSources[type].outputAudioMixerGroup;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.Play();

        float length = clip.length / Mathf.Max(audioSource.pitch, 0.01f);
        await Return3DAudioObject(go, length);
    }

    public float GetVolume(SoundType type)
    {
        return GetVolume(type.ToString());
    }

    public void SetVolume(SoundType type, float volume)
    {
        SetVolume(type.ToString(), volume);
    }

    public float GetPitch(SoundType type)
    {
        return _audioSources[type].pitch;
    }

    public void SetPitch(SoundType type, float pitch)
    {
        _audioSources[type].pitch = Mathf.Clamp(pitch, 0.1f, 3f);
    }

    public void Clear()
    {
        foreach (var audioSource in _audioSources.Values)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }

        PoolManager.Instance.RemovePool(_3dAudioPrefab.name);
    }

    private float LinearToDecibel(float linear)
    {
        return linear != 0f ? Mathf.Log10(linear) * 20f : -144f;
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

    private float DecibelToLinear(float dB)
    {
        return Mathf.Pow(10f, dB / 20f);
    }

    private async Awaitable Return3DAudioObject(GameObject go, float delay)
    {
        await Awaitable.WaitForSecondsAsync(delay);
        PoolManager.Instance.Return(go);
    }
}
