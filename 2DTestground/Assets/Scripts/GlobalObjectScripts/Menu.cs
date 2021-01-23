using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public static bool isInMenu = false;
    public static bool isPause = false;

    public GameObject pauseUI;
    public GameObject objectMenu;
    public GameObject portrait;

    public Animator animatorInventory;
    public Animator animatorPortrait;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonUp("Select")) {
            if (!isPause) {
                Pause();
            }
        }

        if (Input.GetButtonUp("Menu")) {
            if (!isInMenu) {
                OpenMenu();
            }
            isInMenu = true;
        }
        if (Input.GetButtonUp("Back")) {
            if (isInMenu) {
                CloseMenu();
                isInMenu = false;
            } else if (isPause) {
                Resume();
            }
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

    void OpenMenu() {
        portrait.SetActive(true);
        objectMenu.SetActive(true);

        LoadInventory();
        var image = portrait.transform.GetChild(0).GetComponent<Image>();
        var sprite = Resources.Load<Sprite>("ShiningForce/Images/face/bowie");
        image.sprite = sprite;
        animatorPortrait.SetBool("portraitIsOpen", true);
        animatorInventory.SetBool("inventoryIsOpen", true);
    }

    void CloseMenu() {
        animatorPortrait.SetBool("portraitIsOpen", false);
        animatorInventory.SetBool("inventoryIsOpen", false);
       StartCoroutine(WaitForQuaterSec());
    }

    IEnumerator WaitForQuaterSec() {
        yield return new WaitForSeconds(0.25f);

        objectMenu.SetActive(false);
        portrait.SetActive(false);
        Player.IsInMenu = false;
    }

    void LoadInventory() {
        var gameInventory = Inventory.Instance;
        var gameItem = gameInventory.GetPartyMemberById(0).partyMemberInventory;

        MemberInventoryUI.LoadMemberInventory(gameItem);
    }
}
