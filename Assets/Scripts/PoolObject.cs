using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject : MonoBehaviourPun
{
    public readonly static string SA_name = nameof(PunSetActive);
    public void SetActive(bool active)
    {
        photonView.RPC(SA_name, RpcTarget.AllBuffered, active);
    }

    [PunRPC]
    protected void PunSetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
