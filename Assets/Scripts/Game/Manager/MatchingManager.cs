using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class MatchingManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private GameObject panel;
    [SerializeField] private LoadingText loadingText;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button joinButton;

    void Awake()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 3;

        panel.gameObject.SetActive(true);
        joinButton.gameObject.SetActive(true);
        nicknameInput.gameObject.SetActive(true);
        
        loadingText.gameObject.SetActive(false);

        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(QuitButton);

        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(JoinButton);
    }

    private void JoinButton()
    {
        joinButton.gameObject.SetActive(false);
        nicknameInput.gameObject.SetActive(false);
        
        loadingText.gameObject.SetActive(true);
        loadingText.Text = "로딩 중";
        
        PhotonNetwork.ConnectUsingSettings(); // 연결
    }

    private void QuitButton()
    {
        Application.Quit();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = nicknameInput.text;
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // 접속 실패시 방생성
        PhotonNetwork.CreateRoom("", new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnCreatedRoom()
    {
        loadingText.Text = "상대방을 찾는 중";
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        GameManager.Instance.LoadScene(SceneType.INGAME);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            GameManager.Instance.LoadScene(SceneType.INGAME);
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        PhotonNetwork.Disconnect();
        
        joinButton.gameObject.SetActive(false);
        nicknameInput.gameObject.SetActive(false);
        
        panel.gameObject.SetActive(true);
        loadingText.gameObject.SetActive(true);
        loadingText.Text = "로딩 중";
        
        PhotonNetwork.ConnectUsingSettings();
    }
}