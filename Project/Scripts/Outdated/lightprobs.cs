using UnityEngine;
using System.Collections;

public class lightprobs : MonoBehaviour {
    public GameObject lightprob;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	if (Input.GetMouseButtonDown(1))
        {
            GameObject.Instantiate(lightprob, this.transform.position, Quaternion.identity);
        }
	}
}
