using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;

public enum Team
{
    None,
    Red,
    Blue
}
public class GameManager : SingletonPun<GameManager>
{
    public int turnCount;
    public Team turnOwner = Team.Red;
    public Player player;
    public Player enemyPlayer;
    [SerializeField]
    Image image;
    [SerializeField]
    Text text;

    public void GameStart()
    {
        int teamIdx = Random.Range(1, 3);
        photonView.RPC(nameof(PlayerCreate), RpcTarget.OthersBuffered, TeamUtil.OtherInt(teamIdx));
        PlayerCreate(teamIdx);

        image.color = TeamUtil.TeamToColor((Team)teamIdx);
    }

    [PunRPC]
    void FinishLog()
    {

    }

    public void UpdatePlayerColor()
    {
        image.gameObject.SetActive(true);
        image.color = TeamUtil.TeamToColor(player.team);
        player.gameObject.SetActive(false);
    }

    public void UpdateEnemyName()
    {
        text.gameObject.SetActive(true);
        text.text = enemyPlayer.photonView.Owner.NickName + " ´Ô°ú ¸ÅÄªµÇ¼Ì½À´Ï´Ù";
    }

    [PunRPC]
    void PlayerCreate(int teamIdx)
    {
        player = PhotonNetwork.Instantiate(nameof(Player), Vector3.zero, Quaternion.identity).GetComponent<Player>();
        player.photonView.RPC(player.setTeamName, RpcTarget.AllBuffered, teamIdx);
        photonView.RPC(nameof(FinishLog), RpcTarget.Others);
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
            case Team.Red:
                return new Color(1, 0, 0);
            case Team.Blue:
                return new Color(0, 0, 1);
            default:
                return Color.white;
        }
    }

    public static Team OtherTeam(Team team)
    {
        switch (team)
        {
            case Team.Red:
                return Team.Blue;
            case Team.Blue:
                return Team.Red;
            default:
                return Team.None;
        }
    }

    public static int OtherInt(int idx)
    {
        switch (idx)
        {
            case 1:
                return 2;
            case 2:
                return 1;
            default:
                return 0;
        }
    }
}