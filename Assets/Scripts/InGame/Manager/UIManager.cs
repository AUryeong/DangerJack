using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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

    [SerializeField] private TextMeshProUGUI targetValueText;

    [Header("Result")] [SerializeField] private RectTransform gameEndParent;

    [SerializeField] private TextMeshProUGUI winText;

    [Header("Log")] [SerializeField] private RectTransform logParent;

    [SerializeField] private TextMeshProUGUI logTextOrigin;
    private readonly List<TextMeshProUGUI> logTexts = new List<TextMeshProUGUI>();

    [Header("Special Card")] [SerializeField]
    private Image specialCardWindow;

    [SerializeField] private RectTransform specialCardParent;

    [SerializeField] private TextMeshProUGUI specialCardName;
    [SerializeField] private TextMeshProUGUI specialCardDescription;

    [SerializeField] private Button specialCardUseButton;
    [SerializeField] private Button specialCardExitButton;

    [Header("Card Table")] [SerializeField]
    private TextMeshProUGUI tableSealText;

    [SerializeField] private TextMeshProUGUI tableTargetValueText;
    [SerializeField] private TextMeshProUGUI tableResistanceText;
    [SerializeField] private TextMeshProUGUI tableReflectText;

    [Header("Select Card")] [SerializeField]
    private Image selectCardWindow;

    [SerializeField] private TextMeshProUGUI selectCardName;
    [SerializeField] private Button[] selectCardButtons;
    private TextMeshProUGUI[] selectCardTexts;

    private SpecialType specialType = SpecialType.RUIN;
    private readonly List<UISpecialCard> specialCards = new List<UISpecialCard>();

    protected override void OnCreated()
    {
        foreach (RectTransform rect in logParent)
            logTexts.Add(rect.GetComponent<TextMeshProUGUI>());

        foreach (RectTransform rect in specialCardParent)
            specialCards.Add(rect.GetComponent<UISpecialCard>());

        selectCardTexts = selectCardButtons.Select(button => button.transform.GetChild(0).GetComponent<TextMeshProUGUI>()).ToArray();

        specialCardUseButton.onClick.RemoveAllListeners();
        specialCardUseButton.onClick.AddListener(UseSpecialCard);

        specialCardExitButton.onClick.RemoveAllListeners();
        specialCardExitButton.onClick.AddListener(ExitSpecialCard);

        specialCardWindow.gameObject.SetActive(false);
        selectCardWindow.gameObject.SetActive(false);
        gameEndParent.gameObject.SetActive(false);
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
            if (player.specialCards.Count > i)
            {
                specialCards[i].gameObject.SetActive(true);
                specialCards[i].SetSpecial(player.specialCards[i]);
            }
            else
            {
                specialCards[i].gameObject.SetActive(false);
            }
        }
    }

    public void OpenSelectCardWindow(SpecialType type, string[] selectNames, Action[] actions)
    {
        selectCardWindow.gameObject.SetActive(true);

        specialCardName.text = ResourceManager.Instance.GetSpecialData(type).name;
        for (int i = 0; i < selectCardButtons.Length; i++)
        {
            if (selectNames.Length > i)
            {
                selectCardButtons[i].gameObject.SetActive(true);
                
                var temp = i;
                selectCardButtons[i].onClick.RemoveAllListeners();
                selectCardButtons[i].onClick.AddListener(() =>
                {
                    actions[temp].Invoke();
                    ExitSelectCard();
                });
                
                selectCardTexts[i].text = selectNames[i];
            }
            else
            {
                selectCardButtons[i].gameObject.SetActive(false);
            }
        }
    }
    private void ExitSelectCard()
    {
        selectCardWindow.gameObject.SetActive(false);
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
        specialCardWindow.gameObject.SetActive(false);
    }

    private void UseSpecialCard()
    {
        ExitSpecialCard();
        InGameManager.Instance.UseSpecialCard(specialType);
    }

    [PunRPC]
    private void LogTextRPC(string text)
    {
        var noUseText = logTexts.Find(textMeshProUGUI => textMeshProUGUI != null && !textMeshProUGUI.gameObject.activeSelf);
        if (noUseText == null)
        {
            noUseText = Instantiate(logTextOrigin, logParent);
            logTexts.Add(noUseText);
        }

        noUseText.gameObject.SetActive(true);

        noUseText.text = text;
        noUseText.transform.SetAsFirstSibling();
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
        turnCountText.text = InGameManager.Instance.turnCount + "번째 턴";
        turnOwnerText.text = TeamUtil.GetColoringPlayerName(InGameManager.Instance.GetPlayer(InGameManager.Instance.turnOwner)) + "의 턴";
        turnActParent.gameObject.SetActive(InGameManager.Instance.player.team == InGameManager.Instance.turnOwner);
    }

    public void PlayerNotActSetting(Team team, bool notAct)
    {
        if (team == Team.RED)
            redPlayerStatus.SetNotAct(notAct);
        else
            bluePlayerStatus.SetNotAct(notAct);
    }

    public void UpdateCard(Team team, List<int> numberCards, GhostCard ghostCard, bool secretSkip = false)
    {
        bool secret = secretSkip || team == InGameManager.Instance.player.team;
        if (team == Team.RED)
            redPlayerStatus.SetNumberCard(secret, numberCards, ghostCard);
        else
            bluePlayerStatus.SetNumberCard(secret, numberCards, ghostCard);
    }

    public void GameEnd(Player winner)
    {
        gameEndParent.gameObject.SetActive(true);
        turnActParent.gameObject.SetActive(false);
        winText.text = TeamUtil.GetColoringPlayerName(winner) + "의 승리!";
    }

    public void UpdateCardTable()
    {
        tableReflectText.gameObject.SetActive(false);
        tableResistanceText.gameObject.SetActive(false);
        tableSealText.gameObject.SetActive(false);
        tableTargetValueText.gameObject.SetActive(false);

        foreach (var cardTable in InGameManager.Instance.GetCardTables())
        {
            Color color = TeamUtil.TeamToColor(cardTable.owner);
            switch (cardTable.type)
            {
                case SpecialType.TARGET_24:
                    tableTargetValueText.gameObject.SetActive(true);
                    tableTargetValueText.text = "24 목표";
                    tableTargetValueText.color = color;
                    break;
                case SpecialType.TARGET_27:
                    tableTargetValueText.gameObject.SetActive(true);
                    tableTargetValueText.text = "27 목표";
                    tableTargetValueText.color = color;
                    break;
                case SpecialType.SEAL:
                    tableSealText.gameObject.SetActive(true);
                    tableSealText.color = color;
                    break;
                case SpecialType.RESISTANCE:
                    tableResistanceText.gameObject.SetActive(true);
                    tableResistanceText.color = color;
                    break;
                case SpecialType.REFLECT:
                    tableReflectText.gameObject.SetActive(true);
                    tableReflectText.color = color;
                    break;
            }
        }

        targetValueText.text = $"<size=60%>목표 숫자</size>\n{InGameManager.Instance.TargetValue}";
    }
}