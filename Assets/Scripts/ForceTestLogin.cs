using UnityEngine;
using KiiCorp.Cloud.Storage;
using System.Collections;

public class ForceTestLogin : MonoBehaviour {
    public string appId;
    public string appKey;

	// Use this for initialization
	void Awake () {
	    if (Login.User == null) {
            Kii.Initialize(appId, appKey, Kii.Site.US);
            KiiUser u = KiiUser.LogIn("chris", "stupidface");
            Login.User = u;
        }
  	}
}
