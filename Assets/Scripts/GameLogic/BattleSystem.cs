using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem :LSingleton<BattleSystem> {
    float intervalCheckWave;
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
                Debug.Log("ITEM.type = " + item.NextType);
                Unit attack = item.square as Unit;
                Unit defender = SelectAttackTarget();
                if(defender)
                {
                    //Skill skill = new Skill();
                    Skill skill = SkillFactory.getSkillById(item.NextType);
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
        List<Item> enemys = LevelManager.THIS.GetEnemyItems();
        if(enemys.Count > 0)
        {
            while (count++ < 100)
            {
                //int row = Random.Range(0, LevelManager.THIS.opponentRows - 1);
                //int col = Random.Range(0, LevelManager.THIS.maxCols - 1);
                //unit = LevelManager.THIS.GetSquare(col, row) as Unit;
                int randomItemIndex = Random.Range(0, enemys.Count);
                Item item = enemys[randomItemIndex];
                if (item && item.square)
                {
                    unit = enemys[randomItemIndex].square as Unit;
                    if (unit)
                    {
                        break;
                    }
                }

            }
        }

        return unit;
    }

    public void Update()
    {
        intervalCheckWave += Time.deltaTime;
        if(intervalCheckWave > 2)
        {
            intervalCheckWave = 0;
            //先判断游戏是否正在进行
            if (LevelManager.THIS.gameStatus == GameState.Playing)
            {
                //为避免两拨怪间隔时间过短，一次消除会攻击下一波怪的情形，增加刷新间隔，每隔2秒检查一次
                //判断是否需要刷新下一轮怪物
                if (LevelManager.Instance.GetEnemyItems().Count == 0 && GameData.Instance.levelinit)
                {
                    if (++GameData.Instance.levelData.currentWave > 3)
                    {
                        //胜利,显示胜利界面，关卡结算
                        OnSuccess();
                        return;
                    }
                    Debug.Log("GameData.Instance.levelData.currentWave = " + GameData.Instance.levelData.currentWave);
                    //刷新下一轮怪物
                    LevelManager.Instance.GenerateNewEnemys(false);
                    GameData.Instance.levelData.currentWave++;
                }
            }
        }
    }

    //胜利界面
    public void OnSuccess()
    {
        Debug.Log("大吉大利，今晚吃鸡");
    }
}
