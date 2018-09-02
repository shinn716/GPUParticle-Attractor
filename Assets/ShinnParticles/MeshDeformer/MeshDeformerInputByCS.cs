
using UnityEngine;

public class MeshDeformerInputByCS : MonoBehaviour
{

    float force = 1f;
    float forceOffset = 0.1f;//用于产生力的角度

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            HandleInInput();
        }

        if (Input.GetMouseButtonUp(0))
        {
            HandleEndPression();
        }
    }

    void HandleInInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(inputRay, out hit))
        {
            MeshDeformerByCS deformer = hit.collider.GetComponent<MeshDeformerByCS>();
            if (deformer)
            {
                Vector3 point = hit.point;//World space
                point += hit.normal * forceOffset;//world space 用于计算受力的方向
                deformer.AddInDeformingForce(point, force);

            }
        }

    }

    void HandleEndPression()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(inputRay, out hit))
        {
            MeshDeformerByCS deformer = hit.collider.GetComponent<MeshDeformerByCS>();
            if (deformer)
            {
                deformer.ClearVertexVelocities();
            }
        }
    }
}