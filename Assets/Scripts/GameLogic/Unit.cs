using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Unit : Square, IMatchUnit,IBattleUnit {
    public int life = 3;
    public void OnHarm(int harm)
    {
        if(harm >= life)
        {
            item.DestroyItem(false, "2222", true);
        }
        else
        {
            life -= harm;
        }

        Object obj = Resources.Load("damage");
        GameObject damage = Object.Instantiate(obj) as GameObject;
        Vector3 position = transform.position;
        position.z += 1;
        damage.transform.position = position;
        Vector3 target = damage.transform.position;
        target.y += 1;
        Move move = damage.GetComponent<Move>();
        move.target = target;

        TextMeshPro mesh = damage.GetComponent<TextMeshPro>();
        mesh.text = "-" + harm.ToString();
    }
    public void playHit()
    {
        
    }

    public void attackOver()
    {
        throw new System.NotImplementedException();
    }

    public void runningOver()
    {
        throw new System.NotImplementedException();
    }

    public void playRunning()
    {
        throw new System.NotImplementedException();
    }

    public void prepareOver()
    {
        throw new System.NotImplementedException();
    }

    public void playPrepare()
    {
        throw new System.NotImplementedException();
    }

    public void addWithBuff(int buffid)
    {
        throw new System.NotImplementedException();
    }
}
