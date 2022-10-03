using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialCard_GameTable : SpecialCard
{
    [SerializeField]
    protected int timer;

    public virtual bool IsUsableSpecial()
    {
        return true;
    }
    
    public virtual bool IsNumberChangable()
    {
        return true;
    }

    public virtual int GetTargetNumber()
    {
        return -1;
    }

    public virtual void OnBreak()
    {
        Destroy(gameObject);
    }

    public virtual void OnNextTurn()
    {
        timer--;
        if(timer <= 0)
        {
            OnBreak();
        }
    }
}
