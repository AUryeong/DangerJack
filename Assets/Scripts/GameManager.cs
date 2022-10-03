using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;

public enum Team
{
    Red,
    Blue
}
public class GameManager : MonoBehaviourPun
{
    static GameManager _instance;
    public int turnCount;
    public Team turnOwner;
    public List<Team> teams;
    public List<NumberCard> decks;
    public List<SpecialCard> specialDecks;
    public Player player;
    public Player enemyPlayer;
    [SerializeField]
    Image image;
    [SerializeField]
    Text text;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
                FindObjectOfType<GameManager>();
            return _instance;
        }
    }
    void Awake()
    {
        _instance = this;
    }

    public void GameStart()
    {
        teams = new List<Team>() { Team.Red, Team.Blue };
        GameObject obj = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0);
        player = obj.GetComponent<Player>();
        obj.GetComponent<PhotonView>().RPC("SetRandomTeam", RpcTarget.AllBuffered);
        obj.GetComponent<PhotonView>().RPC("GameStart", RpcTarget.AllBuffered);
    }

    public void GameEnd()
    {
        text.gameObject.SetActive(false);
        image.gameObject.SetActive(false);
    }

    public void ShowTeamColor()
    {
        image.gameObject.SetActive(true);
        text.gameObject.SetActive(true);
        image.color = TeamUtil.TeamToColor(player.team);
        text.text = enemyPlayer.photonView.Owner.NickName + "´Ô°ú ¸ÅÄª µÇ¼Ì½À´Ï´Ù.";
    }

    public void NextTurn()
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
        if (team == Team.Red)
            return new Color(1, 0, 0);
        else
            return new Color(0, 0, 1);
    }
}