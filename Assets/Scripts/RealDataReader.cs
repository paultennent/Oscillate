using UnityEngine;
using System.Collections;
using Phidgets;
using Phidgets.Events;

public class RealDataReader : MonoBehaviour {

	Spatial spatial;
	
	private volatile float[]  accNow = {0f,0f,0f};
	private volatile float[] gyroNow = {0f,0f,0f};
	private volatile float[] magNow = {0f,0f,0f};

	private volatile double[] lastMsCount = { 0, 0, 0 };
	private volatile bool[] lastMsCountGood = { false, false, false };
	private volatile double[] gyroHeading = { 0, 0, 0 }; //degrees
	
	public float[] getAccNow(){
		return accNow;
	}
	
	public float[] getGyroNow(){
		//return gyroNow;
		float[] xyz = new float[3];
		xyz [0] = (float)(gyroHeading [0]);
		xyz [1] = (float)(gyroHeading [1]);
		xyz [2] = (float)(gyroHeading [2]);
		return xyz;
	}

	public float[] getMagNow(){
		return gyroNow;
	}



	public void dumpVals(){
		print (accNow[0]+":"+accNow[1]+":"+accNow[2]+":"+gyroNow[0]+":"+gyroNow[1]+":"+gyroNow[2]+":"+magNow[0]+":"+magNow[1]+":"+magNow[2]+":");
	}

	// Use this for initialization
	void Start () {
		spatial = new Spatial ();
		spatial.open ();
		spatial.waitForAttachment (1000);
		spatial.DataRate = 8; // set datarate to 64Hz - framerate shouldn't be higher than this anyway
		spatial.SpatialData += new SpatialDataEventHandler(spatial_SpatialData);
	}

	void spatial_SpatialData(object sender, SpatialDataEventArgs e){
		accNow [0] = (float) e.spatialData [0].Acceleration [0];
		accNow [1] = (float) e.spatialData[0].Acceleration[1];
		accNow [2] = (float) e.spatialData[0].Acceleration[2];

		calculateGyroHeading(e.spatialData, 0); //x axis
		calculateGyroHeading(e.spatialData, 1); //y axis
		calculateGyroHeading(e.spatialData, 2); //z axis
		//print (gyroHeading [0]+","+e.spatialData.Length);
	}

	void calculateGyroHeading(SpatialEventData[] data, int index)
	{
		double gyro = 0;
		for (int i = 0; i < data.Length; i++)
		{
			gyro = data[i].AngularRate[index];
			
			if (lastMsCountGood[index])
			{
				//calculate heading
				double timechange = (double)data[i].Timestamp.TotalMilliseconds - lastMsCount[index]; // in ms
				double timeChangeSeconds = (double)timechange / 1000.0;
				
				//if (index == 0)
				//    Console.WriteLine("X Gyro rate: " + gyro.ToString() + " Time: " + timeChangeSeconds.ToString() + ", " + data[i].Timestamp.TotalMilliseconds.ToString());
				
				gyroHeading[index] += timeChangeSeconds * gyro;
			}
			
			lastMsCount[index] = data[i].Timestamp.TotalMilliseconds;
			lastMsCountGood[index] = true;
		}
	}
	
	// Update is called once per frame
	void Update () {

	}
}
