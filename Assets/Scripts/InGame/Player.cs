using System.Collections.Generic;
using System.Linq;
using Photon.Pun;

public class Player : MonoBehaviourPun
{
    public Team team;
    public List<int> numberCards = new List<int>();
    public List<SpecialType> specialCards = new List<SpecialType>();

    public GhostCard GhostCard { get; private set; }

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
        return numberCards.Sum() + (GhostCard == null ? 0 : GhostCard.number);
    }

    public void SetTeam(Team setTeam)
    {
        photonView.RPC(nameof(SetTeamRPC), RpcTarget.AllBuffered, setTeam);
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

    public void DrawNumberCardSpecial(int number)
    {
        if (DeckManager.Instance.IsNumberDeckEmpty())
        {
            UIManager.Instance.LogText("숫자 카드 덱이 비어있습니다.");
            return;
        }

        if (!DeckManager.Instance.IsContainNumber(number))
        {
            UIManager.Instance.LogText("해당 카드가 덱에 없습니다");
            return;
        }

        DeckManager.Instance.DrawSpecialNumber(number);
        photonView.RPC(nameof(AddNumberCardRPC), RpcTarget.AllBuffered, number);
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

        UIManager.Instance.LogText($"{TeamUtil.GetColoringPlayerName(this)}가 <color=#FFEE00>스페셜 카드</color>를 {count}장 얻었습니다.", LogType.EVERYONE);

        for (int i = 0; i < count; i++)
        {
            SpecialType specialType = DeckManager.Instance.DrawSpecial();
            var specialData = ResourceManager.Instance.GetSpecialData(specialType);

            UIManager.Instance.LogText($"스페셜 카드 <color=#FFEE00>{specialData.name}</color>를 획득했습니다", photonView.IsMine ? LogType.DIRECT : LogType.OTHER);

            photonView.RPC(nameof(AddSpecialCardRPC), RpcTarget.AllBuffered, specialType);
        }
    }

    public void PerfectSelect()
    {
        int sum = GetSum();
        int max = DeckManager.Instance.GetNumberDecks().Max();
        if (sum >= InGameManager.Instance.TargetValue)
        {
            DrawNumberCardSpecial(DeckManager.Instance.GetNumberDecks().Min());
        }
        else if (sum + max <= InGameManager.Instance.TargetValue)
        {
            DrawNumberCardSpecial(max);
        }
        else
        {
            var numberDecks = DeckManager.Instance.GetNumberDecks();
            numberDecks = numberDecks.OrderByDescending((x) => x).ToList();

            int prefectCard = -1;
            foreach (int i in numberDecks)
            {
                prefectCard = i;
                if (sum + prefectCard <= InGameManager.Instance.TargetValue)
                    break;
            }

            DrawNumberCardSpecial(prefectCard);
        }
    }

    [PunRPC]
    private void AddSpecialCardRPC(SpecialType specialType)
    {
        specialCards.Add(specialType);
    }

    [PunRPC]
    private void AddNumberCardRPC(int number)
    {
        if (numberCards.Count <= 0 && InGameManager.Instance.player != this)
            UIManager.Instance.LogText(TeamUtil.GetColoringPlayerName(this) + "가 숫자 <color=#228B22>시크릿</color> 카드를 얻었습니다");
        else
            UIManager.Instance.LogText(TeamUtil.GetColoringPlayerName(this) + "가 숫자 <color=#228B22>" + number + "</color> 카드를 얻었습니다.");

        numberCards.Add(number);

        UIManager.Instance.UpdateCard(team, numberCards, GhostCard);
    }

    public void AddNumberCard(int number)
    {
        photonView.RPC(nameof(AddNumberCardRPC), RpcTarget.AllBuffered, number);
    }

    public void ResetGhostCard()
    {
        photonView.RPC(nameof(ResetGhostCardRPC), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void ResetGhostCardRPC()
    {
        UIManager.Instance.LogText(TeamUtil.GetColoringPlayerName(this) + "의 숫자 <color=#8B0000>" + GhostCard.number + "</color> 고스트 카드가 사라졌습니다");
        GhostCard = null;

        UIManager.Instance.UpdateCard(team, numberCards, GhostCard);
    }
    public void AddGhostCard(int number, int index)
    {
        photonView.RPC(nameof(AddGhostCardRPC), RpcTarget.AllBuffered, number, index);
    }

    [PunRPC]
    private void AddGhostCardRPC(int number, int index)
    {
        UIManager.Instance.LogText(TeamUtil.GetColoringPlayerName(this) + "의 숫자 <color=#228B22>" + number + "</color> 고스트 카드가 생겼습니다");
        GhostCard = new GhostCard(number, index);

        UIManager.Instance.UpdateCard(team, numberCards, GhostCard);
    }

    public void RemoveSpecialCard(SpecialType type)
    {
        photonView.RPC(nameof(RemoveSpecialCardRPC), RpcTarget.AllBuffered, type);
    }

    [PunRPC]
    private void RemoveSpecialCardRPC(SpecialType type)
    {
        specialCards.Remove(type);
    }

    public void ReturnSpecialCard(SpecialType type)
    {
        RemoveSpecialCard(type);
        DeckManager.Instance.ReturnSpecialDeck(type);
    }
    
    public void RemoveNumberCard(int number)
    {
        UIManager.Instance.LogText(TeamUtil.GetColoringPlayerName(this) + "의 숫자 <color=#8B0000>" + number + "</color> 카드가 사라졌습니다", LogType.EVERYONE);

        photonView.RPC(nameof(RemoveNumberCardRPC), RpcTarget.AllBuffered, number);
    }

    [PunRPC]
    private void RemoveNumberCardRPC(int number)
    {
        numberCards.Remove(number);

        UIManager.Instance.UpdateCard(team, numberCards, GhostCard);
    }

    public void ReturnNumberCard(int number)
    {
        UIManager.Instance.LogText(TeamUtil.GetColoringPlayerName(this) + "의 숫자 <color=#FFA500>" + number + "</color> 카드가 덱으로 되돌아 갔습니다", LogType.EVERYONE);

        photonView.RPC(nameof(RemoveNumberCardRPC), RpcTarget.AllBuffered, number);
        DeckManager.Instance.ReturnNumberDeck(number);
    }
}