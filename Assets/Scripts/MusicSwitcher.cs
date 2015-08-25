using UnityEngine;
using System.Collections;

public class MusicSwitcher : MonoBehaviour {
    public AudioClip calmSrc;
    public AudioClip hecticSrc;

    AudioSource music;
    AlienMgr aliens;

    void Awake () {
        music = GetComponent<AudioSource>();
        aliens = FindObjectOfType<AlienMgr>();
    }
	
	// Update is called once per frame
	void Update () {
	    if (aliens.AliensPresent && music.clip == calmSrc) {
            music.clip = hecticSrc;
            music.Play();
        } else if (!aliens.AliensPresent && music.clip == hecticSrc) {
            music.clip = calmSrc;
            music.Play();
        }
	}
}
