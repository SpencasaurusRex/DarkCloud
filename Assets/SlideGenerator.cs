using UnityEngine;

public class SlideGenerator : MonoBehaviour
{
    //public float OuterRadius = 10;
    //public float InnerRadius = 20;
    public AnimationCurve OuterRadius;
    public AnimationCurve InnerRadius;
    public AnimationCurve DeltaHeight;

    public float AngleInterval = 10;
    public float Angle = 360;
    public Material Material;

    void Start()
    {
        float angleIntervalRadians = Mathf.Deg2Rad * AngleInterval;
        float angleRadians = Mathf.Deg2Rad * Angle;

        float height = 0;
        
        for (float theta = 0; theta < angleRadians; height += DeltaHeight.Evaluate(theta), theta += angleIntervalRadians)
        {
            GenerateStair(theta, theta + angleIntervalRadians, height, height + DeltaHeight.Evaluate(theta));
        }
    }

    GameObject GenerateStair(float theta, float theta2, float height, float height2)
    {
        GameObject g = new GameObject();
        g.tag = "Slide";

        Mesh m = new Mesh();
        var vertices = new Vector3[]
        {
            new Vector3(Mathf.Cos(theta) * InnerRadius.Evaluate(theta), height, Mathf.Sin(theta) * InnerRadius.Evaluate(theta)),
            new Vector3(Mathf.Cos(theta2) * InnerRadius.Evaluate(theta2), height2, Mathf.Sin(theta2) * InnerRadius.Evaluate(theta2)),
            new Vector3(Mathf.Cos(theta2) * OuterRadius.Evaluate(theta2), height2, Mathf.Sin(theta2) * OuterRadius.Evaluate(theta2)),
            new Vector3(Mathf.Cos(theta) * OuterRadius.Evaluate(theta), height, Mathf.Sin(theta) * OuterRadius.Evaluate(theta)),
            new Vector3(Mathf.Cos(theta) * InnerRadius.Evaluate(theta), height - .1f, Mathf.Sin(theta) * InnerRadius.Evaluate(theta)),
            new Vector3(Mathf.Cos(theta2) * InnerRadius.Evaluate(theta2), height2 - .1f, Mathf.Sin(theta2) * InnerRadius.Evaluate(theta2)),
            new Vector3(Mathf.Cos(theta2) * OuterRadius.Evaluate(theta2), height2 - .1f, Mathf.Sin(theta2) * OuterRadius.Evaluate(theta2)),
            new Vector3(Mathf.Cos(theta) * OuterRadius.Evaluate(theta), height - .1f, Mathf.Sin(theta) * OuterRadius.Evaluate(theta))
        };
        m.vertices = vertices;
        m.triangles = new [] { 0, 1, 2, 0, 2, 3, 4, 6, 5, 4, 7, 6};
        m.RecalculateNormals();
        m.RecalculateTangents();

        var filter = g.AddComponent<MeshFilter>();
        filter.mesh = m;

        var renderer = g.AddComponent<MeshRenderer>();
        renderer.material = Material;

        g.AddComponent<MeshCollider>();

        g.transform.SetParent(transform, false);

        return g;
    }
}
