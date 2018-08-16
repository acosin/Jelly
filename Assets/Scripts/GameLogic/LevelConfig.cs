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
}

[System.Serializable]
public class CLevelConfig {

    public CLevelDesc LevelDesc;
    public List<CWave> WaveDesc;
}



