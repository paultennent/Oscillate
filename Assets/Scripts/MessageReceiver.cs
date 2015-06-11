using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using System.IO;


public class MessageReceiver : MonoBehaviour {

	private volatile float[] accs = {0f,0f,0f};
	private volatile float realAngle = 0f;
	private volatile float scaledAngle = 0f;
	private volatile float ratio = 0f;

	private TcpClient client;
	private Thread clientThread; 
	private bool alive = true;

	IPEndPoint serverEndPoint;
	// Use this for initialization
	void Start () {
		client = new TcpClient();
		serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);
		this.clientThread = new Thread(new ThreadStart(connect));
		this.clientThread.Start();
	}

	public float[] getAccs(){
		return accs;
	}

	public float getRealAngle(){
		return realAngle;
	}

	public float getScaledAngle(){
		return scaledAngle;
	}

	public float getRatio(){
		return ratio;
	}

	private void connect(){
		while (alive){
			try{
				client.Connect(serverEndPoint);
				
				NetworkStream clientStream = client.GetStream();
				
				ASCIIEncoding encoder = new ASCIIEncoding();
				byte[] buffer = encoder.GetBytes("0\n");
				
				clientStream.Write(buffer, 0 , buffer.Length);
				clientStream.Flush();


				using(StreamReader reader = new StreamReader(clientStream)) {
					string line;
					while((line = reader.ReadLine()) != null) {
						handleLine (line);
					}
				}
			}
			catch{
				print ("No server available");
			}
		}
	}

	void handleLine(string line){
		if (line == "QUERYGOOD") {
			return;
		}
		try{
			string[] data = line.Split (':');
			accs [0] = float.Parse (data [2].Trim ());
			accs [1] = float.Parse (data [3].Trim ());
			accs [2] = float.Parse (data [4].Trim ());

			realAngle = float.Parse (data [0].Trim ());
			scaledAngle = float.Parse (data [1].Trim ());
			ratio = float.Parse (data [5].Trim ());
		}
		catch{
			print ("Error handling line:");
		}

		//print (line);

	}

	void OnApplicationQuit() {
		alive = false;
		// Make sure prefs are saved before quitting.
		try{
			client.Close ();
		}
		catch{
			print ("Client already closed");
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
