using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour  {
    public SoundFile[] SoundFiles;

    private readonly List<SoundFile> _sfxFiles = new List<SoundFile>();

    private readonly List<SoundFile> _pausedSoundFiles = new List<SoundFile>();
    private float _volume = 0.4f;
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

        LoadSfxFiles();
    }

    private void LoadSfxFiles() {
        _sfxFiles.Add(new SoundFile() {
            AudioClip = Resources.Load<AudioClip>(Constants.SoundMenuSwish),
            Name = Constants.SfxMenuSwish,
            Volume = 1,
            Loop = false,
            Pitch = 1,
        });
        _sfxFiles.Add(new SoundFile() {
            AudioClip = Resources.Load<AudioClip>(Constants.SoundMenuDing),
            Name = Constants.SfxMenuDing,
            Volume = 1,
            Loop = false,
            Pitch = 1,
        });
        _sfxFiles.Add(new SoundFile() {
            AudioClip = Resources.Load<AudioClip>(Constants.SoundError),
            Name = Constants.SfxError,
            Volume = 1,
            Loop = false,
            Pitch = 1,
        });
        _sfxFiles.Add(new SoundFile() {
            AudioClip = Resources.Load<AudioClip>(Constants.SoundExplosion),
            Name = Constants.SfxExplosion,
            Volume = 1,
            Loop = false,
            Pitch = 1,
        });
        _sfxFiles.Add(new SoundFile() {
            AudioClip = Resources.Load<AudioClip>(Constants.SoundMovement),
            Name = Constants.SfxMovement,
            Volume = 1,
            Loop = false,
            Pitch = 1,
        });
        _sfxFiles.Add(new SoundFile() {
            AudioClip = Resources.Load<AudioClip>(Constants.SoundHit),
            Name = Constants.SfxHit,
            Volume = 1,
            Loop = false,
            Pitch = 1,
        });
    }

    private void Start() {
        if (SoundFiles.Length == 0) {
            Debug.Log("There are no sound files.");
            return;
        }

        foreach (var soundFile in SoundFiles) {
            soundFile.AudioSource = gameObject.AddComponent<AudioSource>();
            soundFile.AudioSource.clip = soundFile.AudioClip;
            soundFile.AudioSource.volume = _volume;
            soundFile.AudioSource.pitch = soundFile.Pitch;
            soundFile.AudioSource.loop = soundFile.Loop;
            soundFile.AudioSource.outputAudioMixerGroup = FindObjectOfType<AudioMixerGroup>();
        }

        Instance.SoundFiles = SoundFiles;

        Play("Town");
    }


    public void PlaySFX(string name) {
        var soundFile = _sfxFiles.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (soundFile == null) {
            Debug.Log($"Sfx with name {name} not found.");
            return;
        }
        PlaySFX(soundFile.AudioClip);
    }

    public void PlaySFX(AudioClip audioClip) {
        _audioSource.PlayOneShot(audioClip, _SFXvolume);
    }

    /// <summary>
    ///     Play the given audio file.
    /// </summary>
    public float Play(string soundFileName, bool loop = true, float playWithVolume = 0f) {
        var soundFile = Array.Find(SoundFiles, sound => sound.Name.Equals(soundFileName, StringComparison.OrdinalIgnoreCase));

        if (soundFile == null) {
            Debug.Log($"Sound file with name {soundFileName} not found.");
            return 0f;
        }

        soundFile.AudioSource.loop = loop;
        soundFile.AudioSource.volume = playWithVolume > 0 ? playWithVolume : _volume;
        soundFile.AudioSource.Play();
        return soundFile.AudioClip.length;
    }

    public bool IsPlaying(string soundFileName) {
        var soundFile = Array.Find(SoundFiles, sound => sound.Name.Equals(soundFileName, StringComparison.OrdinalIgnoreCase));

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
        var soundFile = Array.Find(SoundFiles, sound => sound.Name.Equals(soundFileName, StringComparison.OrdinalIgnoreCase));

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
        var soundFile = Array.Find(SoundFiles, sound => sound.Name.Equals(soundFileName, StringComparison.OrdinalIgnoreCase));

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
        var soundFile = _pausedSoundFiles.Find(sound => sound.Name.Equals(soundFileName, StringComparison.OrdinalIgnoreCase));

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

    public float GetSFXVolume() {
        return _SFXvolume;
    }

    public float GetMasterVolume() {
        return _volume;
    }
}