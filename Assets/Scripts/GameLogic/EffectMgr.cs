using System.Collections.Generic;
using UnityEngine;
/**
* @brief 特效数据
*/
class EffectMgr : LSingleton<EffectMgr>
{
    protected Vector3 mEnemyCenterPos = new Vector3(0.0f, 2.2f, -10);

    public void initEnemyCenterEffect()
    {
        Unit unit = null;
        List<Item> enemys = EnemyManager.Instance.GetEnemyItems();

        //for (int row = 0; row < LevelManager.THIS.maxRows; row++)
        //{
        //    for (int col = 0; col < LevelManager.THIS.maxCols; col++)
        //    {

        //    }
        //}

        Square square = LevelManager.Instance.GetSquare(LevelManager.THIS.maxCols / 2, 1);

        if (square != null && square.item != null)
        {
            //this.mEnemyCenterPos = square.transform.position;
            //Debug.Log("this.mEnemyCenterPos =" + this.mEnemyCenterPos);
        }
    }

    public void playSceneEffect()
    {
        Object prefab = Resources.Load("Effect/Particle/Skill");
        //Object prefab = Resources.Load("Star");

        GameObject instance = Object.Instantiate(prefab) as GameObject;
        instance.name = "aaaaaaaa";
        instance.transform.parent = LevelManager.THIS.GameField;
        instance.transform.position = this.mEnemyCenterPos;
    }

    public void playSceneEffectByType(ItemsTypes itemsTypes)
    {
        if(ItemsTypes.HORIZONTAL_STRIPPED == itemsTypes ||
            ItemsTypes.VERTICAL_STRIPPED == itemsTypes)
        {
            this._hurtEnemy();
            this.playSceneEffect();
        }
    }

    protected void _hurtEnemy()
    {
        Unit leftUnit = null;
        Unit rightUnit = null;
        Unit unit = BattleSystem.Instance.SelectAttackTarget();
        Debug.Log("col = " + unit.col + "; row= " + unit.row);
        if(null != unit)
        {
            int harm = 7;
            unit.OnHarm(harm);
            this.mEnemyCenterPos = unit.transform.position;

            leftUnit = LevelManager.THIS.GetSquare(unit.col - 1, unit.row) as Unit;

            if(null != leftUnit)
            {
                leftUnit.OnHarm(harm);
            }

            rightUnit = LevelManager.THIS.GetSquare(unit.col + 1, unit.row) as Unit;

            if (null != rightUnit)
            {
                rightUnit.OnHarm(harm);
            }
        }
    }
}