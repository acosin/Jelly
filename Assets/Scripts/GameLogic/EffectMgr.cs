using System.Collections.Generic;
using UnityEngine;

/**
* @brief 特效数据
*/
class EffectMgr : LSingleton<EffectMgr>
{
    protected Vector3 mEnemyCenterPos = Vector3.zero;

    public void initEnemyCenterEffect()
    {
        Unit unit = null;
        List<Item> enemys = LevelManager.THIS.GetEnemyItems();

        //for (int row = 0; row < LevelManager.THIS.maxRows; row++)
        //{
        //    for (int col = 0; col < LevelManager.THIS.maxCols; col++)
        //    {

        //    }
        //}

        Square square = LevelManager.THIS.GetSquare(LevelManager.THIS.maxCols / 2, LevelManager.THIS.maxRows / 2);

        if (square != null && square.item != null)
        {
            this.mEnemyCenterPos = square.transform.position;
        }
    }

    public void playSceneEffect()
    {
        Object prefab = Resources.Load("star");

        GameObject instance = Object.Instantiate(prefab) as GameObject;
        instance.transform.parent = LevelManager.THIS.GameField;
        instance.transform.position = this.mEnemyCenterPos;
    }

    public void playSceneEffectByType(ItemsTypes itemsTypes)
    {
        if(ItemsTypes.HORIZONTAL_STRIPPED == itemsTypes)
        {
            //this.playSceneEffect();
        }
    }
}