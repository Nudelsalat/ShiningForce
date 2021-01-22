using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public static bool isInMenu = false;
    public static bool isPause = false;

    public GameObject pauseUI;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Select")) {
            if (isPause) {
                Resume();
            } else {
                Pause();
            }
        }

        if (Input.GetButtonDown("Menu")) {

        }

    }

    void Resume() {
        pauseUI.SetActive(false);
        Time.timeScale = 1f;
        isPause = false;
    }

    void Pause() {
        pauseUI.SetActive(true);
        Time.timeScale = 0f;
        isPause = true;
    }
}
