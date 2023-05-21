using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class MatchingManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private LoadingText loadingText;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 3;

        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(QuitButton);
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // 연결
    }

    private void QuitButton()
    {
        PhotonNetwork.Disconnect();
        Application.Quit();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = GameManager.Instance.nickName;
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
        
        loadingText.gameObject.SetActive(true);
        loadingText.Text = "로딩 중";
        
        PhotonNetwork.ConnectUsingSettings();
    }
}