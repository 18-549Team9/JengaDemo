#pragma strict
var count = 0.0;
var triggerTime = 10.0;
function Start () {

}

function Update () {
count = count + (1 * Time.deltaTime);
if (count > triggerTime)
	{
	Destroy(gameObject);
	}
}