using UnityEngine;

[ExecuteInEditMode]
public class ShowCenterOfMass : MonoBehaviour
{
    public Rigidbody rb;
    public float radius = 0.1f;
    public Color color = Color.red;

    void OnDrawGizmos()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
                return;
        }

        Gizmos.color = color;
        // 将质心从局部坐标转换到世界坐标
        Vector3 worldCenterOfMass = rb.transform.TransformPoint(rb.centerOfMass);
        Gizmos.DrawSphere(worldCenterOfMass, radius);
    }
}