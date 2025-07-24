using System.Collections.Generic;
using UnityEngine;

public class IslandGeneration : MonoBehaviour
{
    private Vector3 center;
    [SerializeField] private Vector2 minMaxRadius;
    [SerializeField] private Vector2 minMaxRotation;

    List<Vector3> _vertices; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        center = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AssignPolygonCollider()
    {

    }

    public Vector3[] PopulateVertices(Vector3 position, int resolution)
    {
        Vector3[] verts = new Vector3[(resolution+1) * (resolution+1)];
        Vector2[] uvs = new Vector2[verts.Length];

        for(int x = 0; x < resolution; x++)
        {
            for (int z = 0; z < resolution; z++)
            {
                Vector2 vertexPos = new Vector2(position.x + (x * (128 * resolution)), position.y + (z * (128 * resolution)));

                float distance = Vector2.Distance(vertexPos, position);
                float y = Mathf.Sin(Mathf.Clamp((1 + distance) / minMaxRadius.y, 0f, 1f) + 90f) * Mathf.PerlinNoise(position.x * 0.02f, position.y * 0.02f);
                verts[x * z + z] = new Vector3(x * (128f /  resolution), y, z * (128f * resolution));
            }
        }


        return verts;
    }

    public bool IsInBounds(Vector3 position, int count)
    {
        RaycastHit hit;
        if(Physics.Raycast(new Ray(position, Vector3.forward), out hit, Mathf.Infinity)) {
            count++;
            return IsInBounds(hit.point, count);
        }

        return count % 2 == 0;
    }
}
