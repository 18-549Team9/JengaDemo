using UnityEngine;
using System.Collections;

public class FPS : MonoBehaviour
{


	// Attach this to a GUIText to make a frames/second indicator.
	//
	// It calculates frames/second over each updateInterval,
	// so the display does not keep changing wildly.
	//
	// It is also fairly accurate at very low FPS counts (<10).
	// We do this not by simply counting frames per interval, but
	// by accumulating FPS for each frame. This way we end up with
	// correct overall FPS even if the interval renders something like
	// 5.5 frames.

	/*
	public float updateInterval = 0.5F;

	private float accum = 0; // FPS accumulated over the interval
	private int frames = 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval
	*/

	// v2:

	public float frequency = 0.5f;
	public int FramesPerSec { get; protected set; }
	private string text;

	GUIStyle guiStyle = new GUIStyle();
	GUIStyle guiStyleOutline = new GUIStyle();

	void Start()
	{
		/*
		if (!guiText)
		{
			Debug.Log("UtilityFramesPerSecond needs a GUIText component!");
			enabled = false;
			return;
		}
		timeleft = updateInterval;
		 * */

		// v2:
		StartCoroutine(DoFPS());

		guiStyle.normal.textColor = Color.white;
		guiStyle.fontSize = 50;
		guiStyleOutline.normal.textColor = Color.black;
		guiStyleOutline.fontSize = 50;
	}

	void OnGUI()
	{
		//Vector2 p = new Vector2(0.5f*Screen.width, 0.01f*Screen.height);
		Vector2 p = new Vector2(10, 10);

		for (int x = -1; x <= +1; x++)
		{
			for (int y = -1; y <= +1; y++)
			{
				GUI.Label(new Rect(p.x + x, p.y + y, 100, 20), text, guiStyleOutline);
			}
		}
		GUI.Label(new Rect(p.x, p.y, 100, 20), text, guiStyle);
	}

	void Update()
	{
		/*
		timeleft -= Time.deltaTime;
		accum += Time.timeScale / Time.deltaTime;
		++frames;

		// Interval ended - update GUI text and start new interval
		if (timeleft <= 0.0)
		{
			// display two fractional digits (f2 format)
			float fps = accum / frames;
			string format = System.String.Format("{0:F2} FPS", fps);
			guiText.text = format;

			if (fps < 30)
				guiText.material.color = Color.yellow;
			else
				if (fps < 10)
					guiText.material.color = Color.red;
				else
					guiText.material.color = Color.green;
			//	DebugConsole.Log(format,level);
			timeleft = updateInterval;
			accum = 0.0F;
			frames = 0;
		}
		*/


	}

	private IEnumerator DoFPS()
	{
		for (; ; )
		{
			// Capture frame-per-second
			int lastFrameCount = Time.frameCount;
			float lastTime = Time.realtimeSinceStartup;
			yield return new WaitForSeconds(frequency);
			float timeSpan = Time.realtimeSinceStartup - lastTime;
			int frameCount = Time.frameCount - lastFrameCount;

			// Display it
			FramesPerSec = Mathf.RoundToInt(frameCount / timeSpan);
			//guiText.text = FramesPerSec.ToString() + " fps";
			text = FramesPerSec.ToString() + " fps";

			if (FramesPerSec > 60)
				guiStyle.normal.textColor = Color.green;
			else if (FramesPerSec > 30)
			{
				guiStyle.normal.textColor = Color.yellow;
			}
			else
			{
				guiStyle.normal.textColor = Color.red;
			}
		}
	}
}
