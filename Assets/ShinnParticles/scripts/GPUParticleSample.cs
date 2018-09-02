using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public struct GPUParticleData
{
    public bool isActive;       // 有効フラグ
    public Vector3 position;    // 座標
    public Vector3 velocity;    // 加速度
    public Color color;         // 色
    public float duration;      // 生存時間
    public float scale;         // サイズ
}

[RequireComponent(typeof(GPUParticleRendererSample))]
public class GPUParticleSample : GPUParticleBase<GPUParticleData> {
    
    #region public
    public Vector2 velocityRange = new Vector2(0, 1);
    //public float velocityMax = 1000f;
    public float lifeTime = 1;
    public Vector2 scaleRange = new Vector2(.001f, .15f);
    public float gravity = 9.8f;
    #endregion

    #region Color
    public enum ColorType
    {
        RGB,
        HSV,
    }
    [SerializeField]
    public ColorType colortype;

    [SerializeField]
    public Color myColor;

    [Range(0, 1)]
    public float sai = 0;   // 彩度
    [Range(0, 1)]
    public float val = 0.5f;   // 明るさ
    [Range(0, 1)]
    public float hue = 0f;
    #endregion

    #region Emission
    [SerializeField, Header("Emission type")]
    public Type mytype;
    public KeyCode stopkey;

    Transform target;
    bool stopflag = false;
    Type temp;

    [SerializeField, Range(0, 20), Header("Particles distance")]
    public float posz = 5;

    public enum Type{
        Mouse,
        Point, 
        None
    }
    #endregion

    #region Attractor
    [SerializeField, Header("Attactor")]
    public bool enableAttactor = false;

    [SerializeField, Range(0, 1)]
    public float attractionValue = .1f;

    [SerializeField, Range(0, 1)]
    public float attractorGravity = .98f;

    [SerializeField]
    public bool mAttractor = false;
    [SerializeField]
    public Transform[] mtarget;


    Vector4[] mtemp;

    bool attactorst = false;
    float attraction;
    Vector3 mousePos;


    Vector3 position;

    //[SerializeField, Range(0, 100)]
    //public float _range = 50;
    #endregion

    #region Noise
    public bool enableNoise = true;
    public float convergence = 10;
    public float convergenceFrequency = 10f;
    public float convergenceAmplitude = 10;
    public float viscosity = 0.1f;
    #endregion

    private void Start()
    {
        temp = mytype;
        mtemp = new Vector4[mtarget.Length];
    }

    /// <summary>
    /// パーティクルの更新
    /// </summary>
    /// 
    protected override void UpdateParticle()
    {
        particleActiveBuffer.SetCounterValue(0);
        
        cs.SetFloat("_DT", Time.deltaTime);
        cs.SetFloat("_LifeTime", lifeTime);     //lifeTime
        cs.SetFloat("_Gravity", gravity);
        cs.SetBuffer(updateKernel, "_Particles", particleBuffer);
        cs.SetBuffer(updateKernel, "_DeadList", particlePoolBuffer);
        cs.SetBuffer(updateKernel, "_ActiveList", particleActiveBuffer);

        cs.Dispatch(updateKernel, particleNum / THREAD_NUM_X, 1, 1);

        particleCounts[0] = 1;
        particleCounts[1] = 1;
        particleActiveCountBuffer.SetData(particleCounts);
        ComputeBuffer.CopyCount(particleActiveBuffer, particleActiveCountBuffer, 4);
    }

    /// <summary>
    /// パーティクルの発生
    /// THREAD_NUM_X分発生
    /// </summary>
    /// <param name="position"></param>
    /// 
    void EmitParticle()
    {
        particlePoolCountBuffer.SetData(particleCounts);
        ComputeBuffer.CopyCount(particlePoolBuffer, particlePoolCountBuffer, 4);
        particlePoolCountBuffer.GetData(particleCounts);
        particlePoolNum = particleCounts[1];

        if (particlePoolNum < emitNum) return;   // emitNum未満なら発生させない
        
        cs.SetVector("_EmitPosition", position);
        cs.SetFloat("_VelocityMax", velocityRange.x);
        cs.SetFloat("_VelocityMax", velocityRange.y);

        cs.SetFloat("_LifeTime", lifeTime);
        cs.SetFloat("_ScaleMin", scaleRange.x);
        cs.SetFloat("_ScaleMax", scaleRange.y);
        cs.SetFloat("_Hue", hue);
        cs.SetFloat("_Sai", sai);
        cs.SetFloat("_Val", val);
        cs.SetFloat("_Timetime", Time.time);

        //Attacror
        cs.SetBool("_Attractorst", attactorst);


        cs.SetBool("_mAttractorst", mAttractor);

        if (mAttractor)
        {
            attraction = attractionValue;
            attactorst = true;


            for (int i = 0; i < mtemp.Length; i++)
                mtemp[i] = mtarget[i].position;

            cs.SetInt("_mcount", mtemp.Length);
            cs.SetVectorArray("_mPos", mtemp);
        }
        else
            cs.SetVector("_MousePos", mousePos);


        cs.SetFloat("_Attraction", attraction);
        cs.SetFloat("_AttractorGravity", attractorGravity);
        //cs.SetFloat("_Range", _range);

        cs.SetBool("_noise", enableNoise);
        cs.SetFloat("viscosity", viscosity);
        cs.SetFloat("convergence", convergence + Mathf.PerlinNoise(Time.time * convergenceFrequency, Mathf.PingPong(Time.time * convergenceFrequency, 1.0f)) * convergenceAmplitude);
        
        
        cs.SetBuffer(emitKernel, "_ParticlePool", particlePoolBuffer);
        cs.SetBuffer(emitKernel, "_Particles", particleBuffer);

        //cs.Dispatch(emitKernel, particleCounts[0] / THREAD_NUM_X, 1, 1);
        cs.Dispatch(emitKernel, emitNum / THREAD_NUM_X, 1, 1);   // emitNumの数だけ発生
    }


    // Update is called once per frame
    protected override void Update () {
        
        typeSelect();
        colorSelect();

        if (enableAttactor)
        {
            mousePos = Input.mousePosition;
            mousePos.z = posz;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);

            if (!mAttractor) {

                if (Input.GetMouseButton(0))
                    attactorst = true;
                else
                    attactorst = false;

                if (attactorst)
                    attraction = Input.GetMouseButton(0) ? attractionValue : 0f;

            }
        }

        if (Input.GetKeyDown(stopkey)) {
            stopflag = !stopflag;

            if (stopflag)
                mytype = Type.None;
            else
                mytype = temp;
        }
    }


	public void ConvertToHSV(Color inColour)
    {
		float h, s, v;
		Color.RGBToHSV (inColour, out h, out s, out v);
		sai = s;
		val = v;
		hue = h;
    }


    void typeSelect() {
        switch (mytype) {
            default:
                UpdateParticle();
                break;

            case Type.Mouse:

                position = mousePos;

                EmitParticle();
                UpdateParticle();
                break;

            case Type.Point:

                target = transform;
                position = target.position;

                EmitParticle();
                UpdateParticle();
                break;
        }
    }


    void colorSelect()
    {
        switch (colortype)
        {
            default:
                break;

            case ColorType.HSV:
                break;

            case ColorType.RGB:
                ConvertToHSV(myColor);
                break;


        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1);
    }

}
