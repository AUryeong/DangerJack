using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class NumberCard : MonoBehaviourPun
{
    public int number { get; private set; }
    private bool isGhost = false;
    [SerializeField]
    TextMeshPro numberText;
    public void ShowNumber(int toNumber)
    {
        number = toNumber;
        numberText.text = number.ToString();
    }
}
