using System;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum LogType
{
    DIRECT,
    EVERYONE,
    OTHER
}

public class UIManager : SingletonPun<UIManager>
{
    [SerializeField] private UIPlayerStatus redPlayerStatus;
    [SerializeField] private UIPlayerStatus bluePlayerStatus;

    [SerializeField] private TextMeshProUGUI turnCountText;
    [SerializeField] private TextMeshProUGUI turnOwnerText;

    [SerializeField] private RectTransform turnActParent;

    [Header("Result")] [SerializeField] private RectTransform gameEndParent;

    [SerializeField] private TextMeshProUGUI winText;

    [Header("Log")] [SerializeField] private RectTransform logParent;

    [SerializeField] private TextMeshProUGUI logTextOrigin;
    private readonly List<TextMeshProUGUI> logTexts = new List<TextMeshProUGUI>();

    [Header("Special Card")]
    [SerializeField] private Image specialCardWindow;
    
    [SerializeField] private RectTransform specialCardParent;
    [SerializeField] private UISpecialCard specialCardOrigin;
    
    [SerializeField] private TextMeshProUGUI specialCardName;
    [SerializeField] private TextMeshProUGUI specialCardDescription;

    [SerializeField] private Button specialCardUseButton;
    [SerializeField] private Button specialCardExitButton;
    private SpecialType specialType = SpecialType.RUIN;
    private readonly List<UISpecialCard> specialCards = new List<UISpecialCard>();

    protected override void OnCreated()
    {
        foreach (RectTransform rect in logParent)
            logTexts.Add(rect.GetComponent<TextMeshProUGUI>());
        
        foreach (RectTransform rect in specialCardParent)
            specialCards.Add(rect.GetComponent<UISpecialCard>());
        
        specialCardUseButton.onClick.RemoveAllListeners();
        specialCardUseButton.onClick.AddListener(UseSpecialCard);
        
        specialCardExitButton.onClick.RemoveAllListeners();
        specialCardExitButton.onClick.AddListener(ExitSpecialCard);
    }

    public void LogText(string text, LogType logType = LogType.DIRECT)
    {
        switch (logType)
        {
            default:
            case LogType.DIRECT:
                LogTextRPC(text);
                break;
            case LogType.EVERYONE:
                photonView.RPC(nameof(LogTextRPC), RpcTarget.AllBuffered, text);
                break;
            case LogType.OTHER:
                photonView.RPC(nameof(LogTextRPC), RpcTarget.OthersBuffered, text);
                break;
        }
    }

    public void OpenSpecialCardWindow()
    {
        var player = InGameManager.Instance.player;
        
        specialCardWindow.gameObject.SetActive(true);
        
        specialCardName.gameObject.SetActive(false);
        specialCardDescription.gameObject.SetActive(false);
        specialCardUseButton.gameObject.SetActive(false);
        
        for (int i = 0; i < specialCards.Count; i++)
        {
            if (player.speicalCards.Count > i)
            {
                specialCards[i].gameObject.SetActive(true);
                specialCards[i].SetSpecial(player.speicalCards[i]);
            }
            else
            {
                specialCards[i].gameObject.SetActive(false);
            }
        }
    }

    public void ShowSpecialCard(SpecialType type)
    {
        specialType = type;
        
        specialCardName.gameObject.SetActive(true);
        specialCardDescription.gameObject.SetActive(true);
        specialCardUseButton.gameObject.SetActive(true);
        
        var cardData = ResourceManager.Instance.GetSpecialData(type);
        specialCardName.text = cardData.name;
        specialCardDescription.text = cardData.description;
    }

    private void ExitSpecialCard()
    {
        
    }

    private void UseSpecialCard()
    {
        InGameManager.Instance.UseSpecialCard(specialType);
    }

    [PunRPC]
    private void LogTextRPC(string text)
    {
        var noUseText = logTexts.Find(textMeshProUGUI => textMeshProUGUI != null && !textMeshProUGUI.gameObject.activeSelf);
        if (noUseText == null)
            noUseText = Instantiate(logTextOrigin, logParent);

        noUseText.text = text;
        noUseText.gameObject.SetActive(true);
    }

    public void PlayerSetting(Player player)
    {
        if (player.team == Team.RED)
            redPlayerStatus.SetPlayerName(player.NickName);
        else
            bluePlayerStatus.SetPlayerName(player.NickName);
    }

    public void TurnSetting()
    {
        photonView.RPC(nameof(TurnSettingRPC), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void TurnSettingRPC()
    {
        turnCountText.text = InGameManager.Instance.turnCount.ToString();
        turnOwnerText.text = InGameManager.Instance.GetPlayer(InGameManager.Instance.turnOwner).NickName + "의 턴";
        turnActParent.gameObject.SetActive(InGameManager.Instance.player.team == InGameManager.Instance.turnOwner);
    }

    public void PlayerNotActSetting(Team team, bool notAct)
    {
        if (team == Team.RED)
            redPlayerStatus.SetNotAct(notAct);
        else
            bluePlayerStatus.SetNotAct(notAct);
    }

    public void UpdateCard(Team team, List<int> numberCards, bool secretSkip = false)
    {
        bool secret = secretSkip || team == InGameManager.Instance.player.team;
        if (team == Team.RED)
            redPlayerStatus.SetNumberCard(secret, numberCards);
        else
            bluePlayerStatus.SetNumberCard(secret, numberCards);
    }

    public void GameEnd(Player winner)
    {
        gameEndParent.gameObject.SetActive(true);
        turnActParent.gameObject.SetActive(false);
        winText.text = TeamUtil.GetColoringPlayerName(winner) + "의 승리!";
    }
}