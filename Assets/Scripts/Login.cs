using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using KiiCorp.Cloud;
using KiiCorp.Cloud.Storage;

public class Login : MonoBehaviour {
    public string appId;
    public string appKey;
    
    InputField username;
    InputField password;
    Text error;
    string setErrText;

    bool loggedIn;
    public static KiiUser User { get; set; }

    void Awake () {
        username = GameObject.Find("Username").GetComponent<InputField>();
        password = GameObject.Find("Password").GetComponent<InputField>();
        error = GameObject.Find ("Error").GetComponent<Text>();
        loggedIn = false;
        User = null;
        setErrText = "";
        Kii.Initialize(appId, appKey, Kii.Site.US);
    }

	// Use this for initialization
	void Start () {
        if (PlayerPrefs.HasKey("username")) {
            username.text = PlayerPrefs.GetString("username");
        }
        if (PlayerPrefs.HasKey("password")) {
            password.text = PlayerPrefs.GetString("password");
        }
	}
	
	// Update is called once per frame
	void Update () {
        error.text = setErrText;

        if (loggedIn && User != null) {
            PlayerPrefs.SetString("username", username.text);
            PlayerPrefs.SetString("password", password.text);
            Application.LoadLevel("myIsland");
        }
	}

    bool ValidateLogin () {
        if (username.text.Trim().Length < 5) {
            error.text = "Username must be 5+ characters!";
            return false;
        } else if (password.text.Trim().Length < 8) {
            error.text = "Password must be 8+ characters!";
            return false;
        } else if (username.text.Trim().StartsWith("NOBODY")) {
            error.text = "Username cannot start with NOBODY; it is reserved.";
            return false;
        }
        return true;
    }

    public void CreateUser () {
        if (!ValidateLogin()) {
            return;
        }
        KiiUser.Builder builder = KiiUser.BuilderWithName(username.text.Trim());
        KiiUser user = builder.Build();
        user.Register(password.text.Trim(), UserCreatedCallback);
    }

    void UserCreatedCallback (KiiUser callbackUser, System.Exception exc) {
        Debug.LogWarning(exc.Message);
        if (exc != null && exc as System.NullReferenceException == null) {
            // Error handling
            setErrText = "CREATE USER FAILURE: " + exc.Message;
            return;
        }
        setErrText = "CREATE USER SUCCESS " + callbackUser.Username;
        User = callbackUser;

        KiiObject origObj = callbackUser.Bucket("worlds").NewKiiObject();
        origObj["test"] = "true";
        origObj.Save(true, BucketCreatedCallback);
    }

    void BucketCreatedCallback (KiiObject obj, System.Exception exc) {
        if (exc != null && exc as System.NullReferenceException == null) {
            // Error handling
            setErrText = "BUCKET FAILURE: " + exc.Message;
            return;
        }
        setErrText = "BUCKET SUCCESS " + obj.ToString();
        loggedIn = true;
    }

    public void LogIn () {
        if (!ValidateLogin()) {
            return;
        }
        KiiUser.LogIn(username.text.Trim(), password.text.Trim(), LoginCallback);
    }

    void LoginCallback (KiiUser callbackUser, System.Exception exc) {
        if (exc != null && exc as System.NullReferenceException == null) {
            // Error handling
            setErrText = "LOGIN FAILURE: " + exc.Message;
            return;
        }
        setErrText = "LOGIN SUCCESS " + callbackUser.Username;

        User = callbackUser;
        loggedIn = true;
    }

}
