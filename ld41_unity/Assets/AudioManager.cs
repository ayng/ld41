using UnityEngine;
using UnityEngine.Audio;
using System;

[System.Serializable]
public class Sound {
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(1f, 3f)]
    public float pitch = 1f;

    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : MonoBehaviour {

    public Sound[] sounds;

    void Awake() {
        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
        }
    }

    public void Play(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarningFormat("Sound clip \"{0}\" not found", s.name);
        }
        s.source.Play();

    }

}
