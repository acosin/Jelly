using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData
{
    public int currentWave;

    public void init()
    {
        currentWave = -1;
    }
}


public class GameData : LSingleton<GameData>
{
    public LevelData levelData;

    public void init()
    {
        if(null == levelData)
        {
            levelData = new LevelData();
            levelData.init();
        }

    }
}