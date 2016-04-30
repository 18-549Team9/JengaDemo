
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
using System.Net.NetworkInformation;
using System.Threading;
using System.Linq;

public class UDPReceive : MonoBehaviour {

	// receiving Thread
	Thread receiveThread;

	// udpclient object
	UdpClient client;

	public int port;

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

		port = 12345;

		Dictionary<string,string> dict = new Dictionary<string,string>
		{
			{"ip", Network.player.ipAddress.ToString()},
			{"port", port.ToString() }
		};

		WWW requestPOST = hr.POST("raspberrypi.local/start", dict);

		yield return requestPOST;

		init ();

		requestGET = hr.GET("raspberrypi.local/status");
		yield return requestGET;

	}
		

	// init
	private void init()
	{
		receiveThread = new Thread(
			new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();

	}

	// receive thread
	private  void ReceiveData()
	{

		//Debug.Log ("receiving data");
		client = new UdpClient(port);
		//Debug.Log ("successfully started client");
		while (true)
		{	
			//Debug.Log ("going inside loop");
			try
			{
				//Debug.Log ("inside try");
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
				//Debug.Log ("right before client.receive");

				byte[] data = client.Receive(ref anyIP);
				//Debug.Log ("after client.receive");
			
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
		
	public ArrayList filterIRInfo()
	{
		string packet = getLatestUDPPacket ();

		ArrayList finalInformationList = new ArrayList ();
		ArrayList headsetBlobX = new ArrayList ();
		ArrayList headsetBlobY = new ArrayList ();
		ArrayList headsetBlobSize = new ArrayList ();
		ArrayList remainingIndexes = new ArrayList{ 0, 1, 2, 3 };

		string newPacket = packet.Substring (1, packet.Length - 3);
		char[] delimiterChars = { ',' };
		string[] parse = newPacket.Split (delimiterChars);

		for (int i = 1; i <= 10; i += 3) {
			headsetBlobX.Add (float.Parse (parse [i]));
		}

		for (int i = 2; i <= 11; i += 3) {
			headsetBlobY.Add (float.Parse (parse [i]));
		}

		for (int i = 3; i <= 12; i += 3) {
			headsetBlobSize.Add (float.Parse (parse [i]));
		}

		float biggestSize = (float)headsetBlobSize [0];
		int biggestSizeIndex = 0;
		for (int i = 0; i < 4; i++) {
			if ((float)(headsetBlobSize [i]) > biggestSize) {
				biggestSizeIndex = i;
				biggestSize = (float)headsetBlobSize [i];
			}
		}

		finalInformationList.Add (headsetBlobX [biggestSizeIndex]);
		finalInformationList.Add (headsetBlobY [biggestSizeIndex]);
		finalInformationList.Add (headsetBlobSize [biggestSizeIndex]);

		remainingIndexes.Remove (biggestSizeIndex);

		// clicking x,y,blobsize are first 3 parameters in final list now 

		float smallestSize = (float)headsetBlobX [0];
		int smallestSizeIndex = 0;
		foreach (int i in remainingIndexes) {
			if ((float)headsetBlobX [i] < smallestSize) {
				smallestSizeIndex = i;
				smallestSize = (float)headsetBlobX [i];
			}
		}

		finalInformationList.Add (headsetBlobX [smallestSizeIndex]);
		finalInformationList.Add (headsetBlobY [smallestSizeIndex]);

		remainingIndexes.Remove (smallestSizeIndex);

		// clicking x,y,blobsize, left side x,y are in the final list now

		int largestSizeIndex = -1;
		int firstIndex = (int)remainingIndexes [0];
		int secondIndex = (int)remainingIndexes [1];
		if ((float)(headsetBlobX [firstIndex]) > (float)(headsetBlobX [secondIndex])) {
			largestSizeIndex = 0;
		} else {
			largestSizeIndex = 1;
		}

		finalInformationList.Add (headsetBlobX [largestSizeIndex]);
		finalInformationList.Add (headsetBlobY [largestSizeIndex]);

		remainingIndexes.Remove (largestSizeIndex);

		// clicking x,y,blocksize, left side x,y,  right side x,y are in the final list now

		int middleIndex = (int)remainingIndexes [0];
		finalInformationList.Add (headsetBlobX [middleIndex]);
		finalInformationList.Add (headsetBlobY [middleIndex]);
		//		for (int i = 0; i < 9; i++) {
		//			Debug.Log (finalInformationList [i]);
		//		}


		StringBuilder sb = new StringBuilder();
		foreach (object obj in finalInformationList) {
			sb.Append(obj);
			sb.Append (", ");
		}
		string s = sb.ToString();
		Debug.Log (s);

		return finalInformationList;
	}
}