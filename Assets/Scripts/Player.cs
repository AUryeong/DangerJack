using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPun
{
    public Team team;
    public List<NumberCard> numberCards = new List<NumberCard>();
    public List<SpecialCard> specialCards = new List<SpecialCard>();
    public int GetSum()
    {
        int sum = 0;
        numberCards.ForEach((NumberCard x) => sum += x.number);
        return sum;
    }

    [PunRPC]
    public void GameStart()
    {

    }

    [PunRPC]
    public void SetRandomTeam()
    {
        team = RandomUtil.Select(GameManager.Instance.teams);
        GameManager.Instance.teams.Remove(team);
        if (!photonView.IsMine)
            GameManager.Instance.enemyPlayer = this;
        if(GameManager.Instance.teams.Count <= 0)
        {
            GameManager.Instance.ShowTeamColor();
        }
    }

}
