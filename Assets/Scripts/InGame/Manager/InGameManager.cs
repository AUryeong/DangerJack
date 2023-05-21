using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using Random = UnityEngine.Random;

public enum Team
{
    NONE,
    RED,
    BLUE
}

public class CardTable
{
    public readonly Team owner;
    public readonly SpecialType type;

    public CardTable(Team owner, SpecialType type)
    {
        this.owner = owner;
        this.type = type;
    }
}

public class InGameManager : SingletonPunCallBack<InGameManager>
{
    public bool IsGaming { get; private set; }
    public int turnCount = 1;
    public Player player;
    public Player enemyPlayer;

    private bool master;
    private bool isTurnNoDraw;
    private bool isEnemyTurnNoDraw;
    public bool isGameStart;
    public Team turnOwner = Team.RED;

    private readonly List<CardTable> cardTables = new List<CardTable>();

    private int alchemyCount;

    public int TargetValue
    {
        get
        {
            foreach (var cardTable in cardTables)
            {
                if (cardTable.type == SpecialType.TARGET_24)
                    return 24;
                if (cardTable.type == SpecialType.TARGET_27)
                    return 27;
            }

            return 21;
        }
    }

    public bool IsTurnMine => turnOwner == player.team;

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (!IsGaming) return;

        ExitGameMatching();
    }

    public List<CardTable> GetCardTables()
    {
        return new List<CardTable>(cardTables);
    }

    private void OnApplicationQuit()
    {
        DisconnectGameRPC();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            DisconnectGameRPC();
    }

    private void ExitGameMatching()
    {
        GameManager.Instance.LoadScene(SceneType.MATCHING);
    }

    private void ExitGameNaming()
    {
        GameManager.Instance.LoadScene(SceneType.NAMING);
    }

    [PunRPC]
    private void GameEndRPC()
    {
        IsGaming = false;
        photonView.RPC(nameof(DisconnectGameRPC), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void DisconnectGameRPC()
    {
        PhotonNetwork.Disconnect();
    }

    private void Start()
    {
        if (!photonView.IsMine) return;

        master = true;
        Team team = (Team)Random.Range(1, 3);

        photonView.RPC(nameof(PlayerCreate), RpcTarget.OthersBuffered, TeamUtil.OtherTeam(team));
        PlayerCreate(team);
    }

    private void Update()
    {
        if (!master) return;

        CheckLoading();
    }

    [PunRPC]
    private void PlayerCreate(Team team)
    {
        player = PhotonNetwork.Instantiate(nameof(Player), Vector3.zero, Quaternion.identity).GetComponent<Player>();
        player.SetTeam(team);
    }


    private void CheckLoading()
    {
        if (isGameStart) return;

        if (player != null & enemyPlayer != null)
        {
            IsGaming = true;
            isGameStart = true;

            Init();
        }
    }

    private void Init()
    {
        if (!IsGaming) return;

        DeckManager.Instance.Init();
        UIManager.Instance.LogText("���� ����", LogType.EVERYONE);

        player.DrawNumberCard();
        player.DrawNumberCard();

        enemyPlayer.DrawNumberCard();
        enemyPlayer.DrawNumberCard();

        GetPlayer(Team.RED).DrawSpecialCard(2);
        photonView.RPC(nameof(InitRPC), RpcTarget.AllBuffered);

        string coloringName = TeamUtil.GetColoringPlayerName(GetPlayer(Team.RED));
        UIManager.Instance.LogText($"{coloringName}�� ��", LogType.EVERYONE);

        UIManager.Instance.TurnSetting();
    }

    [PunRPC]
    private void InitRPC()
    {
        IsGaming = true;
        isTurnNoDraw = true;

        turnCount = 1;
        turnOwner = Team.RED;
    }

    public void TurnChange(bool isSkipCheckDraw = false)
    {
        if (!IsTurnMine) return;
        if (!IsGaming) return;

        bool temp = isTurnNoDraw && !isSkipCheckDraw;
        photonView.RPC(nameof(TurnChangeRPC), RpcTarget.AllBuffered, temp);
    }

    [PunRPC]
    private void TurnChangeRPC(bool isTurnSkipPrev)
    {
        Team team = turnOwner;
        if (isEnemyTurnNoDraw && isTurnSkipPrev)
        {
            Player winner = null;
            int targetNumber = TargetValue;
            UIManager.Instance.LogText("���� ����!");

            if (player.GetSum() > targetNumber && enemyPlayer.GetSum() > targetNumber)
            {
                if (player.GetSum() == enemyPlayer.GetSum())
                    winner = GetPlayer(Team.RED);
                else if (player.GetSum() > enemyPlayer.GetSum())
                    winner = enemyPlayer;
                else
                    winner = player;
            }
            else
            {
                if (player.GetSum() > targetNumber)
                {
                    if (enemyPlayer.GetSum() <= targetNumber)
                        winner = enemyPlayer;
                }
                else if (enemyPlayer.GetSum() > targetNumber)
                {
                    winner = player;
                }
                else
                {
                    if (player.GetSum() == enemyPlayer.GetSum())
                        winner = GetPlayer(Team.RED);
                    else if (player.GetSum() < enemyPlayer.GetSum())
                        winner = enemyPlayer;
                    else
                        winner = player;
                }
            }

            UIManager.Instance.UpdateCard(player.team, player.numberCards, player.GhostCard, true);
            UIManager.Instance.UpdateCard(enemyPlayer.team, enemyPlayer.numberCards, enemyPlayer.GhostCard, true);
            UIManager.Instance.LogText(TeamUtil.GetColoringPlayerName(winner) + "�� �¸�!");
            UIManager.Instance.GameEnd(winner);

            IsGaming = false;
            photonView.RPC(nameof(GameEndRPC), RpcTarget.OthersBuffered);
            return;
        }

        UIManager.Instance.PlayerNotActSetting(team, isTurnSkipPrev);
        UIManager.Instance.PlayerNotActSetting(TeamUtil.OtherTeam(team), false);

        turnOwner = TeamUtil.OtherTeam(team);
        turnCount++;
        UIManager.Instance.LogText($"{TeamUtil.GetColoringPlayerName(GetPlayer(turnOwner))}�� {turnCount}��° ��");

        if (isTurnSkipPrev)
        {
            UIManager.Instance.LogText($"{TeamUtil.GetColoringPlayerName(GetPlayer(team))}�� <color=#8B0000>���� ī�带 ���� �ʾҽ��ϴ�.</color>");
        }

        if (IsCardTableSetting(SpecialType.SEAL, turnOwner))
        {
            RemoveCardTableRPC(SpecialType.SEAL);
        }

        if (IsCardTableSetting(SpecialType.REFLECT, turnOwner))
        {
            RemoveCardTableRPC(SpecialType.REFLECT);
        }

        isEnemyTurnNoDraw = isTurnSkipPrev;
        isTurnNoDraw = true;

        if (master)
            GetPlayer(turnOwner).DrawSpecialCard(2);

        UIManager.Instance.TurnSetting();
    }

    public void DrawNumber()
    {
        if (!IsTurnMine) return;
        if (!isTurnNoDraw)
        {
            UIManager.Instance.LogText("�̹� �� �̹� ���� ī�带 �̾ҽ��ϴ�.");
            return;
        }

        if (DeckManager.Instance.IsNumberDeckEmpty())
        {
            UIManager.Instance.LogText("���� ī�� ���� ����ֽ��ϴ�.");
            return;
        }

        isTurnNoDraw = false;
        photonView.RPC(nameof(DrawNumberRPC), RpcTarget.OthersBuffered);
        player.DrawNumberCard();
    }

    public void ResetCardTable(Team team = Team.NONE)
    {
        photonView.RPC(nameof(ResetCardTableRPC), RpcTarget.AllBuffered, team);
    }

    [PunRPC]
    private void ResetCardTableRPC(Team team)
    {
        if (team == Team.NONE)
        {
            foreach (var cardTable in cardTables)
                UIManager.Instance.LogText($"<color=#FFEE00>{ResourceManager.Instance.GetSpecialData(cardTable.type).name}</color> ī�尡 ���� ���̺��� �ı��Ǿ����ϴ�.");

            cardTables.Clear();
        }
        else
        {
            foreach (var cardTable in cardTables)
            {
                if (cardTable.owner == team)
                    UIManager.Instance.LogText($"<color=#FFEE00>{ResourceManager.Instance.GetSpecialData(cardTable.type).name}</color> ī�尡 ���� ���̺��� �ı��Ǿ����ϴ�.");
            }

            cardTables.RemoveAll((cardTable) => cardTable.owner == team);
        }

        UIManager.Instance.UpdateCardTable();
    }

    public void RemoveCardTable(SpecialType type)
    {
        photonView.RPC(nameof(RemoveCardTableRPC), RpcTarget.AllBuffered, type);
    }

    [PunRPC]
    private void RemoveCardTableRPC(SpecialType type)
    {
        var cardTable = cardTables.Find((table) => table.type == type);
        if (cardTable != null)
            cardTables.Remove(cardTable);

        UIManager.Instance.LogText($"<color=#FFEE00>{ResourceManager.Instance.GetSpecialData(type).name}</color> ī�尡 ���� ���̺��� �ı��Ǿ����ϴ�.");
        UIManager.Instance.UpdateCardTable();
    }

    public void AddCardTable(Team owner, SpecialType type)
    {
        photonView.RPC(nameof(AddCardTableRPC), RpcTarget.AllBuffered, owner, type);
    }

    [PunRPC]
    private void AddCardTableRPC(Team owner, SpecialType type)
    {
        cardTables.Add(new CardTable(owner, type));
        UIManager.Instance.LogText($"<color=#FFEE00>{ResourceManager.Instance.GetSpecialData(type).name}</color> ī�尡 ���� ���̺� �߰��Ǿ����ϴ�.");
        UIManager.Instance.UpdateCardTable();
    }

    private bool IsCardTableSetting(SpecialType type, Team tableOwner = Team.NONE)
    {
        return cardTables.Exists((cardTable) => cardTable.type == type && (tableOwner == Team.NONE || cardTable.owner == tableOwner));
    }

    [PunRPC]
    private void DrawNumberRPC()
    {
        isTurnNoDraw = false;
    }

    public Player GetPlayer(Team team)
    {
        return player.team == team ? player : enemyPlayer;
    }

    public void UseSpecialCard(SpecialType specialType)
    {
        var owner = player;
        var target = enemyPlayer;
        if (alchemyCount > 0)
        {
            alchemyCount--;
            player.ReturnSpecialCard(specialType);
            UIManager.Instance.LogText($"<color=#FFEE00>{ResourceManager.Instance.GetSpecialData(specialType).name}</color>�� ������ �������½��ϴ�.");
            if (alchemyCount <= 0)
                player.DrawSpecialCard(3);
            else
                UIManager.Instance.OpenSpecialCardWindow(true);
            return;
        }

        if (IsCardTableSetting(SpecialType.SEAL, enemyPlayer.team))
        {
            switch (specialType)
            {
                case SpecialType.RUIN:
                case SpecialType.DESTROY:
                    break;
                default:
                    UIManager.Instance.LogText($"<color=#FFEE00>{ResourceManager.Instance.GetSpecialData(SpecialType.SEAL).name}</color>�� ȿ���� ���� ��� �Ұ����մϴ�!");
                    return;
            }
        }

        if (IsCardTableSetting(SpecialType.RESISTANCE, enemyPlayer.team))
        {
            switch (specialType)
            {
                case SpecialType.ATTACK:
                case SpecialType.CHANGE:
                case SpecialType.MERCY:
                case SpecialType.RECALL:
                case SpecialType.DEPRIVATION:
                    UIManager.Instance.LogText($"<color=#FFEE00>{ResourceManager.Instance.GetSpecialData(SpecialType.RESISTANCE).name}</color>�� ȿ���� ���� ��� �Ұ����մϴ�!");
                    return;
            }
        }

        if (specialType == SpecialType.ALCHEMY)
        {
            if (owner.specialCards.Count <= 2)
            {
                UIManager.Instance.LogText($"<color=#FFEE00>{ResourceManager.Instance.GetSpecialData(specialType).name}</color>�� ����ҷ��� 2���� ����� ī�尡 �ʿ��մϴ�!");
                return;
            }
        }

        player.RemoveSpecialCard(specialType);

        var specialData = ResourceManager.Instance.GetSpecialData(specialType);
        string coloringName = TeamUtil.GetColoringPlayerName(player);
        UIManager.Instance.LogText($"{coloringName}�� ����� ī�� <color=#FFEE00>{specialData.name}</color>�� ����߽��ϴ�", LogType.EVERYONE);

        if (IsCardTableSetting(SpecialType.REFLECT, enemyPlayer.team))
        {
            switch (specialType)
            {
                case SpecialType.ATTACK:
                case SpecialType.DEPRIVATION:
                case SpecialType.MERCY:
                case SpecialType.SEAL:
                    target = player;
                    owner = enemyPlayer;
                    UIManager.Instance.LogText("<color=#FFEE00>�ݻ��� ȿ��</color>�� ���� ����� ��������ϴ�.", LogType.EVERYONE);
                    break;
            }
        }

        switch (specialType)
        {
            case SpecialType.THREE_CARD:
                owner.DrawNumberCardSpecial(3);
                break;
            case SpecialType.FOUR_CARD:
                owner.DrawNumberCardSpecial(4);
                break;
            case SpecialType.FIVE_CARD:
                owner.DrawNumberCardSpecial(5);
                break;
            case SpecialType.SIX_CARD:
                owner.DrawNumberCardSpecial(6);
                break;
            case SpecialType.DECK_DRAW:
                owner.DrawNumberCard();
                break;
            case SpecialType.MIN_CARD:
                if (DeckManager.Instance.IsNumberDeckEmpty())
                {
                    UIManager.Instance.LogText("���� ī�� ���� ����ֽ��ϴ�.");
                    break;
                }

                var minCard = DeckManager.Instance.GetNumberDecks().Min();
                owner.DrawNumberCardSpecial(minCard);
                break;
            case SpecialType.MAX_CARD:
                if (DeckManager.Instance.IsNumberDeckEmpty())
                {
                    UIManager.Instance.LogText("���� ī�� ���� ����ֽ��ϴ�.");
                    break;
                }

                var maxCard = DeckManager.Instance.GetNumberDecks().Max();
                owner.DrawNumberCardSpecial(maxCard);
                break;
            case SpecialType.RETURN:
                if (owner.numberCards.Count <= 1)
                {
                    UIManager.Instance.LogText("��ũ�� ī��� �ǵ��� �� �����ϴ�.");
                    break;
                }

                if (owner.GhostCard != null)
                {
                    if (owner.GhostCard.index == owner.numberCards.Count)
                    {
                        UIManager.Instance.LogText(TeamUtil.GetColoringPlayerName(owner) + "�� ���� <color=#8B0000>" + owner.GhostCard.number + "</color> ��Ʈ ī�尡 ��������ϴ�", LogType.EVERYONE);
                        owner.ResetGhostCard();
                        return;
                    }
                }

                var lastCard = owner.numberCards[owner.numberCards.Count - 1];
                owner.ReturnNumberCard(lastCard);
                break;
            case SpecialType.DEPRIVATION:
                if (target.numberCards.Count <= 1)
                {
                    UIManager.Instance.LogText("��ũ�� ī��� �ǵ��� �� �����ϴ�.");
                    break;
                }

                if (target.GhostCard != null)
                {
                    if (target.GhostCard.index == target.numberCards.Count)
                    {
                        target.ResetGhostCard();
                        return;
                    }
                }

                var enemyLastCard = target.numberCards[target.numberCards.Count - 1];
                target.ReturnNumberCard(enemyLastCard);
                break;
            case SpecialType.ATTACK:
                target.DrawNumberCard();
                break;
            case SpecialType.RECALL:
                if (owner.numberCards.Count > 1)
                {
                    if (owner.GhostCard != null)
                    {
                        if (owner.GhostCard.index == owner.numberCards.Count)
                        {
                            UIManager.Instance.LogText(TeamUtil.GetColoringPlayerName(owner) + "�� ���� <color=#228B22>" + owner.GhostCard.number + "</color> ��Ʈ ī�尡 ��������ϴ�", LogType.EVERYONE);
                            owner.ResetGhostCard();
                            return;
                        }
                    }

                    var recallLastCard = owner.numberCards[owner.numberCards.Count - 1];
                    owner.ReturnNumberCard(recallLastCard);
                }
                else
                    UIManager.Instance.LogText("��ũ�� ī��� �ǵ��� �� �����ϴ�.");

                if (target.numberCards.Count > 1)
                {
                    if (target.GhostCard != null)
                    {
                        if (target.GhostCard.index == target.numberCards.Count)
                        {
                            UIManager.Instance.LogText(TeamUtil.GetColoringPlayerName(target) + "�� ���� <color=#228B22>" + target.GhostCard.number + "</color> ��Ʈ ī�尡 ��������ϴ�", LogType.EVERYONE);
                            target.ResetGhostCard();
                            return;
                        }
                    }

                    var recallLastCard = target.numberCards[target.numberCards.Count - 1];
                    target.ReturnNumberCard(recallLastCard);
                }
                else
                    UIManager.Instance.LogText("��ũ�� ī��� �ǵ��� �� �����ϴ�.");

                break;
            case SpecialType.CHANGE:
                if (owner.numberCards.Count <= 1 || target.numberCards.Count <= 1)
                {
                    UIManager.Instance.LogText("��ũ�� ī��� ��ȯ�� �� �����ϴ�.");
                    break;
                }

                var playerCard = owner.numberCards[owner.numberCards.Count - 1];
                var enemyCard = target.numberCards[target.numberCards.Count - 1];
                if (owner.GhostCard != null)
                {
                    if (owner.GhostCard.index == owner.numberCards.Count)
                    {
                        target.RemoveNumberCard(enemyCard);
                        owner.AddNumberCard(enemyCard);
                        target.AddGhostCard(owner.GhostCard.number, target.numberCards.Count);
                        owner.ResetGhostCard();
                        return;
                    }
                }

                if (target.GhostCard != null)
                {
                    if (target.GhostCard.index == target.numberCards.Count)
                    {
                        owner.RemoveNumberCard(playerCard);
                        target.AddNumberCard(playerCard);
                        owner.AddGhostCard(target.GhostCard.number, owner.numberCards.Count);
                        target.ResetGhostCard();
                        return;
                    }
                }

                owner.RemoveNumberCard(playerCard);
                target.RemoveNumberCard(enemyCard);

                target.AddNumberCard(playerCard);
                owner.AddNumberCard(enemyCard);
                break;
            case SpecialType.LOW_HIGH_CHECK:
                int playerSum = owner.GetSum();
                int enemySum = target.GetSum();

                if (playerSum > enemySum)
                    UIManager.Instance.LogText("����� ���� �� Ů�ϴ�.");
                else if (playerSum == enemySum)
                    UIManager.Instance.LogText("����� �հ� ����� ���� �����ϴ�.");
                else
                    UIManager.Instance.LogText("����� ���� �� Ů�ϴ�.");

                break;
            case SpecialType.SPECIAL_EYE:
                UIManager.Instance.OpenSelectCardWindow(specialType,
                    new string[]
                    {
                        "���� ū ���� �˾Ƴ���",
                        "���� ���� ���� �˾Ƴ���"
                    },
                    new Action[]
                    {
                        () => UIManager.Instance.LogText("���� ū �� : " + target.numberCards.Max()),
                        () => UIManager.Instance.LogText("���� ���� �� : " +
                                                         (target.GhostCard != null && target.GhostCard.number < target.numberCards.Min() ? target.GhostCard.number : enemyPlayer.numberCards.Min()))
                    });
                break;
            case SpecialType.ALCHEMY:
                alchemyCount = 2;
                UIManager.Instance.OpenSpecialCardWindow(true);
                break;
            case SpecialType.PERFECT_SELECT:
                if (DeckManager.Instance.IsNumberDeckEmpty())
                {
                    UIManager.Instance.LogText("���� ī�� ���� ����ֽ��ϴ�.");
                    break;
                }

                owner.PerfectSelect();
                break;
            case SpecialType.MERCY:
                if (DeckManager.Instance.IsNumberDeckEmpty())
                {
                    UIManager.Instance.LogText("���� ī�� ���� ����ֽ��ϴ�.");
                    break;
                }

                target.PerfectSelect();
                break;
            case SpecialType.TARGET_24:
                if (IsCardTableSetting(SpecialType.TARGET_27))
                    RemoveCardTable(SpecialType.TARGET_27);

                AddCardTable(owner.team, SpecialType.TARGET_24);
                break;
            case SpecialType.TARGET_27:
                if (IsCardTableSetting(SpecialType.TARGET_24))
                    RemoveCardTable(SpecialType.TARGET_24);

                AddCardTable(owner.team, SpecialType.TARGET_27);
                break;
            case SpecialType.SEAL:
                AddCardTable(owner.team, SpecialType.SEAL);
                break;
            case SpecialType.DESTROY:
                ResetCardTable(TeamUtil.OtherTeam(owner.team));
                break;
            case SpecialType.RUIN:
                ResetCardTable();
                break;
            case SpecialType.RESISTANCE:
                AddCardTable(owner.team, SpecialType.RESISTANCE);
                break;
            case SpecialType.GHOST_CARD:
                int index = owner.numberCards.Count;
                UIManager.Instance.OpenSelectCardWindow(specialType,
                    new string[]
                    {
                        "-1���� �����Ѵ�",
                        "0���� �����Ѵ�",
                        "1�� �����Ѵ�"
                    },
                    new Action[]
                    {
                        () => owner.AddGhostCard(-1, index),
                        () => owner.AddGhostCard(0, index),
                        () => owner.AddGhostCard(1, index)
                    });
                break;
            case SpecialType.REFLECT:
                AddCardTable(owner.team, SpecialType.REFLECT);
                break;
            case SpecialType.TIME_WATCH:
                TurnChange(true);
                break;
        }
    }
}

public static class RandomUtil
{
    public static T Select<T>(List<T> ts)
    {
        return ts[Random.Range(0, ts.Count)];
    }
}

public static class TeamUtil
{
    public static Color TeamToColor(Team team)
    {
        switch (team)
        {
            case Team.RED:
                return Color.red;
            case Team.BLUE:
                return Color.blue;
            default:
                return Color.white;
        }
    }

    public static string TeamToColorText(Team team)
    {
        switch (team)
        {
            case Team.RED:
                return "<#ff0000>";
            case Team.BLUE:
                return "<#0000ff>";
            default:
                return "";
        }
    }

    public static Team OtherTeam(Team team)
    {
        switch (team)
        {
            case Team.RED:
                return Team.BLUE;
            case Team.BLUE:
                return Team.RED;
            default:
                return Team.NONE;
        }
    }

    public static string GetColoringPlayerName(Player player)
    {
        return TeamToColorText(player.team) + player.NickName + "</color>";
    }
}