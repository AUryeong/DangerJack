using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Random = UnityEngine.Random;

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
        UIManager.Instance.LogText("게임 시작", LogType.EVERYONE);

        player.DrawNumberCard();
        player.DrawNumberCard();

        enemyPlayer.DrawNumberCard();
        enemyPlayer.DrawNumberCard();

        GetPlayer(Team.RED).DrawSpecialCard(2);
        photonView.RPC(nameof(InitRPC), RpcTarget.AllBuffered);
        
        string coloringName = TeamUtil.GetColoringPlayerName(GetPlayer(TeamUtil.OtherTeam(Team.RED)));
        UIManager.Instance.LogText($"{coloringName}의 턴", LogType.EVERYONE);

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
            int targetNumber = 21;
            UIManager.Instance.LogText("게임 종료!");

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
            UIManager.Instance.LogText(TeamUtil.GetColoringPlayerName(player) + "의 승리!");
            UIManager.Instance.GameEnd(winner);
            IsGaming = false;
            return;
        }

        UIManager.Instance.PlayerNotActSetting(team, isTurnNoDraw);
        UIManager.Instance.PlayerNotActSetting(TeamUtil.OtherTeam(team), false);
        string coloringName = TeamUtil.GetColoringPlayerName(GetPlayer(TeamUtil.OtherTeam(team)));
        UIManager.Instance.LogText($"{coloringName}의 턴");

        if (isTurnSkipPrev)
        {
            UIManager.Instance.LogText($"{coloringName}가 숫자 카드를 뽑지 않았습니다.");
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
            UIManager.Instance.LogText("이번 턴 이미 숫자 카드를 뽑았습니다.");
            return;
        }

        if (DeckManager.Instance.IsNumberDeckEmpty())
        {
            UIManager.Instance.LogText("숫자 카드 덱이 비어있습니다.");
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