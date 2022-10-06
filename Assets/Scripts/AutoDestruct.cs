using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestruct : MonoBehaviour
{
    public float duration = 1;
    private float durationTime = 0;

    protected void OnEnable()
    {
        durationTime = 0;
    }

    protected void Update()
    {
        durationTime += Time.deltaTime;
        if(durationTime >= duration)
        {
            gameObject.SetActive(false);
        }
    }
}
