using UnityEngine;
using System.Collections;

public class Forces : MonoBehaviour {

	private GameObject datareader;
	private DataReader drScript; 
	private Rigidbody rb;

	// Use this for initialization
	void Start () {
		datareader = GameObject.Find("Pivot");
		drScript = datareader.GetComponent<DataReader>();
		rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		float[] acc = drScript.getAccNow ();
		rb.AddForce (acc [0], acc [1], acc [2]);
	}
}
