using UnityEngine;

namespace SnakeJam
{
    /// Wraps two AudioSources: one for one-shot SFX, one for looping ambience.
    public class GameAudio
    {
        readonly AudioSource sfxSource;
        readonly AudioSource ambientSource;

        public GameAudio(GameObject host)
        {
            sfxSource = host.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;

            ambientSource = host.AddComponent<AudioSource>();
            ambientSource.playOnAwake = false;
            ambientSource.loop = true;
            ambientSource.spatialBlend = 0f;
        }

        public void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        public void PlayAmbient(AudioClip clip, float volume = 0.5f)
        {
            if (clip == null) return;
            if (ambientSource.clip == clip && ambientSource.isPlaying) return; // don't restart
            ambientSource.clip = clip;
            ambientSource.volume = Mathf.Clamp01(volume);
            ambientSource.Play();
        }

        public void StopAmbient()
        {
            ambientSource.Stop();
        }

        public void SetAmbientVolume(float volume)
        {
            ambientSource.volume = Mathf.Clamp01(volume);
        }

        public void RestartAmbient(AudioClip clip, float volume = 0.5f)
        {
            if (clip == null) return;
            ambientSource.Stop();
            ambientSource.clip = clip;
            ambientSource.volume = Mathf.Clamp01(volume);
            ambientSource.time = 0f;
            ambientSource.Play();
        }
    }
}