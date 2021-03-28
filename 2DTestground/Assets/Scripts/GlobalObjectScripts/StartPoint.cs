using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.SceneManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartPoint : MonoBehaviour {
    public string sceneFromWhichToCome;

    private Player player;
    private OverviewCameraMovement overViewCamere;
    private GameObject movePoint;
    private GameObject _fadeOutScreen;

    // Start is called before the first frame update
    void Awake() {
        var lastSceneName = LevelManager.getLastNameLevelString().ToLower();
        if (!sceneFromWhichToCome.ToLower().Equals(lastSceneName)) {
            return;
        }
        player = FindObjectOfType<Player>();
        movePoint = GameObject.FindGameObjectWithTag("MovePoint");
        overViewCamere = FindObjectOfType<OverviewCameraMovement>();
        player.transform.position = movePoint.transform.position = gameObject.transform.position;
        overViewCamere.transform.position = new Vector3(gameObject.transform.position.x,
            gameObject.transform.position.y, overViewCamere.transform.position.z);
    }
    void Start() {
        _fadeOutScreen = GameObject.Find("FadeOutScreen");
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
