using UnityEngine;
using System.Collections;

public class FacePlayer : MonoBehaviour {

	// Update is called once per frame
	void Update () {
        transform.LookAt(transform.position - Camera.main.transform.forward);
	}
}
