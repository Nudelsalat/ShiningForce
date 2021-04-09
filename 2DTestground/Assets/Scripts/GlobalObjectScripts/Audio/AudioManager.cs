using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour  {
    public SoundFile[] SoundFiles;

    private List<SoundFile> _pausedSoundFiles = new List<SoundFile>();
    private float _volume = 0.2f;
    private float _SFXvolume = 0.6f;
    private AudioSource _audioSource;

    public static AudioManager Instance;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }
        else {
            Instance = this;
        }

        _audioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        if (SoundFiles.Length == 0) {
            Debug.Log("There are no sound files.");
            return;
        }

        foreach (var soundFile in SoundFiles) {
            soundFile.AudioSource = gameObject.AddComponent<AudioSource>();
            soundFile.AudioSource.clip = soundFile.AudioClip;
            soundFile.AudioSource.volume = soundFile.Volume *= _volume;
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
    public void PlaySFX(AudioClip audioClip) {
        _audioSource.PlayOneShot(audioClip, _SFXvolume);
    }

    /// <summary>
    ///     Play the given audio file.
    /// </summary>
    public float Play(string soundFileName, bool loop = true) {
        var soundFile = Array.Find(SoundFiles, sound => sound.Name == soundFileName);

        if (soundFile == null) {
            Debug.Log($"Sound file with name {soundFileName} not found.");
            return 0f;
        }


        soundFile.AudioSource.loop = loop;
        //TODO need to change the volume of the Themes... way to loud
        //soundFile.AudioSource.volume = _volume;
        soundFile.AudioSource.Play();
        return soundFile.AudioClip.length;
    }

    public bool IsPlaying(string soundFileName) {
        var soundFile = Array.Find(SoundFiles, sound => sound.Name == soundFileName);

        if (soundFile == null) {
            Debug.Log($"Sound file with name {soundFileName} not found.");
            return false;
        }

        return soundFile.AudioSource.isPlaying;
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

        _pausedSoundFiles.Add(soundFile);
        soundFile.AudioSource.Pause();
    }

    public void UnPause(string soundFileName) {
        var soundFile = _pausedSoundFiles.Find(sound => sound.Name == soundFileName);

        if (soundFile == null) {
            Debug.LogError($"Sound file with name {soundFileName} not found.");
            return;
        }
        if (soundFile.AudioSource.isPlaying) {
            Debug.Log($"Given sound file {soundFileName} is currently playing.");
            _pausedSoundFiles.Remove(soundFile);
            return;
        }

        _pausedSoundFiles.Remove(soundFile);
        soundFile.AudioSource.UnPause();
    }

    public void PauseAll() {
        var soundFiles = Array.FindAll(SoundFiles, sound => sound.AudioSource.isPlaying);

        if (soundFiles.Length <= 0) {
            Debug.Log($"No Soundfile to pause");
            return;
        }
        _pausedSoundFiles.AddRange(soundFiles);
        foreach (var file in soundFiles) {
            file.AudioSource.Pause();
        }
    }

    public void UnPauseAll() {
        if (_pausedSoundFiles.Count <= 0) {
            Debug.LogError($"No Audiofiles are paused.");
            return;
        }

        foreach (var file in _pausedSoundFiles) {
            file.AudioSource.UnPause();
        }
        _pausedSoundFiles.Clear();
    }
}