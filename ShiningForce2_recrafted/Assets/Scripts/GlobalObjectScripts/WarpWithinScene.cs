using System.Collections;
using Assets.Scripts.GameData.Trigger;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.LevelManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpWithinScene : MonoBehaviour, IEventTrigger {

    public Transform PointToWarpTo;
    public AudioClip audioClip;

    void Start() {
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Player")) {
            DoWarp();
        }
    }

    public void EventTrigger() {
        DoWarp();
    }

    public void DoWarp() {
        if (audioClip != null) {
            AudioSource.PlayClipAtPoint(audioClip, transform.position);
        }
        FadeInOut.Instance.FadeOutAndThenBackIn(2.5f);
        StartCoroutine(WaitForSecToWarp(0.4f));
    }

    IEnumerator WaitForSecToWarp(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        var player = Player.Instance;
        var cursor = Cursor.Instance;
        var overViewCamera = OverviewCameraMovement.Instance;
        if (player) {
            player.SetPosition(PointToWarpTo.position);
        }
        if (cursor) {
            cursor.SetPosition(PointToWarpTo.position);
        }
        if (overViewCamera) {
            overViewCamera.transform.position = new Vector3(PointToWarpTo.position.x,
                PointToWarpTo.position.y, overViewCamera.transform.position.z);
        }
        Player.InWarp = false;
    }
}
