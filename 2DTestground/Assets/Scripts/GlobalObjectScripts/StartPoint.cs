using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.SceneManager;
using UnityEngine;

public class StartPoint : MonoBehaviour {
    public string sceneFromWhichToCome;

    private PlayerMovement player;
    private OverviewCameraMovement overViewCamere;
    private GameObject movePoint;

    // Start is called before the first frame update
    void Awake() {
        var lastSceneName = LevelManager.getLastNameLevelString().ToLower();
        if (!sceneFromWhichToCome.ToLower().Equals(lastSceneName)) {
            return;
        }
        player = FindObjectOfType<PlayerMovement>();
        movePoint = GameObject.FindGameObjectWithTag("MovePoint");
        overViewCamere = FindObjectOfType<OverviewCameraMovement>();
        player.transform.position = movePoint.transform.position = gameObject.transform.position;
        overViewCamere.transform.position = new Vector3(gameObject.transform.position.x,
            gameObject.transform.position.y, overViewCamere.transform.position.z);
    }
}
