using System.Collections.Generic;
using Photon.Pun;

public class DeckManager : SingletonPun<DeckManager>
{
    private List<int> numberDecks = new List<int>();

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
    }

    public bool IsNumberEmpty()
    {
        return numberDecks.Count <= 0;
    }

    public int NumberDraw()
    {
        int number = RandomUtil.Select(numberDecks);
        photonView.RPC(nameof(NumberDrawRPC), RpcTarget.AllBuffered, number);
        return number;
    }

    [PunRPC]
    private void NumberDrawRPC(int number)
    {
        numberDecks.Remove(number);
    }
}