using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBattleUnit  {



    void playHit();

    void attackOver();

    void runningOver();

    void playRunning();

    void prepareOver();

    void playPrepare();

    void addWithBuff(int buffid);
}
