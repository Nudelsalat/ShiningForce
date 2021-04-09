using System.Collections;
using Assets.Scripts.SceneManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpToScene : MonoBehaviour {

    public string sceneToWarpTo;
    public AudioClip audioClip;
    private GameObject _fadeOutScreen;

    void Start() {
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.tag.Equals("Player")) {
            DoWarp();
        }
    }

    public void DoWarp() {
        if (audioClip != null) {
            AudioSource.PlayClipAtPoint(audioClip, transform.position);
        }
        Player.InputDisabledInDialogue = true;
        LevelManager.setLastLevelInt(SceneManager.GetActiveScene().buildIndex);
        LevelManager.setLastLevelString(SceneManager.GetActiveScene().name);
        _fadeOutScreen = GameObject.Find("FadeOutScreen");
        StartCoroutine(DoFade());
    }

    IEnumerator DoFade() {
        CanvasGroup canvas = _fadeOutScreen.GetComponent<CanvasGroup>();
        canvas.alpha = 0;
        while (canvas.alpha < 1) {
            canvas.alpha += Time.deltaTime * 2;
            yield return null;
        }
        Player.InputDisabledInDialogue = false;
        SceneManager.LoadScene(sceneToWarpTo, LoadSceneMode.Single);
    }
}
