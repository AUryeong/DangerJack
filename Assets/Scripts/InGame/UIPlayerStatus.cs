using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class UIPlayerStatus : MonoBehaviourPun
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI[] numberCardTexts;
    [SerializeField] private TextMeshProUGUI notActText;
    [SerializeField] private TextMeshProUGUI sumText;

    public void SetPlayerName(string playerName)
    {
        photonView.RPC(nameof(SetPlayerNameRPC), RpcTarget.AllBuffered, playerName);
    }

    [PunRPC]
    private void SetPlayerNameRPC(string playerName)
    {
        playerNameText.text = playerName;
    }

    public void SetNotAct(bool notAct)
    {
        photonView.RPC(nameof(SetNotActRPC), RpcTarget.AllBuffered, notAct);
    }

    [PunRPC]
    private void SetNotActRPC(bool notAct)
    {
        notActText.gameObject.SetActive(notAct);
    }

    public void SetNumberCard(bool secret, List<int> numberCards)
    {
        int sum = 0;
        for (int i = 0; i < numberCardTexts.Length; i++)
        {
            if (numberCards.Count > i)
            {
                numberCardTexts[i].gameObject.SetActive(true);
                if (i == 0 && !secret)
                    numberCardTexts[i].text = "?";
                else
                {
                    sum += numberCards[i];
                    numberCardTexts[i].text = numberCards[i].ToString();
                    // TODO 색깔
                }
            }
            else
                numberCardTexts[i].gameObject.SetActive(false);
        }

        string text = "종합 : " + sum;
        if (!secret)
            text += " + ?";

        sumText.text = text;
    }
}