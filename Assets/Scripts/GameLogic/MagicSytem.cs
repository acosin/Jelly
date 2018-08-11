using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicSytem : LSingleton<MagicSytem>
{
    List<Magic> magics = new List<Magic>();

    // New
    public void CreateMagicNew(Unit attacker, Unit defender, Skill skill)
    {
        uint magicid = skill.magicid;

        //Magic magic = new Magic();
        if (attacker == null || defender == null || defender.item == null)
        {
            return;
        }
        //magics.Add(magic);

        //Object arrowPrefab = Resources.Load("star");
        Object arrowPrefab = Resources.Load(skill.getSkillAssetPath());

        GameObject arrow = Object.Instantiate(arrowPrefab) as GameObject;
        arrow.transform.parent = LevelManager.THIS.GameField;
        arrow.transform.position = attacker.item.transform.position;

#if true
        MagicFly fly = arrow.GetComponent<MagicFly>();
        fly.targetPos = defender.item.transform.position;
        fly.targetUnit = defender;
        //fly.speed = 10;
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
