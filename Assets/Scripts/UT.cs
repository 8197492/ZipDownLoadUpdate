using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UT : MonoBehaviour
{
    public int aaaa = 0;
	// Use this for initialization
	void Start ()
    {
        Test.Instance.Log(aaaa.ToString());
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
