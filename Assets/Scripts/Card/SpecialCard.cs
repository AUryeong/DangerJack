using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class SpecialCard
{
    protected Player owner
    {
        get; private set;
    }
    public virtual void Init(Player player)
    {
        owner = player;
    }
    public virtual void OnUse()
    {

    }
}
