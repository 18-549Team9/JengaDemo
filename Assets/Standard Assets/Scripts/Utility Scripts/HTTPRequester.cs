using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HTTPRequester : MonoBehaviour {

	// Use this for initialization
	void Start () {}

	void Update () {}

	public WWW GET(String url)
	{
		WWW www = new WWW (url);
		StartCoroutine (WaitForRequest (www));
		return www; 
	}

	public WWW POST(String url, Dictionary<string,string> request)
	{
		WWWForm form = new WWWForm();
		foreach(KeyValuePair<String,String> post_arg in post)
		{
			form.AddField(post_arg.Key, post_arg.Value);
		}
		WWW www = new WWW(url, form);
		StartCoroutine(WaitForRequest(www));
		return www; 
	}
		
	private IEnumerator WaitForRequest(WWW www)
	{
		yield return www;
		// check for errors
		if (www.error == null)
		{
			Debug.Log("WWW Ok!: " + www.text);
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}    
	}
}
