using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Transformation;

public class MainMenuBtn : MonoBehaviour
{
    private RectTransform btnRectTransform;
    public GameObject root;
    private Animator mainMenuAnimator;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Awake()
    {
        btnRectTransform = GetComponent<RectTransform>();
        mainMenuAnimator = root.GetComponent<Animator>();
    }

    public void scaleDown()
    {
        // Scale the button to 0.8 when clicked
        btnRectTransform.localScale = new Vector3(0.8f, 0.8f, 1f);
    }

    public void scaleUp()
    {
        // Reset the button scale to 1.0 when released
        btnRectTransform.localScale = new Vector3(1.0f, 1.0f, 1f);
    }

    public void setNameOK()
    {
        LobbyManager.Instance.Authenticate(EditPlayerName.Instance.GetPlayerName());
        gameObject.transform.parent.gameObject.SetActive(false);
    }

    public void createGameClicked()
    {
        mainMenuAnimator.SetBool("CreateClicked", true);
    }

    public void createGameBack()
    {
        mainMenuAnimator.SetBool("CreateClicked", false);
    }

    public void joinGameClicked()
    {
        mainMenuAnimator.SetBool("JoinClicked", true);
    }

    public void joinGameBack()
    {
        mainMenuAnimator.SetBool("JoinClicked", false);
    }

    public void joinLobbyBack()
    {
        mainMenuAnimator.SetBool("JoinLobbyClicked", false);
    }

    public void joinedLobbyBack()
    {
        mainMenuAnimator.SetBool("LobbyCreated", false);
        LobbyManager.Instance.LeaveLobby();
    }

    public void startGame()
    {
        root.transform.GetChild(6).GetChild(11).GetComponent<Button>().interactable = false;
        LobbyManager.Instance.StartGame();
    }

    public void lobbyNotFoundBack()
    {
        mainMenuAnimator.SetBool("InvalidCode", false);
    }

    public void settingClicked()
    {
        mainMenuAnimator.SetBool("SettingClicked", true);
    }

    public void settingBack()
    {
        mainMenuAnimator.SetBool("SettingClicked", false);
    }

    public void pauseSettingClicked()
    {
        mainMenuAnimator.SetBool("PauseSettingClicked", true);
    }

    public void pauseSettingBack()
    {
        mainMenuAnimator.SetBool("PauseSettingClicked", false);
    }

    public void hidePause()
    {
        root.SetActive(false);
        root.transform.parent.GetChild(1).GetChild(1).gameObject.SetActive(true);
        root.transform.parent.GetChild(1).GetChild(2).gameObject.SetActive(true);
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void leaveGame()
    {
        if (LobbyManager.Instance.IsLobbyHost() || NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Loading", LoadSceneMode.Single);
            LobbyManager.Instance.DeleteLobby();
            //NetworkManager.Singleton.Shutdown();
            StartCoroutine(Wait(0.5f));
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            LobbyManager.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }
        //else
        //{
        //    LobbyManager.Instance.LeaveLobby();
        //    SceneManager.LoadScene(0, LoadSceneMode.Single);
        //}
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void endGame()
    {
        if (LobbyManager.Instance.IsLobbyHost() || NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Loading", LoadSceneMode.Single);
            LobbyManager.Instance.DeleteLobby();
            //NetworkManager.Singleton.Shutdown();
            StartCoroutine(Wait(0.5f));
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            LobbyManager.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void exit()
    {
        PlayerPrefs.DeleteKey("AvatarNo"); // Clear saved avatar
        PlayerPrefs.Save();
        Application.Quit();
    }

    IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        NetworkManager.Singleton.Shutdown();
    }

    //XR Main Menu
    public void CreateGameClickedXR()
    {
        root.transform.GetChild(3).gameObject.SetActive(true);
        transform.parent.gameObject.SetActive(false);
    }

    public void JoinGameClickedXR()
    {
        root.transform.GetChild(4).gameObject.SetActive(true);
        transform.parent.gameObject.SetActive(false);
    }

    public void SettingsClickedXR()
    {
        root.transform.GetChild(2).gameObject.SetActive(true);
        transform.parent.gameObject.SetActive(false);
    }

    public void SettingsBackXR()
    {
        root.transform.GetChild(0).gameObject.SetActive(true);
        transform.parent.gameObject.SetActive(false);
    }

    public void CreateGameBackXR()
    {
        root.transform.GetChild(0).gameObject.SetActive(true);
        transform.parent.gameObject.SetActive(false);
    }

    public void JoinGameBackXR()
    {
        root.transform.GetChild(0).gameObject.SetActive(true);
        transform.parent.gameObject.SetActive(false);
    }

    public void JoinLobbyBackXR()
    {
        root.transform.GetChild(4).gameObject.SetActive(true);
        transform.parent.gameObject.SetActive(false);
    }

    public void JoinedLobbyBackXR()
    {
        root.transform.GetChild(0).gameObject.SetActive(true);
        transform.parent.gameObject.SetActive(false);
        LobbyManager.Instance.LeaveLobby();
    }

    public void LobbyNotFoundBackXR()
    {
        transform.parent.gameObject.SetActive(false);
    }

    public void PauseSettingClickedXR()
    {
        gameObject.transform.parent.gameObject.SetActive(false);
        root.transform.GetChild(1).gameObject.SetActive(true);
    }

    public void PauseSettingBackXR()
    {
        gameObject.transform.parent.gameObject.SetActive(false);
        root.transform.GetChild(0).gameObject.SetActive(true);
    }
}
