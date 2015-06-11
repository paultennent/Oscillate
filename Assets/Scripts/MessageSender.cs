using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;

public class MessageSender : MonoBehaviour {

	private TcpListener tcpListener;
	private Thread listenThread;
	private RotateAroundObject roa;
	private bool alive = true;

	// Use this for initialization
	void Start () {
		GameObject swing = GameObject.Find ("Swing");
		roa = swing.GetComponent <RotateAroundObject> ();

		this.tcpListener = new TcpListener(IPAddress.Any, 3000);
		this.listenThread = new Thread(new ThreadStart(ListenForClients));
		this.listenThread.Start();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void ListenForClients()
	{
		this.tcpListener.Start();
		
		while (true)
		{
			//blocks until a client has connected to the server
			TcpClient client = this.tcpListener.AcceptTcpClient();
			
			//create a thread to handle communication 
			//with connected client
			Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
			clientThread.Start(client);
		}
	}

	private void HandleClientComm(object client)
	{
		TcpClient tcpClient = (TcpClient)client;
		NetworkStream clientStream = tcpClient.GetStream();
		
		byte[] message = new byte[4096];
		int bytesRead;
		
		while (alive)
		{
			bytesRead = 0;
			
			try
			{
				//blocks until a client sends a message
				bytesRead = clientStream.Read(message, 0, 4096);
			}
			catch
			{
				//a socket error has occured
				break;
			}
			
			if (bytesRead == 0)
			{
				//the client has disconnected from the server
				break;
			}
			
			//message has successfully been received
			ASCIIEncoding encoder = new ASCIIEncoding();
			string data = encoder.GetString(message, 0, bytesRead);
			print (data);
			bool first = true;
			while (alive)
			{	
				//get the message here
				byte[] buffer;
				if (first){
					buffer = encoder.GetBytes("QUERYGOOD\n");
					first = false;
				}
				else{
					buffer = encoder.GetBytes(roa.getAngleString());
				}

				try
				{
					//blocks until a client sends a message
					clientStream.Write(buffer, 0 , buffer.Length);
					clientStream.Flush();;
				}
				catch
				{
					//a socket error has occured
					break;
				}
				
				if (bytesRead == 0)
				{
					//the client has disconnected from the server
					break;
				}
				Thread.Sleep(50);
			}
		}
		tcpClient.Close();
	}

	void OnApplicationQuit() {
		alive = false;
	}
}
