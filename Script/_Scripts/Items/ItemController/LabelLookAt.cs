using UnityEngine;
using System.Collections;

public class LabelLookAt : MonoBehaviour {

	Camera cam;
	Transform trans;

	public bool show;

	// Use this for initialization
	void Start () {
		cam = Camera.main;
		trans = transform;
	}
	
	// Update is called once per frame
	void Update () {
		//If the label is to be shown then make sure the label is always looking at the camera
		if(show) {
			trans.LookAt(cam.transform);
			trans.position = trans.root.position + Vector3.up;
		}
	}
}
