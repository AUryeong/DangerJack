using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.PlayerLoop;

public class Player : MonoBehaviourPun
{
    public Team team;
    public bool notAct { get; private set; }
    public List<int> numberCards = new List<int>();

    public int GetSum()
    {
        return numberCards.Sum();
    }

    public void SetTeam(Team team)
    {
        photonView.RPC(nameof(SetTeamRPC), RpcTarget.AllBuffered, team);
    }

    public void SetNotAct(bool setNotAct)
    {
        photonView.RPC(nameof(SetNotActRPC), RpcTarget.AllBuffered, setNotAct);
    }

    [PunRPC]
    private void SetNotActRPC(bool setNotAct)
    {
        notAct = setNotAct;
    }

    [PunRPC]
    private void SetTeamRPC(Team setTeam)
    {
        team = setTeam;
        UIManager.Instance.PlayerSetting(this);
        if (photonView.IsMine)
            InGameManager.Instance.player = this;
        else
            InGameManager.Instance.enemyPlayer = this;
    }

    public void DrawNumberCard()
    {
        if (DeckManager.Instance.IsNumberEmpty())
        {
            return;
        }
        
        photonView.RPC(nameof(AddNumberCardRPC), RpcTarget.AllBuffered, DeckManager.Instance.NumberDraw());
    }

    [PunRPC]
    private void AddNumberCardRPC(int number)
    {
        numberCards.Add(number);

        UIManager.Instance.UpdateCard(team, numberCards);
    }
}