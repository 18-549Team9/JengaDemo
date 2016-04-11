
/*
 
    -----------------------
    UDP-Receive (send to)
    -----------------------
    // [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]
   
   
    // > receive
    // 127.0.0.1 : 8051
   
    // send
    // nc -u 127.0.0.1 8051
 
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceive : MonoBehaviour {

	// receiving Thread
	Thread receiveThread;

	// udpclient object
	UdpClient client;

	// public
	public string IP = "169.254.205.210"; 
	public int port; // define > init

	// infos
	public string lastReceivedUDPPacket="";
	public string allReceivedUDPPackets=""; // clean up this from time to time!

	private HTTPRequester hr;

	// start from shell
	private static void Main()
	{
		UDPReceive receiveObj=new UDPReceive();
		receiveObj.init();

		string text="";
		do
		{
			text = Console.ReadLine();
		}
		while(!text.Equals("exit"));
	}
	// start from unity3d
	public IEnumerator Start()
	{
		hr = GameObject.Find ("HTTPRequester").GetComponentInChildren<HTTPRequester> ();

		WWW requestGET = hr.GET("raspberrypi.local/status");
		yield return requestGET;

		Dictionary<string,string> dict = new Dictionary<string,string>
		{
			{"ip", "169.254.205.210"},
			{"port", "12486" }
		};
			

		WWW requestPOST = hr.POST("raspberrypi.local/start", dict);

		yield return requestPOST;


		init ();

		requestGET = hr.GET("raspberrypi.local/status");
		yield return requestGET;

	}

	// OnGUI
	void OnGUI()
	{
//		Rect rectObj=new Rect(40,10,200,400);
//		GUIStyle style = new GUIStyle();
//		style.alignment = TextAnchor.UpperLeft;
//		GUI.Box(rectObj,"# UDPReceive\n169.254.142.188 "+port+" #\n"
//			+ "shell> nc -u 169.254.142.188 : "+port+" \n"
//			+ "\nLast Packet: \n"+ lastReceivedUDPPacket
//			+ "\n\nAll Messages: \n"+allReceivedUDPPackets
//			,style);
	}

	// init
	private void init()
	{

		// define port
		port = 12486;

		// status
		//print("Sending to 169.254.142.188 : "+port);
		//print("Test-Sending to this Port: nc -ul 169.254.27.195  "+port+"");

		receiveThread = new Thread(
			new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();

	}

	// receive thread
	private  void ReceiveData()
	{

		Debug.Log ("receiving data");
		client = new UdpClient(port);
		Debug.Log ("successfully started client");
		while (true)
		{	
			Debug.Log ("going inside loop");
			try
			{
				Debug.Log ("inside try");
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
				Debug.Log ("right before client.receive");

				byte[] data = client.Receive(ref anyIP);
				Debug.Log ("after client.receive");
			
				string text = Encoding.UTF8.GetString(data);


				// latest UDPpacket
				lastReceivedUDPPacket=text;

			}
			catch (Exception err)
			{
				print(err.ToString());
			}
		}
	}

	IEnumerator OnApplicationQuit()
	{
		Dictionary<string,string> dict = new Dictionary<string,string> { };
		WWW requestPOST = hr.POST("raspberrypi.local/stop", dict);
		yield return requestPOST;
		//WWW requestGET = hr.GET("raspberrypi.local/status");
	}

	// getLatestUDPPacket
	// cleans up the rest
	public string getLatestUDPPacket()
	{
		allReceivedUDPPackets="";
		return lastReceivedUDPPacket;
	}
}