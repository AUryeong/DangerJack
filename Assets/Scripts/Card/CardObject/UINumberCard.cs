using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UINumberCard : PoolObject
{
    TextMeshProUGUI numberText;
    protected NumberCard showNumberCard;

    protected void Awake()
    {
        numberText = GetComponent<TextMeshProUGUI>();
    }

    public void ShowCard(NumberCard numberCard)
    {
        if (numberCard == null) return;
        if (numberCard == showNumberCard) return;
        showNumberCard = numberCard;
        if (numberCard == GameManager.Instance.enemyPlayer.numberCards[0])
            numberText.text = numberCard.number.ToString();
        else
            numberText.text = "?";
    }
}
