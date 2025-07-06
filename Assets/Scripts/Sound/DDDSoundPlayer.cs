using UnityEngine;
using UnityEngine.Audio;

namespace GameFramework
{
    public class DDDSoundPlayer : PoolObject
    {
        private AudioSource _audioSource;

        public void Play(AudioClip clip, AudioMixerGroup output, float minDistance, float maxDistance)
        {
            _audioSource.clip = clip;
            _audioSource.outputAudioMixerGroup = output;
            _audioSource.minDistance = minDistance;
            _audioSource.maxDistance = maxDistance;
            _audioSource.Play();
        }

        public override void OnCreate()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public override void OnGetFromPool()
        {
            
        }

        public override void OnReturnToPool()
        {
            _audioSource.clip = null;
        }

        public override void OnRelease()
        {
            
        }
    }
}
