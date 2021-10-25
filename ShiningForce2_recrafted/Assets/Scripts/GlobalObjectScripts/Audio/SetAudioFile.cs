using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GlobalObjectScripts.Audio {
    class SetAudioFile : MonoBehaviour {
        public string AudioFileName = "Town";

        void Start() {
            var audioManager = AudioManager.Instance;
            Debug.Log($"Audiomanager: {audioManager}.");
            var currentlyPlaying = audioManager.GetCurrentTrackName();
            if (AudioFileName.Equals(currentlyPlaying, StringComparison.OrdinalIgnoreCase)) {
                return;
            }
            audioManager.StopAll();
            var length = audioManager.Play(AudioFileName);
            if (length <= 0.1f) {
                audioManager.Play("Town");
            }
        }
    }
}
