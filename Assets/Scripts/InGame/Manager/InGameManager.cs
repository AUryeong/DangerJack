using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public enum Team
{
    NONE,
    RED,
    BLUE
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
    public int targetValue { get; private set; } = 21;

    public bool IsTurnMine => turnOwner == player.team;

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (!IsGaming) return;

        ExitGame();
    }

    public void ExitGame()
    {
        GameManager.Instance.LoadScene(SceneType.TITLE);
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
        if (!IsGaming) return;
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

    public void SetTargetValue(int value)
    {
        photonView.RPC(nameof(SetTargetValueRPC), RpcTarget.AllBuffered, value);
    }

    [PunRPC]
    private void SetTargetValueRPC(int value)
    {
        targetValue = value;
        UIManager.Instance.TargetValueSetting();
    }

    public void TurnChange()
    {
        if (!IsTurnMine) return;
        if (!IsGaming) return;

        bool temp = isTurnNoDraw;
        photonView.RPC(nameof(TurnChangeRPC), RpcTarget.AllBuffered, temp);
    }

    [PunRPC]
    private void TurnChangeRPC(bool isTurnSkipPrev)
    {
        Team team = turnOwner;
        if (isEnemyTurnNoDraw && isTurnSkipPrev)
        {
            Player winner = null;
            int targetNumber = targetValue;
            UIManager.Instance.LogText("���� ����!");

            if (player.GetSum() > targetNumber && enemyPlayer.GetSum() > targetNumber)
            {
                if (player.GetSum() == enemyPlayer.GetSum())
                    winner = player.team == Team.BLUE ? player : enemyPlayer;
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
                        winner = player.team == Team.BLUE ? player : enemyPlayer;
                    else if (player.GetSum() < enemyPlayer.GetSum())
                        winner = enemyPlayer;
                    else
                        winner = player;
                }
            }

            UIManager.Instance.UpdateCard(player.team, player.numberCards, true);
            UIManager.Instance.UpdateCard(enemyPlayer.team, enemyPlayer.numberCards, true);
            UIManager.Instance.LogText(TeamUtil.GetColoringPlayerName(winner) + "�� �¸�!");
            UIManager.Instance.GameEnd(winner);
            IsGaming = false;
            return;
        }

        UIManager.Instance.PlayerNotActSetting(team, isTurnNoDraw);
        UIManager.Instance.PlayerNotActSetting(TeamUtil.OtherTeam(team), false);

        UIManager.Instance.LogText($"{TeamUtil.GetColoringPlayerName(GetPlayer(TeamUtil.OtherTeam(team)))}�� ��");
        if (isTurnSkipPrev)
        {
            UIManager.Instance.LogText($"{TeamUtil.GetColoringPlayerName(GetPlayer(team))}�� ���� ī�带 ���� �ʾҽ��ϴ�.");
        }

        isEnemyTurnNoDraw = isTurnSkipPrev;
        isTurnNoDraw = true;

        turnOwner = TeamUtil.OtherTeam(team);
        turnCount++;

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
        player.RemoveSpecialCard(specialType);
        var specialData = ResourceManager.Instance.GetSpecialData(specialType);
        string coloringName = TeamUtil.GetColoringPlayerName(player);
        UIManager.Instance.LogText($"{coloringName}�� ����� ī�� {specialData.name}�� ����߽��ϴ�", LogType.EVERYONE);
        switch (specialType)
        {
            case SpecialType.THREE_CARD:
                player.DrawNumberCardSpecial(3);
                break;
            case SpecialType.FOUR_CARD:
                player.DrawNumberCardSpecial(4);
                break;
            case SpecialType.FIVE_CARD:
                player.DrawNumberCardSpecial(5);
                break;
            case SpecialType.SIX_CARD:
                player.DrawNumberCardSpecial(6);
                break;
            case SpecialType.DECK_DRAW:
                player.DrawNumberCard();
                break;
            case SpecialType.MIN_CARD:
                if (DeckManager.Instance.IsNumberDeckEmpty())
                {
                    UIManager.Instance.LogText("���� ī�� ���� ����ֽ��ϴ�.");
                    break;
                }
                var minCard = DeckManager.Instance.GetNumberDecks().Min();
                player.DrawNumberCardSpecial(minCard);
                break;
            case SpecialType.MAX_CARD:
                if (DeckManager.Instance.IsNumberDeckEmpty())
                {
                    UIManager.Instance.LogText("���� ī�� ���� ����ֽ��ϴ�.");
                    break;
                }
                var maxCard = DeckManager.Instance.GetNumberDecks().Max();
                player.DrawNumberCardSpecial(maxCard);
                break;
            case SpecialType.RETURN:
                if (player.numberCards.Count <= 1)
                {
                    UIManager.Instance.LogText("��ũ�� ī��� �ǵ��� �� �����ϴ�.");
                    break;
                }
                var lastCard = player.numberCards[player.numberCards.Count - 1];
                player.ReturnNumberCard(lastCard);
                break;
            case SpecialType.DEPRIVATION:
                if (enemyPlayer.numberCards.Count <= 1)
                {
                    UIManager.Instance.LogText("��ũ�� ī��� �ǵ��� �� �����ϴ�.");
                    break;
                }
                var enemyLastCard = enemyPlayer.numberCards[enemyPlayer.numberCards.Count - 1];
                enemyPlayer.ReturnNumberCard(enemyLastCard);
                break;
            case SpecialType.ATTACK:
                enemyPlayer.DrawNumberCard();
                break;
            case SpecialType.RECALL:
                if (player.numberCards.Count > 1)
                {
                    var recallLastCard = player.numberCards[player.numberCards.Count - 1];
                    player.ReturnNumberCard(recallLastCard);
                }
                else
                    UIManager.Instance.LogText("��ũ�� ī��� �ǵ��� �� �����ϴ�.");

                if (enemyPlayer.numberCards.Count > 1)
                {
                    var recallLastCard = enemyPlayer.numberCards[enemyPlayer.numberCards.Count - 1];
                    enemyPlayer.ReturnNumberCard(recallLastCard);
                }
                else
                    UIManager.Instance.LogText("��ũ�� ī��� �ǵ��� �� �����ϴ�.");
                break;
            case SpecialType.CHANGE:
                if (player.numberCards.Count <= 1 || enemyPlayer.numberCards.Count <= 1)
                {
                    UIManager.Instance.LogText("��ũ�� ī��� ��ȯ�� �� �����ϴ�.");
                    break;
                }
                var playerCard = player.numberCards[player.numberCards.Count - 1];
                var enemyCard = enemyPlayer.numberCards[enemyPlayer.numberCards.Count - 1];

                player.RemoveNumberCard(playerCard);
                enemyPlayer.RemoveNumberCard(enemyCard);

                enemyPlayer.AddNumberCard(playerCard);
                player.AddNumberCard(enemyCard);
                break;
            case SpecialType.LOW_HIGH_CHECK:
                int playerSum = player.GetSum();
                int enemySum = enemyPlayer.GetSum();

                if (playerSum > enemySum)
                    UIManager.Instance.LogText("����� ���� �� Ů�ϴ�.");
                else if (playerSum == enemySum)
                    UIManager.Instance.LogText("����� �հ� ����� ���� �����ϴ�.");
                else
                    UIManager.Instance.LogText("����� ���� �� Ů�ϴ�.");

                break;
            case SpecialType.SPECIAL_EYE:
                // TODO ���� UI ����
                break;
            case SpecialType.ALCHEMY:
                // TODO ���� UI ����
                break;
            case SpecialType.PERFECT_SELECT:
                if (DeckManager.Instance.IsNumberDeckEmpty())
                {
                    UIManager.Instance.LogText("���� ī�� ���� ����ֽ��ϴ�.");
                    break;
                }
                int sum = player.GetSum();
                if (sum >= targetValue)
                {
                    player.DrawNumberCardSpecial(DeckManager.Instance.GetNumberDecks().Min());
                }
                else if (sum + 11 <= targetValue)
                {
                    player.DrawNumberCardSpecial(DeckManager.Instance.GetNumberDecks().Max());
                }
                else
                {
                    var numberDecks = DeckManager.Instance.GetNumberDecks();
                    numberDecks = numberDecks.OrderByDescending((x) => x).ToList();

                    int goodCard = -1;
                    foreach (int i in numberDecks)
                    {
                        goodCard = i;
                        if (sum + goodCard <= 21)
                            break;
                    }
                    
                    player.DrawNumberCardSpecial(goodCard);
                }
                break;
            case SpecialType.MERCY:
                if (DeckManager.Instance.IsNumberDeckEmpty())
                {
                    UIManager.Instance.LogText("���� ī�� ���� ����ֽ��ϴ�.");
                    break;
                }
                int mercySum = enemyPlayer.GetSum();
                if (mercySum >= targetValue)
                {
                    enemyPlayer.DrawNumberCardSpecial(DeckManager.Instance.GetNumberDecks().Min());
                }
                else if (mercySum + 11 <= targetValue)
                {
                    enemyPlayer.DrawNumberCardSpecial(DeckManager.Instance.GetNumberDecks().Max());
                }
                else
                {
                    var numberDecks = DeckManager.Instance.GetNumberDecks();
                    numberDecks = numberDecks.OrderByDescending((x) => x).ToList();

                    int goodCard = -1;
                    foreach (int i in numberDecks)
                    {
                        goodCard = i;
                        if (mercySum + goodCard <= 21)
                            break;
                    }

                    enemyPlayer.DrawNumberCardSpecial(goodCard);
                }
                break;
            case SpecialType.TARGET_24: 
                SetTargetValue(24);
                break;
            case SpecialType.TARGET_27:
                SetTargetValue(27);
                break;
            case SpecialType.SEAL:
                break;
            case SpecialType.DESTROY:
                break;
            case SpecialType.RUIN:
                break;
            case SpecialType.RESISTANCE:
                break;
            case SpecialType.GHOST_CARD:
                break;
            case SpecialType.REFLECT:
                break;
            case SpecialType.TIMEWATCH:
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