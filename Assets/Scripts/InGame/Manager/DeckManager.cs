using System.Collections.Generic;
using Photon.Pun;

public enum SpecialType
{
    THREE_CARD,
    FOUR_CARD,
    FIVE_CARD,
    SIX_CARD,
    DECK_DRAW,
    MIN_CARD,
    MAX_CARD,
    RETURN,
    DEPRIVATION,
    ATTACK,
    RECALL,
    CHANGE,
    LOW_HIGH_CHECK,
    SPECIAL_EYE,
    ALCHEMY,
    PERFECT_SELECT,
    MERCY,
    TARGET_24,
    TARGET_27,
    SEAL,
    DESTROY,
    RUIN,
    RESISTANCE,
    GHOST_CARD,
    REFLECT,
    TIME_WATCH
}

public class GhostCard
{
    public int index;
    public int number;

    public GhostCard(int number, int index)
    {
        this.number = number;
        this.index = index;
    }
}
public class DeckManager : SingletonPun<DeckManager>
{
    private List<int> numberDecks = new List<int>();
    private readonly List<SpecialType> specialDecks = new List<SpecialType>();

    public void Init()
    {
        photonView.RPC(nameof(InitRPC), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void InitRPC()
    {
        numberDecks = new List<int>();
        for (int i = 1; i <= 11; i++)
            numberDecks.Add(i);
        
        for (SpecialType specialCard = 0; specialCard <= SpecialType.TIME_WATCH; specialCard++)
        {
            specialDecks.Add(specialCard);
        }
    }

    #region NumberCard
    public bool IsNumberDeckEmpty()
    {
        return numberDecks.Count <= 0;
    }

    public bool IsContainNumber(int number)
    {
        return numberDecks.Exists((card) => card == number);
    }

    public List<int> GetNumberDecks()
    {
        return new List<int>(numberDecks);
    }

    public int DrawNumber()
    {
        int number = RandomUtil.Select(numberDecks);
        photonView.RPC(nameof(DrawNumberRPC), RpcTarget.AllBuffered, number);
        return number;
    }

    [PunRPC]
    private void DrawNumberRPC(int number)
    {
        numberDecks.Remove(number);
    }
    
    public void ReturnNumberDeck(int number)
    {
        photonView.RPC(nameof(ReturnNumberDeckRPC), RpcTarget.AllBuffered, number);
    }

    [PunRPC]
    private void ReturnNumberDeckRPC(int number)
    {
        numberDecks.Add(number);
    }

    #endregion
    
    #region SpecialCard
    public bool IsSpecialDeckEmpty()
    {
        return specialDecks.Count <= 0;
    }

    public List<SpecialType> GetSpecialDecks()
    {
        return new List<SpecialType>(specialDecks);
    }

    public SpecialType DrawSpecial()
    {
        SpecialType card = RandomUtil.Select(specialDecks);
        photonView.RPC(nameof(DrawSpecialRPC), RpcTarget.AllBuffered, card);
        return card;
    }

    public void DrawSpecialNumber(int number)
    {
        if (IsContainNumber(number))
            photonView.RPC(nameof(DrawNumberRPC), RpcTarget.AllBuffered, number);
    }

    [PunRPC]
    private void DrawSpecialRPC(SpecialType card)
    {
        specialDecks.Remove(card);
    }
    
    public void ReturnSpecialDeck(SpecialType card)
    {
        photonView.RPC(nameof(ReturnSpecialDeckRPC), RpcTarget.AllBuffered, card);
    }

    [PunRPC]
    private void ReturnSpecialDeckRPC(SpecialType card)
    {
        specialDecks.Add(card);
    }

    #endregion
}