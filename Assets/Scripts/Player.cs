using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.PlayerLoop;

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
    public void SetTeam(int teamIdx)
    {
        team = (Team)teamIdx;
        if (photonView.IsMine)
        {
            GameManager.Instance.player = this;
            GameManager.Instance.UpdatePlayerColor();
        }
        else
        {
            GameManager.Instance.enemyPlayer = this;
            GameManager.Instance.UpdateEnemyName();
        }
    }

}
