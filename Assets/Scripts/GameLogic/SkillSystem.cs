using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSystem : LSingleton<SkillSystem> {

    public void UseSkill(Unit attacker, Unit defender, Skill skill )
    {
        //MagicSytem.Instance.CreateMagicNew(attacker, defender, skill.magicid);
        MagicSytem.Instance.CreateMagicNew(attacker, defender, skill);
    }
}
