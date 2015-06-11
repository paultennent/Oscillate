using UnityEngine;
using System.Collections;

public class RotateAroundObject : MonoBehaviour {
	bool useLiveData = true;

	//private GameObject swing; 
	private GameObject swingPivot;
	private GameObject datareader;

	private bool debug = false;
	RealDataReader drScript;
	DataReader drScriptRecorded;

	private volatile float mycorrectedAngle = 0f;
	private volatile float myscaledAngle = 0f;

	private volatile float gX = 0f;
	private volatile float gY = 0f;
	private volatile float gZ = 0f;

	private volatile float myratio = 0f;
	
	private GyroAccelFilter errorFilter=new GyroAccelFilter();
	private SwingGameLogic gameLogic=new SwingGameLogic();

	// Use this for initialization 
	void Start () { 
	
		Application.runInBackground = true;
		//swing = GameObject.Find("Swing"); 
		swingPivot = GameObject.Find("SwingPivot");
		datareader = GameObject.Find ("DataHandler");

		if (useLiveData) {
			drScript = datareader.GetComponent<RealDataReader> ();
		} else {
			drScriptRecorded = datareader.GetComponent<DataReader> ();
			drScriptRecorded.Load();
		}
	}
	

	public string getAngleString(){
		return mycorrectedAngle + ":" + myscaledAngle + ":" + gX +":"+gY+":"+gZ+":"+myratio+"\n";
	}

	void dump(float[] f, string name){
		print (name + ":" + " x:" + f [0] + " y:" + f [1] + " z:" + f [2]);
	}

	float square(float f){
		return f * f;
	}

	public void offsetTexture(GameObject obj,float yOfs)
	{
		if (obj == null)
			return;
		//print (obj);
		foreach(Transform tran in obj.transform)
		{
			Renderer renderer=tran.gameObject.GetComponent<Renderer> ();
			Vector2 curOfs = renderer.material.mainTextureOffset;
			//print(""+yOfs+","+curOfs);
			curOfs.y = yOfs ;
//			curOfs.y = -yOfs / renderer.bounds.size.y;
			renderer.material.mainTextureOffset=curOfs;
		}
	}

	public void ColourFloor(float ratio)
	{
		// first and last 10% are stable
		ratio = Mathf.Clamp (ratio, 0.1f, 0.9f)-0.1f;
		ratio /= 0.8f;

		GameObject floor = GameObject.Find("Floor");

		float red = 1f - (ratio * 0.89f);
		float green = 0.6f - (ratio * 0.49f);
		float blue = ratio * 0.11f;

		floor.GetComponent<Renderer> ().material.color = new Color (red,green,blue);

		GameObject ceiling = GameObject.Find("Ceiling");
		foreach (Transform tran in ceiling.transform) 
		{
			Renderer renderer=tran.gameObject.GetComponent<Renderer> ();
			renderer.material.color=new Color (0.905f-ratio*0.81f, 1-ratio*0.89f, 1-ratio*0.89f);
		}

	}



	public void MoveFloor(float floorDropMultiplier){
		const float PIVOT_HEIGHT = 2.14f;
		const float WALL_SIZE = 3.7f;
		const float POST_SIZE = 6f;


		float totalDrop = PIVOT_HEIGHT * floorDropMultiplier;


		GameObject floor = GameObject.Find("Base");
		GameObject walls = GameObject.Find("ExpandingWalls");

		float floorPos = -(totalDrop -2.14f);
		floorPos = Mathf.Clamp (floorPos, -(WALL_SIZE * 3),0);
		
		Vector3 pos,scale;

		// drop floor
		Vector3 fpos = floor.transform.localPosition;
		fpos.y = floorPos;
		floor.transform.localPosition = fpos;


		walls = GameObject.Find ("ExpandingWall2");
		scale = walls.transform.localScale;
		scale.z = (WALL_SIZE-floorPos);
		walls.transform.localScale = scale;
		pos = walls.transform.localPosition;
		pos.y = 2.47f+floorPos*0.5f;
		walls.transform.localPosition = pos;

		walls = GameObject.Find ("ExpandingWall1");
		scale = walls.transform.localScale;
		scale.z = (WALL_SIZE-floorPos);
		walls.transform.localScale = scale;
		pos = walls.transform.localPosition;
		pos.y = 2.47f+floorPos*0.5f;
		walls.transform.localPosition = pos;
		walls = GameObject.Find ("ExpandingWall3");
		scale = walls.transform.localScale;
		scale.z = (WALL_SIZE-floorPos);
		walls.transform.localScale = scale;
		pos = walls.transform.localPosition;
		pos.y = 2.47f+floorPos*0.5f;
		walls.transform.localPosition = pos;
		walls = GameObject.Find ("ExpandingWall4");
		scale = walls.transform.localScale;
		scale.z = (WALL_SIZE-floorPos);
		walls.transform.localScale = scale;
		pos = walls.transform.localPosition;
		pos.y = 2.47f+floorPos*0.5f;
		walls.transform.localPosition = pos;

		walls = GameObject.Find ("SwingPost1");
		scale = walls.transform.localScale;
		scale.z = (POST_SIZE-floorPos);
		walls.transform.localScale = scale;
		pos = walls.transform.localPosition;
		pos.y = 1.37f+floorPos*0.5f;
		walls.transform.localPosition = pos;

		walls = GameObject.Find ("SwingPost2");
		scale = walls.transform.localScale;
		scale.z = (POST_SIZE-floorPos);
		walls.transform.localScale = scale;
		pos = walls.transform.localPosition;
		pos.y = 1.37f+floorPos*0.5f;
		walls.transform.localPosition = pos;
	}

	void resetEyes(){
		OVRManager.display.RecenterPose();
	}
	

	void Update () { 
		//float[] Axyz = drScript.getAccNow ();
		float[] Gxyz;
		double time ;
		float [] Gaccel;
		if (useLiveData) {
			Gxyz = drScript.getGyroNow ();
			time = Time.time;
			Gaccel = drScript.getAccNow ();
		} else {
			Gxyz = drScriptRecorded.getGyroNow ();
			time = drScriptRecorded.getTimeNow ();
			Gaccel = drScriptRecorded.getAccNow ();
		}

		gX = Gaccel [0];
		gY = Gaccel [1];
		gZ = Gaccel [2];

		float correctedAngle=errorFilter.addValue (time, Gxyz [0], Gaccel [2]);
		if (debug) {
			if (errorFilter.debugMessage.Length != 0) {
				print (errorFilter.debugMessage);
			}
		}

		gameLogic.onAngle (time, correctedAngle, errorFilter, Gxyz [0]);

		MoveFloor (gameLogic.floorDropMultiply);
		ColourFloor (gameLogic.climaxRatio);
		//print(gameLogic.gameTime+";"+gameLogic.swingMultiply+":"+gameLogic.climaxRatio+":"+correctedAngle);

		mycorrectedAngle = correctedAngle;
		correctedAngle *= gameLogic.swingMultiply;
		correctedAngle=Mathf.Clamp (correctedAngle, -124, 124);
		myratio = gameLogic.climaxRatio;

		myscaledAngle = correctedAngle;

		swingPivot.transform.localEulerAngles = new Vector3(-correctedAngle,0,0);

		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}

		if (Input.GetKeyDown(KeyCode.Return)) {
			errorFilter.reset(drScript.getGyroNow ()[0]);
			gameLogic.reset();
		}

	}

}