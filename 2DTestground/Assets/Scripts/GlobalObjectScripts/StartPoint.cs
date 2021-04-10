using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.SceneManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartPoint : MonoBehaviour {
    public string sceneFromWhichToCome;

    private GameObject _fadeOutScreen;

    // Start is called before the first frame update
    void Awake() {
        if (sceneFromWhichToCome.Equals("Default")) {
            FadeIn();
        }
    }

    void Start() {
        var lastSceneName = LevelManager.getLastNameLevelString().ToLower();
        if (sceneFromWhichToCome.ToLower().Equals(lastSceneName)) {
            FadeIn();
        }
    }

    private void FadeIn() {
        _fadeOutScreen = GameObject.Find("FadeOutScreen");
        var player = Player.Instance;
        var cursor = Cursor.Instance;
        if (!player || !cursor) {
            CanvasGroup canvas = _fadeOutScreen.GetComponent<CanvasGroup>();
            canvas.alpha = 0;
            return;
        }
        var overViewCamera = OverviewCameraMovement.Instance;
        player.SetPosition(gameObject.transform.position);
        cursor.SetPosition(gameObject.transform.position);
        overViewCamera.transform.position = new Vector3(gameObject.transform.position.x,
            gameObject.transform.position.y, overViewCamera.transform.position.z);
        Player.InputDisabledInDialogue = true;
        StartCoroutine(DoFadeIn());
    }

    IEnumerator DoFadeIn() {
        CanvasGroup canvas = _fadeOutScreen.GetComponent<CanvasGroup>();
        canvas.alpha = 1;
        while (canvas.alpha > 0) {
            canvas.alpha -= Time.deltaTime * 2;
            yield return null;
        }
        Player.InputDisabledInDialogue = false;
    }
}
