using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    TMP_InputField nicknameInput;
    [SerializeField]
    GameObject panel;
    [SerializeField]
    TextMeshProUGUI loadingText;
    [SerializeField]
    TextMeshProUGUI findingEnemyText;
    [SerializeField]
    Button quitButton;
    [SerializeField]
    Button joinButton;
    void Awake()
    {
        SetResoultion();
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 3;
        panel.gameObject.SetActive(true);
        joinButton.gameObject.SetActive(true);
        nicknameInput.gameObject.SetActive(true);
        findingEnemyText.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(false);
        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(new UnityAction(QuitButton));
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(new UnityAction(JoinButton));
    }
    #region 立加包府
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = nicknameInput.text;
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // 立加 角菩矫 规积己
        PhotonNetwork.CreateRoom("", new RoomOptions { MaxPlayers = 2 });
    }
    public override void OnCreatedRoom()
    {
        loadingText.gameObject.SetActive(false);
        findingEnemyText.gameObject.SetActive(true);
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        panel.gameObject.SetActive(false);
        GameManager.Instance.GameStart();
    }
    public override void OnJoinedRoom()
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            panel.gameObject.SetActive(false);
            GameManager.Instance.GameStart();
        }
    }
    void JoinButton()
    {
        joinButton.gameObject.SetActive(false);
        nicknameInput.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(true);
        PhotonNetwork.ConnectUsingSettings();
    }
    void QuitButton()
    {
        Application.Quit();
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        PhotonNetwork.Disconnect();
        panel.gameObject.SetActive(true);
        joinButton.gameObject.SetActive(false);
        nicknameInput.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(true);
        findingEnemyText.gameObject.SetActive(false);
        GameManager.Instance.GameEnd();
        PhotonNetwork.ConnectUsingSettings();
    }
    #endregion
    void SetResoultion()
    {
        int WIDTH = 2000;
        int HEIGHT = 900;

        int DEVICE_WIDTH = Screen.width;
        int DEVICE_HEIGHT = Screen.height;

        float RATIO = (float)WIDTH / HEIGHT;
        float DEVICE_RATIO = (float)DEVICE_WIDTH / DEVICE_HEIGHT;

        Screen.SetResolution(WIDTH, (int)(((float)DEVICE_HEIGHT / DEVICE_WIDTH) * WIDTH), true);

        if (RATIO < DEVICE_RATIO)
        {
            float newWidth = RATIO / DEVICE_RATIO;
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f);
        }
        else
        {
            float newHeight = DEVICE_RATIO / RATIO;
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight);
        }
    }
}
