using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill {
    public uint id;
    public string name;
    public uint magicid;//技能释放的魔法

    // 获取技能资源 Id
    public string getSkillAssetPath()
    {
        string ret = "star";

        if(1 == magicid)
        {
            ret = "star";
        }
        else if(2 == magicid)
        {
            ret = "star_1";
        }
        else if (3 == magicid)
        {
            ret = "star_2";
        }
        else if (4 == magicid)
        {
            ret = "star_3";
        }
        else if (5 == magicid)
        {
            ret = "star_4";
        }

        return ret;
    }
}

/**
 * @brief 技能生成，总共五个技能
 */
public class SkillFactory
{
    public static Skill getSkillById(ItemsTypes itemsTypes)
    {
        Skill ret = new Skill();
        ret.magicid = 1;

        if (ItemsTypes.NONE == itemsTypes)
        {
            ret.magicid = 1;
        }
        else if (ItemsTypes.VERTICAL_STRIPPED == itemsTypes)
        {
            ret.magicid = 2;
        }
        else if (ItemsTypes.HORIZONTAL_STRIPPED == itemsTypes)
        {
            ret.magicid = 3;
        }
        else if (ItemsTypes.BOMB == itemsTypes)
        {
            ret.magicid = 4;
        }
        else if (ItemsTypes.INGREDIENT == itemsTypes)
        {
            ret.magicid = 5;
        }

        return ret;
    }
}