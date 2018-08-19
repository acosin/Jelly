using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CLevelDesc
{
    public int MaxTime;
    public int MinTime;
    public List<int> MoneyArray;
    public int MonsterHint;
    public int SoldierUnlock;
}
[System.Serializable]
public class CGenerateStrategy
{
    public string CanGenerateBone;
    public string Prioity;
}
[System.Serializable]
public class CMonster
{
    public int MonsterCount;
    public int MonsterID;
}
[System.Serializable]
public class CWave
{
    public CGenerateStrategy GenerateStrategy;
    public string InitEnemyID;
    public List<CMonster> MonsterArray;
    public List<int> InitEnemyIDList;
    public List<int> GetInitEnemyIDList()
    {
        if(InitEnemyIDList == null)
        {
            InitEnemyIDList = new List<int>();
            string[] ids = InitEnemyID.Split(',');
            foreach (string id in ids)
            {
                Debug.Log("id = " + id);
                int enmeyId = 0;
                if(id != "")
                {
                    enmeyId = int.Parse(id);
                }

                InitEnemyIDList.Add(enmeyId);
            }
        }
        return InitEnemyIDList;
    }
    public int GetInitEnemyID(int col, int row)
    {
        int index = col + row * LevelManager.Instance.maxCols;
        int enemyId = -1;
        if(index < GetInitEnemyIDList().Count)
        {
            enemyId = GetInitEnemyIDList()[index];
        }
        return enemyId;
    }
}

[System.Serializable]
public class CLevelConfig {

    public CLevelDesc LevelDesc;
    public List<CWave> WaveDesc;
}



