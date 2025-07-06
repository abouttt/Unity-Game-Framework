using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace GameFramework
{
    public sealed class SoundManager : IService
    {
        public float MasterVolume
        {
            get => GetVolume("Master");
            set => SetVolume("Master", value);
        }

        private readonly Transform _root;
        private readonly AudioMixer _audioMixer;
        private readonly GameObject _dddSoundPlayerPrefab;
        private readonly List<AudioSource> _audioSources = new();
        private readonly PoolManager _poolManager;

        public SoundManager(AudioMixer audioMixer, GameObject dddSoundPlayerPrefab, PoolManager poolManager)
        {
            _root = new GameObject("SoundRoot").transform;
            GameObject.DontDestroyOnLoad(_root.gameObject);

            _audioMixer = audioMixer;
            _dddSoundPlayerPrefab = dddSoundPlayerPrefab;
            _poolManager = poolManager;

            CreateAudioSources();
            _poolManager.CreatePool("DDDSoundPlayer", _dddSoundPlayerPrefab, 5);
        }

        public void Play2D(AudioClip clip, SoundType type, bool playOneShot = true)
        {
            if (clip == null)
            {
                Debug.LogWarning("[SoundManager] Attempted to play a null AudioClip.");
                return;
            }

            var audioSource = _audioSources[(int)type];

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
            var audioSource = _audioSources[(int)type];
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        public async void Play3D(AudioClip clip, SoundType type, Vector3 position, Transform parent = null)
        {
            if (clip == null)
            {
                Debug.LogWarning("[SoundManager] Attempted to play a null AudioClip.");
                return;
            }

            var soundPlayer = _poolManager.Get<DDDSoundPlayer>("DDDSoundPlayer", parent);
            soundPlayer.transform.position = position;
            soundPlayer.Play(clip, _audioSources[(int)type].outputAudioMixerGroup, 1f, 100f);

            float timeScale = Time.timeScale;
            float time = clip.length * ((timeScale < 0.01f) ? 0.01f : timeScale);

            await AutoDestroy(soundPlayer.gameObject, time);
        }

        public float GetVolume(SoundType type)
        {
            return GetVolume(type.ToString());
        }

        public void SetVolume(SoundType type, float volume)
        {
            SetVolume(type.ToString(), volume);
        }

        public void Clear()
        {
            foreach (var audioSource in _audioSources)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }

            _poolManager.RemovePool("DDDSoundPlayer");
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

        private async Awaitable AutoDestroy(GameObject dddSoundPlayer, float delay)
        {
            await Awaitable.WaitForSecondsAsync(delay);
            var poolObject = dddSoundPlayer.GetComponent<PoolObject>();
            _poolManager.Return(poolObject);
        }

        private void CreateAudioSources()
        {
            foreach (SoundType type in Enum.GetValues(typeof(SoundType)))
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

                _audioSources.Add(audioSource);
                go.transform.SetParent(_root);
            }

            _audioSources[(int)SoundType.BGM].loop = true;
        }
    }
}
