using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour {
    public Vector3 target;
    public float speed = 3;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
        float distance = Vector3.Distance(transform.position, target);
        if(distance < .01)
        {
            Destroy(gameObject);
        }
	}
}
