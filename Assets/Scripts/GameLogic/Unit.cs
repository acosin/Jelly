using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : Square, IMatchUnit,IBattleUnit {
    public int life = 3;
    public void OnHarm(int harm)
    {
        if(harm >= life)
        {
            item.DestroyItem(false, "2222", true);
        }
        else
        {
            life -= harm;
        }
    }
    public void playHit()
    {
        
    }

    public void attackOver()
    {
        throw new System.NotImplementedException();
    }

    public void runningOver()
    {
        throw new System.NotImplementedException();
    }

    public void playRunning()
    {
        throw new System.NotImplementedException();
    }

    public void prepareOver()
    {
        throw new System.NotImplementedException();
    }

    public void playPrepare()
    {
        throw new System.NotImplementedException();
    }

    public void addWithBuff(int buffid)
    {
        throw new System.NotImplementedException();
    }
}
