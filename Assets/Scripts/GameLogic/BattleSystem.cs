using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem :LSingleton<BattleSystem> {

    public IEnumerator onCombine(List<Combine> combines)
    {
        //处理玩法逻辑
        foreach(Combine comb in combines)
        {
            //switch(comb.nextType)
            //{
            //    case ItemsTypes.NONE:
            //        {

            //            break;
            //        }
            //    case ItemsTypes.BOMB:
            //        {

            //            break;
            //        }
            //    case ItemsTypes.HORIZONTAL_STRIPPED:
            //        {

            //            break;
            //        }
            //    case ItemsTypes.VERTICAL_STRIPPED:
            //        {

            //            break;
            //        }
            //    case ItemsTypes.PACKAGE:
            //        {

            //            break;
            //        }
            //    case ItemsTypes.INGREDIENT:
            //        {

            //            break;
            //        }
            //    default:
            //        {
            //            break;
            //        }
            //}

            foreach(Item item in comb.items)
            {
                Unit attack = item.square as Unit;
                Unit defender = SelectAttackTarget();
                if(defender)
                {
                    Skill skill = new Skill();
                    SkillSystem.Instance.UseSkill(attack, defender, skill);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        foreach (Combine comb in combines)
        {
            foreach (Item item in comb.items)
            {
                item.DestroyItem(false, "", true);
            }
        }
    }


    Unit SelectAttackTarget()
    {
        Unit unit = null;
        int count = 0;
        while(count++ < 10)
        {
            int row = Random.Range(0, LevelManager.THIS.opponentRows - 1);
            int col = Random.Range(0, LevelManager.THIS.maxCols - 1);
            unit = LevelManager.THIS.GetSquare(col, row) as Unit;
            if(unit)
            {
                break;
            }
        }

        return unit;
    }
}
