using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ConditionAction
{
    bool satisfy;
    bool isTrigged;
    int action;//满足条件后，执行的动作
}
public class Magic {
    public bool isDead;
    public float life;
    public uint effectid;
    public void tick(float delta)
    {

    }
}
