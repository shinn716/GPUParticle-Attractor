
using UnityEngine;

struct DBuffer
{
    public Vector3 vertexPos;//作用的mesh顶点
}

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformerByCS : MonoBehaviour
{
    public ComputeShader shader;
    private ComputeBuffer buffer;
    int bufferArrayLength;

    private Mesh deformingMesh;
    Vector3[] originalVertices, displacedVertices;
    Vector3[] vertexVelocities;
    new MeshCollider collider;

    public float AlphaOfVertexes = 0.5f;
    Vector3 lastPoint = Vector3.zero;
    float uniformScale = 1f;

    #region public
    public void AddInDeformingForce(Vector3 point, float force)
    {
        point = transform.InverseTransformPoint(point);
        if (lastPoint == Vector3.zero)
            lastPoint = point;

        else if (lastPoint != Vector3.zero && lastPoint != point)
        {
            lastPoint = point;
            ClearVertexVelocities();
        }

        Dispatch(force, point);
        //根据Shader返回的buffer数据更新物体信息
        DBuffer[] values = new DBuffer[bufferArrayLength];
        buffer.GetData(values);
        for (int i = 0; i < bufferArrayLength; i++)
        {
            displacedVertices[i] = values[i].vertexPos;
        }

        deformingMesh.vertices = displacedVertices;
        deformingMesh.RecalculateNormals();
        collider.sharedMesh = deformingMesh;
    }


    public void ReCover()
    {
        vertexVelocities = new Vector3[originalVertices.Length];
        deformingMesh.vertices = originalVertices;
        deformingMesh.RecalculateNormals();
    }

    public void ClearVertexVelocities()
    {
        vertexVelocities = new Vector3[originalVertices.Length];
    }
    #endregion

    #region unity
    void Start()
    {
        deformingMesh = transform.GetComponent<MeshFilter>().mesh;
        bufferArrayLength = deformingMesh.vertexCount;
        originalVertices = deformingMesh.vertices;
        displacedVertices = new Vector3[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            displacedVertices[i] = originalVertices[i];
        }
        vertexVelocities = new Vector3[originalVertices.Length];

        collider = GetComponent<MeshCollider>();

        CreateBuffer();
    }

    #endregion

    void CreateBuffer()
    {
        //count数组的长度（等于2个三维的积 2x2x1 * 2x2x1），40是结构体的字节长度
        buffer = new ComputeBuffer(bufferArrayLength, 12);
        DBuffer[] values = new DBuffer[bufferArrayLength];
        for (int i = 0; i < bufferArrayLength; i++)
        {
            DBuffer m = new DBuffer();
            SetStruct(ref m, displacedVertices[i]);
            values[i] = m;
        }
        // 初始化结构体并赋予buffer
        buffer.SetData(values);
    }

    void SetStruct(ref DBuffer m, Vector3 vertexPos)
    {
        m.vertexPos = vertexPos;

    }


    void Dispatch(float force, Vector3 pressPos)
    {
        //必须分配足够多的线程
        //int groupx = (int)Mathf.Pow(bufferArrayLength / 256, 1 / 3);
        shader.SetFloat("force", force);
        shader.SetVector("pressPos", pressPos);
        shader.SetInt("groupx", 8);
        shader.SetInt("groupy", 8);

        int kid = shader.FindKernel("CSMain");
        shader.SetBuffer(kid, "dBuffer", buffer);

        shader.Dispatch(kid, 8, 8, 8);
    }

    #region private
    #endregion

    void ReleaseBuffer()
    {
        buffer.Release();
    }
    private void OnDisable()
    {
        ReleaseBuffer();
    }
}