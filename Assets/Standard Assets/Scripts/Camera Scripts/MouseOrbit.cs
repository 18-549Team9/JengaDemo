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

	private HTTPRequester hr;
	private UDPReceive ur;

	void Start()
	{
		WWW requestGET = hr.GET("raspberrypi.local/status");
		Debug.Log(requestGET.text);

		Dictionary<string,string> dict = new Dictionary<string,string>
		{
			{"ip", "169.254.152.188"},
			{"port", "12345" }
		};

		WWW requestPOST = hr.POST("rasberrypi.local/start", dict);

		requestGET = hr.GET("raspberrypi.local/status");
		Debug.Log(requestGET.text);
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
		if(Target != null)
		{
			x += (float)(Input.GetAxis("Mouse X") * xSpeed * 0.02f);
			y -= (float)(Input.GetAxis("Mouse Y") * ySpeed * 0.02f);

			y = ClampAngle(y, yMinLimit, yMaxLimit);

			Quaternion rotation = Quaternion.Euler(y, x, 0);
			Vector3 position = rotation * (new Vector3(0.0f, 0.0f, -Distance)) + Target.position;

			transform.rotation = rotation;
			transform.position = position;
		}
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

	void OnApplicationQuit()
	{
		Dictionary<string,string> dict = new Dictionary<string,string> { };
		WWW requestPOST = hr.POST("rasberrypi.local/stop", dict);
		WWW requestGET = hr.GET("raspberrypi.local/status");
		Debug.Log(requestGET.text);
	}

	void UDPUpdateXY(out float param1, out float param2)
	{
		string packet = ur.getLatestUDPPacket();

		char[] delimiterChars = { ' ', '(', ')' };
		string blob1 = packet.Split(delimiterChars) [1];
		if (blob1 == "None") {
			param1 = x;
			param2 = y;
		} else {
			char[] comma = { ',' };
			string[] xyblob = blob1.Split(comma);
			param1 = float.Parse(xyblob[0]);
			param2 = float.Parse(xyblob[1]);
		}
	}
}