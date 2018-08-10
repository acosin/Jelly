﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicSytem : LSingleton<MagicSytem>
{
    List<Magic> magics = new List<Magic>();
    public void CreateMagic(Unit attacker, Unit defender, uint magicid)
    {
        //Magic magic = new Magic();

        //magics.Add(magic);
        Object arrowPrefab = Resources.Load("star");

        GameObject arrow = Object.Instantiate(arrowPrefab) as GameObject;
        arrow.transform.parent = LevelManager.THIS.GameField;
        arrow.transform.position = attacker.item.transform.position;

#if true
        MagicFly fly = arrow.AddComponent<MagicFly>();
        fly.targetPos = defender.item.transform.position;
        fly.targetUnit = defender;
        fly.speed = 10;
        Debug.Log("create magic");
#else

#endif
    }

    public void tick(float delta)
    {
        foreach(Magic magic in magics)
        {
            magic.tick(delta);
            if(magic.isDead)
            {
                magics.Remove(magic);
            }
        }
    }
}
