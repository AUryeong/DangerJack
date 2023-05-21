using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NamingManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button joinButton;

    private bool isJoining;

    private void Awake()
    {
        joinButton.gameObject.SetActive(true);
        nicknameInput.gameObject.SetActive(true);

        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(QuitButton);

        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(JoinButton);
    }

    private void Start()
    {
        nicknameInput.text = GameManager.Instance.nickName;
    }

    private void JoinButton()
    {
        if (isJoining) return;
        if (string.IsNullOrEmpty(nicknameInput.text) || string.IsNullOrWhiteSpace(nicknameInput.text)) return;

        isJoining = true;
        GameManager.Instance.nickName = nicknameInput.text;
        GameManager.Instance.LoadScene(SceneType.MATCHING);
    }

    private void QuitButton()
    {
        if (isJoining) return;

        Application.Quit();
    }
}