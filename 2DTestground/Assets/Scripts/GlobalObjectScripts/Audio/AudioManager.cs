using System;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>  {
    public SoundFile[] SoundFiles;

    private void Start() {
        if (SoundFiles.Length == 0) {
            Debug.Log("There are no sound files.");
            return;
        }

        foreach (var soundFile in SoundFiles) {
            soundFile.AudioSource = gameObject.AddComponent<AudioSource>();
            soundFile.AudioSource.clip = soundFile.AudioClip;
            soundFile.AudioSource.volume = soundFile.Volume;
            soundFile.AudioSource.pitch = soundFile.Pitch;
            soundFile.AudioSource.loop = soundFile.Loop;
            soundFile.AudioSource.outputAudioMixerGroup = FindObjectOfType<AudioMixerGroup>();
        }

        Instance.SoundFiles = SoundFiles;

        Play("Town");
    }

    /// <summary>
    ///     Play the given audio file.
    /// </summary>
    public void Play(string soundFileName) {
        var soundFile = Array.Find(SoundFiles, sound => sound.Name == soundFileName);

        if (soundFile == null) {
            Debug.Log($"Sound file with name {soundFileName} not found.");
            return;
        }

        soundFile.AudioSource.Play();
    }

    /// <summary>
    ///     Stop playing the given audio file.
    /// </summary>
    public void Stop(string soundFileName) {
        var soundFile = Array.Find(SoundFiles, sound => sound.Name == soundFileName);

        if (soundFile == null) {
            Debug.LogError($"Sound file with name {soundFileName} not found.");
            return;
        }

        if (!soundFile.AudioSource.isPlaying) {
            Debug.Log($"Given sound file {soundFileName} is not currently playing.");
            return;
        }

        soundFile.AudioSource.Stop();
    }

    /// <summary>
    ///     Pause the given audio file.
    /// </summary>
    public void Pause(string soundFileName) {
        var soundFile = Array.Find(SoundFiles, sound => sound.Name == soundFileName);

        if (soundFile == null) {
            Debug.LogError($"Sound file with name {soundFileName} not found.");
            return;
        }

        if (!soundFile.AudioSource.isPlaying) {
            Debug.Log($"Given sound file {soundFileName} is not currently playing.");
            return;
        }

        soundFile.AudioSource.Pause();
    }
}