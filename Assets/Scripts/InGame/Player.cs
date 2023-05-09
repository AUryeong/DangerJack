using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPun
{
    public Team team;
    public List<int> numberCards = new List<int>();
    public List<SpecialType> speicalCards = new List<SpecialType>();

    private new string name = string.Empty;

    public string NickName
    {
        get
        {
            if (string.IsNullOrEmpty(name))
                name = photonView.Owner.NickName;
            return name;
        }
    }

    public int GetSum()
    {
        return numberCards.Sum();
    }

    public void SetTeam(Team team)
    {
        photonView.RPC(nameof(SetTeamRPC), RpcTarget.AllBuffered, team);
    }

    [PunRPC]
    private void SetTeamRPC(Team setTeam)
    {
        team = setTeam;
        UIManager.Instance.PlayerSetting(this);
        base.name = NickName;
        if (photonView.IsMine)
            InGameManager.Instance.player = this;
        else
            InGameManager.Instance.enemyPlayer = this;
    }

    public void DrawNumberCard()
    {
        if (DeckManager.Instance.IsNumberDeckEmpty())
        {
            UIManager.Instance.LogText("숫자 카드 덱이 비어있습니다.");
            return;
        }

        photonView.RPC(nameof(AddNumberCardRPC), RpcTarget.AllBuffered, DeckManager.Instance.DrawNumber());
    }

    public void DrawSpecialCard(int count = 1)
    {
        if (DeckManager.Instance.IsSpecialDeckEmpty())
        {
            UIManager.Instance.LogText("스페셜 카드 덱이 비어있습니다.");
            return;
        }

        int deckCount = DeckManager.Instance.GetSpecialDecks().Count;
        if (count > deckCount)
        {
            count = deckCount;
            UIManager.Instance.LogText($"스페셜 카드가 부족해 {count}장만 뽑습니다.");
        }
        
        UIManager.Instance.LogText($"{TeamUtil.GetColoringPlayerName(this)}가 스페셜 카드를 {count}장 뽑았습니다.", LogType.EVERYONE);

        for (int i = 0; i < count; i++)
        {
            SpecialType specialType = DeckManager.Instance.DrawSpecial();
            var specialData = ResourceManager.Instance.GetSpecialData(specialType);
            
            UIManager.Instance.LogText($"스페셜 카드 {specialData.name}를 획득했습니다", photonView.IsMine ? LogType.DIRECT : LogType.OTHER);
            
            photonView.RPC(nameof(AddSpecialCardRPC), RpcTarget.AllBuffered, specialType);
        }
    }

    [PunRPC]
    private void AddSpecialCardRPC(SpecialType specialType)
    {
        speicalCards.Add(specialType);
    }

    [PunRPC]
    private void AddNumberCardRPC(int number)
    {
        if (numberCards.Count <= 0)
            UIManager.Instance.LogText(TeamUtil.GetColoringPlayerName(this) + "가 숫자 시크릿 카드를 뽑았습니다.");
        else
            UIManager.Instance.LogText(TeamUtil.GetColoringPlayerName(this) + "가 숫자 " + number + " 카드를 뽑았습니다.");
        
        numberCards.Add(number);

        UIManager.Instance.UpdateCard(team, numberCards);
    }
}