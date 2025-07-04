using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;

public class TerrainController : MonoBehaviour
{
    [SerializeField] private SpriteShapeController shape;

    [FormerlySerializedAs("scale")] public int terrainHeightScale = 10;

    public int numOfPoints = 150;
    public int distanceBetweenPoints = 3;
    
    // How frequent is the hills
    public float noiseScale = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shape = GetComponent<SpriteShapeController>();
        // Adding after from the second point (top right corner)
        for (int i = 3; i < numOfPoints + 3; i++)
        {
            // Get the position of 1 + distanceBetweenPoints (Top left corner)
            var posX = shape.spline.GetPosition(i - 1).x + distanceBetweenPoints;
            // Insert point from the second position (Top right corner)
            float y = Mathf.PerlinNoise(posX * noiseScale, Random.Range(0f, 1000f)) * terrainHeightScale;
            var newPos = new Vector3(posX, y, 0);
            shape.spline.InsertPointAt(i, newPos);
            Debug.Log($"Adding point {i} at position {newPos}");
        }
        Debug.Log($"{shape.spline.GetPointCount() - 1} Final point position: {shape.spline.GetPosition(shape.spline.GetPointCount() - 1)}");
        Debug.Log($"Before final point position: {shape.spline.GetPosition(shape.spline.GetPointCount() - 2)}");
        // Make the final point has the same posX as the point before it
        var beforeFinalPoint = shape.spline.GetPosition(shape.spline.GetPointCount() - 2);
        // Set the final point to have the same x position as the before final point
        // And has the y of the beginning point
        var beginningPoint = shape.spline.GetPosition(0);
        shape.spline.SetPosition(shape.spline.GetPointCount() - 1, new Vector3(beforeFinalPoint.x, beginningPoint.y, 0));
        
        // Smoothout the terrarin by modifying tangnents
        for (int i = 3; i < numOfPoints + 3; i++)
        {
            shape.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
            shape.spline.SetLeftTangent(i, new Vector3(-5, 0, 0));
            shape.spline.SetRightTangent(i, new Vector3(5, 0, 0));
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}