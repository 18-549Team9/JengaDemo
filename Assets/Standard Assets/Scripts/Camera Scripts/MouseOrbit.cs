using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using HTTPRequester;
//using UDPReceive;


public class MouseOrbit : MonoBehaviour
{
	public Transform Target;
	public float Distance = 5.0f;
	public float xSpeed = 250.0f;
	public float ySpeed = 120.0f;
	public float yMinLimit = -20.0f;
	public float yMaxLimit = 80.0f;

	private float x;
	private float y;

	private float readX;
	private float readY;

	public int IRWidth = 1000;
	public int IRHeight = 760;

	private float previousReadX = 500; // x coordinate center
	private float previousReadY = 380; // y coordinate center

	private int turnSpeed = 20; // higher for faster turn speed, lower for slower turn speed

	public Boolean didReadSuccess = false;

	private UDPReceive ur;

	void Start()
	{
		ur = GameObject.Find ("UDPReceive").GetComponentInChildren<UDPReceive> ();
	}

	void Awake()
	{
		Vector3 angles = transform.eulerAngles;
		x = angles.x;
		y = angles.y;

		if(GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().freezeRotation = true;
		}
	}

	void LateUpdate()
	{
		float temp1 = 0f;
		float temp2 = 0f;
		UDPUpdateXY (ref temp1, ref temp2);
		if (didReadSuccess == true) {
			readX = temp1;
			readY = temp2;

			// SOME CRAZY FORMULA, adjust turnSpeed at the top if you want to change the speed
			float dx = (previousReadX - readX) / (IRWidth / turnSpeed);
			float dy = (previousReadY - readY) / (IRHeight / turnSpeed);

			previousReadX = readX;
			previousReadY = readY;
			//float dx = (readX - (IRWidth / 2)) / (IRWidth / 2);
			//float dy = (readY - (IRHeight / 2)) / (IRHeight / 2);

			//Debug.Log (dx);
			//Debug.Log (dy);

			x += (float)(dx * xSpeed * 0.02f);
			y += (float)(dy * ySpeed * 0.02f);

			//x += (float)(Input.GetAxis("Mouse X") * xSpeed * 0.02f);
			//y -= (float)(Input.GetAxis("Mouse Y") * ySpeed * 0.02f);

			y = ClampAngle (y, yMinLimit, yMaxLimit);

			Quaternion rotation = Quaternion.Euler (y, x, 0);
			Vector3 position = rotation * (new Vector3 (0.0f, 0.0f, -Distance)) + Target.position;

			transform.rotation = rotation;
			transform.position = position;
		} else {
			x += (float)(0f * xSpeed * 0.02f);
			y -= (float)(0f * ySpeed * 0.02f);

			//x += (float)(Input.GetAxis("Mouse X") * xSpeed * 0.02f);
			//y -= (float)(Input.GetAxis("Mouse Y") * ySpeed * 0.02f);

			y = ClampAngle (y, yMinLimit, yMaxLimit);

			Quaternion rotation = Quaternion.Euler (y, x, 0);
			Vector3 position = rotation * (new Vector3 (0.0f, 0.0f, -Distance)) + Target.position;

			transform.rotation = rotation;
			transform.position = position;
		}
//		}
	}

	private float ClampAngle(float angle, float min, float max)
	{
		if(angle < -360)
		{
			angle += 360;
		}
		if(angle > 360)
		{
			angle -= 360;
		}
		return Mathf.Clamp (angle, min, max);
	}

	void UDPUpdateXY(ref float param1, ref float param2)
	{
		string blob1 = "";
		string packet = ur.getLatestUDPPacket();
		//Debug.Log ("packet:" + packet);
		if (packet != "") {
			char[] delimiterChars = { ',' };
			string[] parse = packet.Split (delimiterChars);
			blob1 = parse [1];
			int intBlob = int.Parse (parse [1]);

			if (intBlob == -1) {
				// Maybe change this later because it's a little sketch that I'm checking for -1
				// Put blob size instead?
				param1 = readX;
				param2 = readY;
				didReadSuccess = false;
			} else {
				// IR LED detected, record the coordinate
				param1 = float.Parse (parse [1]);
				param2 = float.Parse (parse [2]);


				if (param1 > 10 && param2 > 10) {
					// Some stupid bug with initialization, use 10 to guarantee the IR LED is on
					didReadSuccess = true;
				}
			}
		}

	}
}