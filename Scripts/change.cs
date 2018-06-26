using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class change : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void changeColor()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }
}
