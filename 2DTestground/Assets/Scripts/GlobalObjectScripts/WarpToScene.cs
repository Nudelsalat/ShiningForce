using System.Collections;
using Assets.Scripts.SceneManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpToScene : MonoBehaviour {

    public string sceneToWarpTo;
    public AudioClip audioClip;

    void Start() {
        GameObject.Find("FadeOutScreen").GetComponent<CanvasGroup>().alpha = 0;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.tag.Equals("Player")) {
            if (audioClip != null) {
                AudioSource.PlayClipAtPoint(audioClip, transform.position);
            }
            Player.InputDisabledInDialogue = true;
            LevelManager.setLastLevelInt(SceneManager.GetActiveScene().buildIndex);
            LevelManager.setLastLevelString(SceneManager.GetActiveScene().name);
            StartCoroutine(DoFade());
        }
    }
    IEnumerator DoFade() {
        CanvasGroup canvas = GameObject.Find("FadeOutScreen").GetComponent<CanvasGroup>();
        canvas.alpha = 0;
        while (canvas.alpha < 1) {
            canvas.alpha += Time.deltaTime * 2;
            yield return null;
        }
        Player.InputDisabledInDialogue = false;
        SceneManager.LoadScene(sceneToWarpTo, LoadSceneMode.Single);
    }
}
