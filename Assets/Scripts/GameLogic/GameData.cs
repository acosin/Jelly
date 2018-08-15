using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData
{
    public int currentWave;
    public bool isEndLevel;
    public void init()
    {
        currentWave = 0;
        isEndLevel = false;
    }
}


public class GameData : LSingleton<GameData>
{
    public LevelData levelData;
    public bool levelinit;
    public void init()
    {
        if(null == levelData)
        {
            levelData = new LevelData();
            levelData.init();
        }

        levelinit = true;
    }

    public void reset()
    {
        levelinit = false;
        levelData = null;
    }
}