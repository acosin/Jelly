using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicFly : MonoBehaviour {
    public Vector3 targetPos;
    public Unit targetUnit;
    public float speed = 3;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(targetUnit)
        {
            Vector3 position = Vector3.MoveTowards(transform.position, targetUnit.transform.position, speed * Time.deltaTime);
            float distance = Vector3.Distance(position, targetUnit.transform.position);
            //position.z = 5;
            transform.position = position;
            //transform.RotateAround(Vector3.up, 360 * Time.deltaTime);
            Debug.LogWarning("distance = " + distance + "transform.pos=" + transform.position + "; target.pos = " + targetUnit.transform.position);
            if (distance < 0.01)
            {
                Destroy(gameObject);
                if(targetUnit.item)
                {
                    targetUnit.item.DestroyItem();
                }

            }
        }

	}
}
