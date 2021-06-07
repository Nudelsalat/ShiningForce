using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.LevelManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartPoint : MonoBehaviour {
    public string sceneFromWhichToCome;
    public int id;

    private FadeInOut _fadeInOut;

    void Start() {
        _fadeInOut = FadeInOut.Instance;
        if (sceneFromWhichToCome.Equals("Default")) {
            FadeIn();
        }
        var lastSceneName = LevelManager.getLastNameLevelString().ToLower();
        var givenId = Inventory.Instance.GetWarpId();
        if (sceneFromWhichToCome.ToLower().Equals(lastSceneName) && (givenId == 0 || givenId == id)) {
            StartCoroutine(Delay(0.01f));
        }
    }

    private void FadeIn() {
        var player = Player.Instance;
        var cursor = Cursor.Instance;
        if (!player || !cursor) {
            Player.InputDisabledInDialogue = true;
            _fadeInOut.FadeIn(2);
            StartCoroutine(WaitForSecEnablePlayer(0.5f));
            return;
        }
        var overViewCamera = OverviewCameraMovement.Instance;
        player.SetPosition(gameObject.transform.position);
        cursor.SetPosition(gameObject.transform.position);
        overViewCamera.transform.position = new Vector3(gameObject.transform.position.x,
            gameObject.transform.position.y, overViewCamera.transform.position.z);
        Player.InputDisabledInDialogue = true;
        _fadeInOut.FadeIn(2);
        StartCoroutine(WaitForSecEnablePlayer(0.5f));
    }

    IEnumerator WaitForSecEnablePlayer(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        Inventory.Instance.ResetWarpId();
        Player.InputDisabledInDialogue = false;
    }
    IEnumerator Delay(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        FadeIn();
    }
}
