using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorCon : MonoBehaviour {


    public GPUParticleSample particles;
    public bool enable = false;


	void Update () {

        if(enable)
            particles.hue = Mathf.PingPong(Time.time, 1);
    }

}
