using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLevelDesc
{
    public int MaxTime;
    public int MinTime;
    public List<int> MoneyArray;
    public int MonsterHint;
    public int SoldierUnlock;
}

public class CGenerateStrategy
{
    public string CanGenerateBone;
    public string Prioity;
}

public class CMonster
{
    public int MonsterCount;
    public int MonsterID;
}
public class CWave
{
    public CGenerateStrategy GenerateStrategy;
    public string InitEnemyID;
    public List<CMonster> MonsterArray;
}


public class CLevelConfig {

    public CLevelDesc LevelDesc;
    public List<CWave> WaveDesc;
}



