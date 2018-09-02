using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUParticleRendererSample : GPUParticleRendererBase<GPUParticleData> {

    public bool debug = false;
    public Vector2 pos = new Vector2(10, 72);
    public GUIStyle style;

    protected override void SetMaterialParam()
    {
        material.SetBuffer("_Particles", particleBuffer);
        material.SetBuffer("_ParticleActiveList", activeIndexBuffer);
        material.SetFloat("_Scale", 1f);

        material.SetPass(0);
    }

    private void OnGUI()
    {
        if (debug) 
            GUI.Label(new Rect(pos.x, pos.y, 480, 64), "Active / Pool " + particle.GetActiveParticleNum() + " / " + particle.GetPoolParticleNum(), style);
    }
}
