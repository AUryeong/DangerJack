using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkPoolManager : SingletonDontDestroy<NetworkPoolManager>
{
    public Dictionary<string, List<PoolObject>> pools = new Dictionary<string, List<PoolObject>>();

    public override void OnReset()
    {
        base.OnReset();
        foreach (var photonviews in pools.Values)
            foreach (var photonview in photonviews)
                photonview.gameObject.SetActive(false);
    }

    public PoolObject Init(string origin, Vector3? pos = null, Quaternion? rotation = null)
    {
        if (string.IsNullOrEmpty(origin)) return null;

        Vector3 p = pos.HasValue ? pos.Value : Vector3.zero;
        Quaternion r = rotation.HasValue ? rotation.Value : Quaternion.identity;
        PoolObject photonView = null;

        if (pools.ContainsKey(origin))
        {
            List<PoolObject> activeViews = pools[origin].FindAll((PoolObject x) => !x.gameObject.activeSelf);
            if (activeViews.Count > 0)
            {
                photonView = activeViews[0];
                photonView.SetActive(true);
                return photonView;
            }
        }
        else
        {
            pools.Add(origin, new List<PoolObject>());
        }
        photonView = PhotonNetwork.Instantiate(origin, p, r).GetComponent<PoolObject>();
        DontDestroyOnLoad(photonView);
        pools[origin].Add(photonView);

        return photonView;
    }
}
