using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class SpecialCard : MonoBehaviour
{
    [SerializeField]
    protected TextMeshPro nameText;
    [SerializeField]
    protected TextMeshPro loreText;
    private Player owner;
    public virtual void Init(Player player)
    {
        owner = player;
    }
    public virtual void OnUse()
    {

    }
}
