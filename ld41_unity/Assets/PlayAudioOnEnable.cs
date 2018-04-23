using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioOnEnable : MonoBehaviour {

    void OnEnable() {
        GetComponent<AudioSource>().Play();
    }
}
