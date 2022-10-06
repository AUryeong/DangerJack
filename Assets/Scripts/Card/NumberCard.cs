using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class NumberCard
{
    public int number;
    public bool isGhost = false;
    public Player owner
    {
        get; private set;
    }
    public void Init(Player player)
    {
        owner = player;
    }
}
