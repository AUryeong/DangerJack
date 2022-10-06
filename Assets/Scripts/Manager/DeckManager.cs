using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : SingletonPun<DeckManager>
{
    public List<NumberCard> decks;
    public List<SpecialCard> specialDecks;

    public override void OnReset()
    {
        base.OnReset();
        decks.Clear();
        for (int i = 1; i <= decks.Count; i++)
        {
            decks.Add(new NumberCard()
            {
                number = i
            });
        }
    }
    public void DrawNumberCard(Player p)
    {
        if (decks.Count <= 0)
            return;
        NumberCard numberCard = RandomUtil.Select(decks);
        decks.Remove(numberCard);

        p.numberCards.Add(numberCard);
    }
}
