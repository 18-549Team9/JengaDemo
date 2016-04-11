using UnityEngine;
using System.Collections;

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
	public bool didReadSuccess = false;
	public bool buttonPress = false;
	public Vector2 position;

	void Start()
	{
		Cursor.visible = false;
		mainCamera = Camera.main;
		ur = GameObject.Find ("UDPReceive").GetComponentInChildren<UDPReceive> ();

		// top left (0,0)
		// bottom right (1000,760)
		int IRWidth = 1000;
		int IRHeight = 760;
		// bottom left: (0,0)
		// top right: (Screen.width, Screen.height)


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
				didReadSuccess = false;
			} else {
				// IR LED detected, record the coordinate
				if (intBlob >= 4) {
					buttonPress = true;
				}
				param1 = float.Parse (parse [1]);
				param2 = float.Parse (parse [2]);
				if (param1 > 10 && param2 > 10) {
					// Some stupid bug with initialization, use 10 to guarantee the IR LED is on
					didReadSuccess = true;
				}
			}
		}

	}


	void DrawQuad(Rect position, Color color) {
		Texture2D texture = new Texture2D(1, 1);
		texture.SetPixel(0,0,color);
		texture.Apply();
		GUI.skin.box.normal.background = texture;
		GUI.Box(position, GUIContent.none);
	}

	void Update ()
	{
		float temp1 = 0f;
		float temp2 = 0f;
		UDPUpdateXY (ref temp1, ref temp2);
		if (didReadSuccess) {
			x = (int) (temp1 * xScaling);
			y = (int) (temp2 * yScaling);
		}
		position = new Vector2 (x, y);

		// Make sure the user pressed the mouse down
		if (!buttonPress)
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
		while (buttonPress)
		{
			Ray ray = mainCamera.ScreenPointToRay (position);
			springJoint.transform.position = ray.GetPoint(distance);
			yield return null;
		}
		if (springJoint.connectedBody)
		{
			springJoint.connectedBody.drag = oldDrag;
			springJoint.connectedBody.angularDrag = oldAngularDrag;
			springJoint.connectedBody = null;
		}
	}
		
}