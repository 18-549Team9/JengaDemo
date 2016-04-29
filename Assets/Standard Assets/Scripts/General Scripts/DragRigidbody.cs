using UnityEngine;
using System.Collections;
using System;

public class DragRigidbody : MonoBehaviour {

	public float spring = 50.0f;
	public float damper = 5.0f;
	public float drag = 10.0f;
	public float angularDrag = 5.0f;
	public float distance = 0.2f;
	public bool attachToCenterOfMass = false;
	public Camera mainCamera;
	public RaycastHit hit;

	private SpringJoint springJoint;
	private double xScaling;
	private double yScaling;
	private UDPReceive ur;

	private int x;
	private int y;
	private int blob;
	public bool didReadSuccess = false;
	public Vector2 position;

	bool getButtonPress() {
		if (blob >= 4)
			return true;
		return false;
	}

	void Start()
	{
		Cursor.visible = false;
		mainCamera = Camera.main;
		ur = GameObject.Find ("UDPReceive").GetComponentInChildren<UDPReceive> ();

		// top left (0,0)
		// bottom right (1000,760)
		int IRWidth = 1024;
		int IRHeight = 768;
		// bottom left: (0,0)
		// top right: (Screen.width, Screen.height)
		Debug.Log(Screen.width);
		Debug.Log (Screen.height);

		// for xScaling always subtract the scaled value from Screen.width
		xScaling = (double)Screen.width / (double)IRWidth;
		yScaling = (double)Screen.height / (double)IRHeight;

		Debug.Log (xScaling);
		Debug.Log (yScaling);

		x = Screen.width / 2;
		y = Screen.height / 2;
	
		
	}

	void OnGUI() {
		Rect rectObj = new Rect(x,Screen.height-y,10,10);
		DrawQuad (rectObj, Color.black);
	}

	//[103238, 816, 137, 4, 173, 90, 3, 484, 10, 3, 460, 412, 6]


	void DrawQuad(Rect position, Color color) {
		Texture2D texture = new Texture2D(1, 1);
		texture.SetPixel(0,0,color);
		texture.Apply();
		GUI.skin.box.normal.background = texture;
		GUI.Box(position, GUIContent.none);
	}

	void Update ()
	{
		string packet = ur.getLatestUDPPacket();
		if (packet.Equals ("")) {
			Debug.Log ("changing packet");
			packet = "[103238, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1]";
		}
		ArrayList temp = ur.filterIRInfo ();
		float temp1 = (float)temp[0];
		float temp2 = (float)temp[1];
		int temp3 = Convert.ToInt32(temp [2]);
		if (temp1 != -1 || temp2 != -1 || temp3 != -1) {
			x = (int) (temp1 * xScaling);
			y = Screen.height - (int) (temp2 * yScaling);
			blob = temp3;
		}
		//Debug.Log (x);
		//Debug.Log (y);

		position = new Vector2 (x, y);

		// Make sure the user pressed the mouse down
		if (!getButtonPress())
			return;

		// We need to actually hit an object
		if (!Physics.Raycast(mainCamera.ScreenPointToRay(position), out hit, 100))
			return;
		// We need to hit a rigidbody that is not kinematic
		if (!hit.rigidbody || hit.rigidbody.isKinematic)
			return;

		if (!springJoint)
		{
			GameObject go = new GameObject("Rigidbody dragger");
			Rigidbody body = go.AddComponent <Rigidbody>() as Rigidbody;
			springJoint = go.AddComponent<SpringJoint>() as SpringJoint;
			body.isKinematic = true;
		}

		springJoint.transform.position = hit.point;
		if (attachToCenterOfMass)
		{
			Vector3 anchor = transform.TransformDirection(hit.rigidbody.centerOfMass) + hit.rigidbody.transform.position;
			anchor = springJoint.transform.InverseTransformPoint(anchor);
			springJoint.anchor = anchor;
		}
		else
		{
			springJoint.anchor = Vector3.zero;
		}

		springJoint.spring = spring;
		springJoint.damper = damper;
		springJoint.maxDistance = distance;
		springJoint.connectedBody = hit.rigidbody;

		StartCoroutine(DragObject(hit.distance));
	}

	IEnumerator DragObject (float distance)
	{
		float oldDrag = springJoint.connectedBody.drag;
		float oldAngularDrag = springJoint.connectedBody.angularDrag;
		springJoint.connectedBody.drag = drag;
		springJoint.connectedBody.angularDrag = angularDrag;
		Camera mainCamera = Camera.main;
		while (this.getButtonPress())
		{
			Ray ray = mainCamera.ScreenPointToRay (position);
			springJoint.transform.position = ray.GetPoint(distance);
			yield return null;
		}
		Debug.Log ("reached");
		if (springJoint.connectedBody)
		{
			springJoint.connectedBody.drag = oldDrag;
			springJoint.connectedBody.angularDrag = oldAngularDrag;
			springJoint.connectedBody = null;
		}
	}
		
}