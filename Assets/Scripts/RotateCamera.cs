using UnityEngine;
using System.Collections;

public class RotateCamera : MonoBehaviour {

	GameObject anchor;
	GameObject swingPivot;
	GameObject pendulumscaled;
	GameObject pendulumreal;
	GameObject powerbar;
	float rotSpeed = 10f;
	float ratio =5f;
	MessageReceiver mr;

	bool flag = true;
	bool camflag = true;


	// Use this for initialization
	void Start () {
		Application.runInBackground = true;
		anchor = GameObject.Find("Anchor");
		mr = GameObject.Find ("DataHandler").GetComponent<MessageReceiver> ();
		swingPivot = GameObject.Find ("SwingPivot");
		pendulumscaled = GameObject.Find ("pendulumpivotscaled");
		pendulumreal = GameObject.Find ("pendulumpivotreal");
		powerbar = GameObject.Find ("PowerBar");
	}	

	// Update is called once per frame
	void Update () {
		transform.RotateAround (anchor.transform.position, Vector3.down, Time.deltaTime * rotSpeed);
		transform.RotateAround (anchor.transform.position, Vector3.left, ((Time.deltaTime * rotSpeed)/ratio));
		//transform.RotateAround (anchor.transform.position, Vector3.left, (Mathf.Sin((float) Time.time))/ratio);
	
		//move the swing
		swingPivot.transform.localEulerAngles = new Vector3(-mr.getScaledAngle(),0,0);
		//print ("Angle:"+mr.getScaledAngle());

		//time to change the GUI text
		if (flag) {
			GameObject.Find ("DataText").GetComponent<TextMesh> ().text = "Real Angle: " + Mathf.RoundToInt (-mr.getRealAngle ()) + "°\n\nScaled Angle: " + Mathf.RoundToInt (-mr.getScaledAngle ()) + "°\n\n\nX: " + (mr.getAccs () [0]).ToString ("F2") + " G\n\nY: " + (mr.getAccs () [1]).ToString ("F2") + " G \n\nZ: " + (mr.getAccs () [2]).ToString ("F2") + " G";
			pendulumscaled.transform.localEulerAngles = new Vector3 (0, 0, -mr.getScaledAngle ());
			pendulumreal.transform.localEulerAngles = new Vector3 (0, 0, -mr.getRealAngle ());

			//now the power bar
			float size = ((mr.getRatio ()) * 7.5f);
			powerbar.transform.localScale = new Vector3 (0.3f, size, 0.3f);
			Vector3 pos = powerbar.transform.localPosition;
			pos.x = size * 0.75f;
			powerbar.transform.localPosition = pos;
		}

		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}

		if (Input.GetKeyDown(KeyCode.D)) {

			GameObject.Find("DataText").GetComponent<Renderer>().enabled = !flag;
			GameObject.Find("PowerBar").GetComponent<Renderer>().enabled = !flag;
			GameObject.Find("Pendulum").GetComponent<Renderer>().enabled = !flag;
			GameObject.Find("Pendulumscaled").GetComponent<Renderer>().enabled = !flag;
			GameObject.Find("Face").GetComponent<Renderer>().enabled = !flag;
			flag = !flag;
		}

		if (Input.GetKeyDown(KeyCode.C)) {
			GameObject.Find("Monitor").GetComponent<Renderer>().enabled = !camflag;
			GameObject.Find("Screen").GetComponent<Renderer>().enabled = !camflag;
			camflag = !camflag;
		}

	}

}
