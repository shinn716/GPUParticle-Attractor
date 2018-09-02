using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GPUParticleSample))]
public class GPUParticleSampleEditor : Editor {
    
    GPUParticleSample _script;
    
    bool AttractorGroup = true;
    bool NoiseGroup = true;

    private SerializedObject m_Object;
    private SerializedProperty m_Property;

    public override void OnInspectorGUI()
    {
        _script = (GPUParticleSample)target;

        EditorGUILayout.Space();
        _script.cs = (ComputeShader)EditorGUILayout.ObjectField("Compute shader", _script.cs, typeof(ComputeShader), true);
        _script.particleMax = EditorGUILayout.IntField("Particle maxium", _script.particleMax);
        _script.emitMax = EditorGUILayout.IntField("Emission rate", _script.emitMax);

        EditorGUILayout.Space();
        _script.velocityRange = EditorGUILayout.Vector2Field("Velocity", _script.velocityRange);
        _script.lifeTime = EditorGUILayout.FloatField("Life time", _script.lifeTime);
        _script.scaleRange = EditorGUILayout.Vector2Field("Scale range", _script.scaleRange);
        _script.gravity = EditorGUILayout.FloatField("Gravity", _script.gravity);


        EditorGUILayout.Space();
        _script.colortype = (GPUParticleSample.ColorType)EditorGUILayout.EnumPopup("Color type", _script.colortype);
        ColorType();

        EditorGUILayout.Space();
        _script.mytype = (GPUParticleSample.Type)EditorGUILayout.EnumPopup("Emission type", _script.mytype);
        EmissionType();

        EditorGUILayout.Space();
        AttractorGroup = EditorGUILayout.Foldout(AttractorGroup, "Attractor group");
        if (AttractorGroup)
        {
            _script.enableAttactor = EditorGUILayout.Toggle("Enable attractor", _script.enableAttactor);

            if (_script.enableAttactor)
            {
                _script.attractionValue = EditorGUILayout.Slider("Attractor value", _script.attractionValue, 0, 1);
                //_script._range = EditorGUILayout.Slider("Range", _script._range, 0, 100);
                _script.attractorGravity = EditorGUILayout.Slider("Attractor gravity", _script.attractorGravity, 0, 1);


                EditorGUILayout.Space();
                _script.mAttractor = EditorGUILayout.Toggle("Enable attractor", _script.mAttractor);

                if (_script.mAttractor)
                {

                    EditorGUIUtility.LookLikeInspector();
                    SerializedProperty tps = serializedObject.FindProperty("mtarget");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(tps, true);

                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();

                    EditorGUIUtility.LookLikeControls();

                }

            }
        }


        EditorGUILayout.Space();
        NoiseGroup = EditorGUILayout.Foldout(NoiseGroup, "Noise group");
        if (NoiseGroup)
        {
            _script.enableNoise = EditorGUILayout.Toggle("Enable noise", _script.enableNoise);

            if (_script.enableNoise)
            {
                _script.convergence = EditorGUILayout.Slider("Initial value", _script.convergence, 0, 10);
                //_script.convergenceFrequency = EditorGUILayout.FloatField("Convergence frequency", _script.convergenceFrequency);
                _script.convergenceAmplitude = EditorGUILayout.Slider("Amplitude", _script.convergenceAmplitude, 0, 10);
                _script.viscosity = EditorGUILayout.Slider("Scale offset", _script.viscosity, 0, 1);
            }

        }


        EditorGUILayout.Space();
        _script.stopkey = (KeyCode)EditorGUILayout.EnumPopup("Stop key", _script.stopkey);
    }


    void ColorType() {
        switch (_script.colortype) {

            case GPUParticleSample.ColorType.HSV:
                _script.hue = EditorGUILayout.Slider("Hue", _script.hue, 0, 1);
                _script.sai = EditorGUILayout.Slider("Saturation", _script.sai, 0, 1);
                _script.val = EditorGUILayout.Slider("Value", _script.val, 0, 1);
                break;

            case GPUParticleSample.ColorType.RGB:
                _script.myColor = EditorGUILayout.ColorField("Color", _script.myColor);
                break;


            default:
                break;
        }
    }


    void EmissionType()
    {
        switch (_script.mytype)
        {
            case GPUParticleSample.Type.Mouse:
                _script.posz = EditorGUILayout.FloatField("Mouse distance", _script.posz);
                break;

            case GPUParticleSample.Type.Point:
                //_script.target = (Transform)EditorGUILayout.ObjectField("Target", _script.target, typeof(Transform), true);
                break;

            default:
                break;

        }
    }
}
