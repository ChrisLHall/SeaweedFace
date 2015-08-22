using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {
    public InputMgr InputMgr {get; private set;}
	// Use this for initialization
	void Awake () {
        InputMgr = GetComponent<InputMgr>();
  	}
}
