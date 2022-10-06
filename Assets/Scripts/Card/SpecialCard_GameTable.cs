using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialCard_GameTable : SpecialCard
{
    protected int timer;
    protected virtual int maxTimer
    {
        get
        {
            return 0;
        }
    }

    public override void Init(Player player)
    {
        base.Init(player);
        timer = maxTimer;
    }

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
    }

    public virtual void OnNextTurn()
    {
        timer--;
        if (timer <= 0)
        {
            OnBreak();
        }
    }
}
