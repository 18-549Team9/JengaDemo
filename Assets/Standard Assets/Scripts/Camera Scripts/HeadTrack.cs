using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeadTrack : MonoBehaviour {

	public Transform Target;

	bool firstPass = true;
	Vector3 headPosition = new Vector3(0,0,0);

	Vector2 firstPoint;
	Vector2 secondPoint;
	Vector2 topPoint;

	public int IRWidth = 1024;
	public int IRHeight = 768;
	private float headX = 512; // x coordinate center
	private float headY = 384; // y coordinate center
	private float headDist = 15;

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
			Vector3 newHeadPosition;
			if (firstPass) {
				newHeadPosition = new Vector3 (headX, headY, headDist);
				headPosition = newHeadPosition;
				firstPass = false;
			}
			Debug.Log (headX + ", " + headY + ", " + headDist);
			newHeadPosition = new Vector3 (headX, headY, headDist);
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
		ArrayList temp = ur.filterIRInfo();
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
}
