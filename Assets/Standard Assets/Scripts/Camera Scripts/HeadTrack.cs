using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeadTrack : MonoBehaviour {

	public Transform Target;

	Vector3 headPosition = new Vector3(0,0,0);

	Vector2 firstPoint;
	Vector2 secondPoint;
	Vector2 topPoint;

	public int IRWidth = 1024;
	public int IRHeight = 768;
	private float headX = 512; // x coordinate center
	private float headY = 384; // y coordinate center
	private float headDist = 5;

	const float radiansPerPixel = (float)(Mathf.PI / 4.0f) / 1024.0f;
	const float dotDistanceInMM = 162.0f;
	const float screenHeightInMM = 204.0f;

	private UDPReceive ur;


	// Use this for initialization
	void Start () {
		ur = GameObject.Find ("UDPReceive").GetComponentInChildren<UDPReceive> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (updateHead ()) {
			Vector3 newHeadPosition = new Vector3 (headX, headY, headDist);
			Vector3 translation = newHeadPosition - headPosition;

			transform.Translate (translation);

			Vector3 cross = Vector3.Cross (transform.position, Target.position);

			float w = (float) (Math.Sqrt(Vector3.SqrMagnitude(transform.position) * Vector3.SqrMagnitude(Target.position))
				+ Vector3.Dot(transform.position, Target.position));
			Quaternion rotation = new Quaternion (cross.x, cross.y, cross.z, w);

			transform.rotation = rotation;

			headPosition = newHeadPosition;
		}
	}

	bool updateHead () {
		ArrayList temp = filterIRInfo(ur.getLatestUDPPacket());
		firstPoint.x = (float)temp [3];
		firstPoint.y = (float)temp [4];
		secondPoint.x = (float)temp [5];
		secondPoint.y = (float)temp [6];
		topPoint.x = (float)temp [7];
		topPoint.y = (float)temp [8];

		if (firstPoint.x == -1 || firstPoint.y == -1 ||
			secondPoint.x == -1 || secondPoint.y == -1 ||
			topPoint.x == -1 || topPoint.y == -1)
			return false;

		float dx = firstPoint.x - secondPoint.x;
		float dy = firstPoint.y - secondPoint.y;
		float pointDist = (float)Math.Sqrt(dx * dx + dy * dy);

		float angle = radiansPerPixel * pointDist / 2;

		headDist = (float)((dotDistanceInMM / 2) / Math.Tan (angle)) / screenHeightInMM;

		float avgX = (firstPoint.x + secondPoint.x) / 2.0f;
		float avgY = (firstPoint.y + secondPoint.y) / 2.0f;

		headX = (float)(Math.Sin(radiansPerPixel * (avgX - 512)) * headDist);

		float relativeVerticalAngle = (avgY - 384) * radiansPerPixel;

		headY = .5f + (float)(Math.Sin(relativeVerticalAngle) * headDist);

		return true;
	}

	ArrayList filterIRInfo(String packet)
	{
		ArrayList headsetBlobX = new ArrayList();
		ArrayList headsetBlobY = new ArrayList();
		ArrayList headsetBlobSize = new ArrayList();

		ArrayList remainingIndexes = new ArrayList{0,1,2,3};

		ArrayList finalInformationList = new ArrayList ();

		string blob1 = "";
		//string packet = ur.getLatestUDPPacket();
		//Debug.Log (packet);
		char[] delimiterChars = { ',' };
		string[] parse = packet.Split (delimiterChars);

		for (int i = 1; i <= 10; i += 3) {
			headsetBlobX.Add (float.Parse (parse [i]));
		}

		for (int i = 2; i <= 11; i += 3) {
			headsetBlobY.Add (float.Parse (parse [i]));
		}

		for (int i = 3; i <= 12; i += 3) {
			headsetBlobSize.Add (float.Parse (parse [i]));
		}

		float biggestSize = -1;
		int biggestSizeIndex = -1;
		for (int i = 0; i < 4; i++) {
			if ((float)(headsetBlobSize[i]) > biggestSize) {
				biggestSizeIndex = i;
				biggestSize = (float) headsetBlobSize[i];
			}
		}

		finalInformationList.Add (headsetBlobX [biggestSizeIndex]);
		finalInformationList.Add (headsetBlobY [biggestSizeIndex]);
		finalInformationList.Add (headsetBlobSize [biggestSizeIndex]);

		remainingIndexes.Remove (biggestSizeIndex);

		// clicking x,y,blobsize are first 3 parameters in final list now 

		float smallestSize = (float)headsetBlobX[0];
		int smallestSizeIndex = 0;
		foreach (int i in remainingIndexes) {
			if ((float)headsetBlobX[i] < smallestSize) {
				smallestSizeIndex = i;
				smallestSize = (float) headsetBlobX[i];
			}
		}

		finalInformationList.Add (headsetBlobX [smallestSizeIndex]);
		finalInformationList.Add (headsetBlobY [smallestSizeIndex]);

		remainingIndexes.Remove (smallestSizeIndex);

		// clicking x,y,blobsize, left side x,y are in the final list now

		int largestSizeIndex = -1;
		int firstIndex = (int) remainingIndexes [0];
		int secondIndex = (int) remainingIndexes [1];
		if ((float) (headsetBlobX [firstIndex]) > (float)(headsetBlobX [secondIndex])) {
			largestSizeIndex = 0;
		} else {
			largestSizeIndex = 1;
		}

		finalInformationList.Add (headsetBlobX [largestSizeIndex]);
		finalInformationList.Add (headsetBlobY [largestSizeIndex]);

		remainingIndexes.Remove (largestSizeIndex);

		// clicking x,y,blocksize, left side x,y,  right side x,y are in the final list now

		int middleIndex = (int) remainingIndexes[0];
		finalInformationList.Add (headsetBlobX [middleIndex]);
		finalInformationList.Add (headsetBlobY [middleIndex]);
		//		for (int i = 0; i < 9; i++) {
		//			Debug.Log (finalInformationList [i]);
		//		}

		return finalInformationList;
	}
}
