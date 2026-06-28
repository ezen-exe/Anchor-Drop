using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TMPro;

[ExecuteInEditMode]
public class MapGenerator : MonoBehaviour
{
    [Header("Map Data")]
    public TextAsset geoJsonFile;
    public Material roadMaterial;
    public Material buildingMaterial;

    [Header("Map Settings")]
    public float centerLatitude = 26.5123f;
    public float centerLongitude = 80.2329f;
    
    [Header("Labels")]
    public Color textColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public float fontSize = 15f;
    public TMP_FontAsset customFont;

    private Vector3 originOffset;

    // This runs when you change values in the Inspector
    void OnValidate()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            Generate();
        }
    }

    // This runs when the game starts
    void Start()
    {
        Generate();
    }

    void Generate()
    {
        // 1. Clean up old map
        Transform existingMap = transform.Find("Campus_Map");
        if (existingMap != null)
        {
            if (Application.isPlaying) Destroy(existingMap.gameObject);
            else DestroyImmediate(existingMap.gameObject);
        }

        // 2. Generate new map
        if (geoJsonFile == null) return;
        originOffset = LatLonToMeters(centerLatitude, centerLongitude);
        GenerateMap(geoJsonFile.text);
    }

    void GenerateMap(string jsonText)
    {
        JObject mapData = JObject.Parse(jsonText);
        JArray features = (JArray)mapData["features"];

        GameObject mapParent = new GameObject("Campus_Map");
        mapParent.transform.SetParent(this.transform); // Keeps it organized

        foreach (JObject feature in features)
        {
            JObject properties = (JObject)feature["properties"];
            JObject geometry = (JObject)feature["geometry"];
            if (geometry == null) continue;

            string geoType = geometry["type"].ToString();
            JArray coordinates = (JArray)geometry["coordinates"];
            string featureName = properties["name"] != null ? properties["name"].ToString() : "Unnamed Feature";

            if (geoType == "LineString" && properties["highway"] != null)
            {
                DrawPath(coordinates, featureName, mapParent.transform, roadMaterial, 6f); 
            }
            else if (geoType == "Polygon" && properties["building"] != null)
            {
                DrawSolidPolygon((JArray)coordinates[0], featureName, mapParent.transform, buildingMaterial);
            }
        }
    }

    void DrawPath(JArray coordinates, string objName, Transform parent, Material mat, float width)
    {
        GameObject lineObj = new GameObject(objName);
        lineObj.transform.SetParent(parent);

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.material = mat;
        line.startWidth = width;
        line.endWidth = width;
        line.useWorldSpace = false;
        line.alignment = LineAlignment.View; 

        List<Vector3> points = new List<Vector3>();
        foreach (JArray coord in coordinates)
        {
            float lon = (float)coord[0];
            float lat = (float)coord[1];
            Vector3 worldPos = LatLonToMeters(lat, lon) - originOffset;
            points.Add(worldPos);
        }

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }

    void DrawSolidPolygon(JArray coordinates, string objName, Transform parent, Material mat)
    {
        GameObject polyObj = new GameObject(objName);
        polyObj.transform.SetParent(parent);

        List<Vector2> points2D = new List<Vector2>();
        foreach (JArray coord in coordinates)
        {
            float lon = (float)coord[0];
            float lat = (float)coord[1];
            Vector3 worldPos = LatLonToMeters(lat, lon) - originOffset;
            points2D.Add(new Vector2(worldPos.x, worldPos.z));
        }

        PolygonCollider2D polyCollider = polyObj.AddComponent<PolygonCollider2D>();
        polyCollider.SetPath(0, points2D.ToArray());

        Mesh mesh = polyCollider.CreateMesh(false, false);
        Vector3[] vertices3D = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            vertices3D[i] = new Vector3(mesh.vertices[i].x, 0.1f, mesh.vertices[i].y);
        }
        mesh.vertices = vertices3D;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        polyObj.AddComponent<MeshFilter>().mesh = mesh;
        polyObj.AddComponent<MeshRenderer>().material = mat;
        
        if (objName != "Unnamed Feature") CreateLabel(objName, mesh.bounds.center, parent);
        DestroyImmediate(polyCollider); // Clean up physics used for mesh gen
    }
    
    void CreateLabel(string text, Vector3 position, Transform parent)
    {
        GameObject textObj = new GameObject(text + "_Label");
        textObj.transform.SetParent(parent);
        textObj.transform.localPosition = new Vector3(position.x, 0.2f, position.z);
        textObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        TextMeshPro tmpro = textObj.AddComponent<TextMeshPro>();
        tmpro.text = text;
        tmpro.fontSize = fontSize;
        tmpro.color = textColor;
        tmpro.alignment = TextAlignmentOptions.Center;
        tmpro.enableWordWrapping = false;
        tmpro.overflowMode = TextOverflowModes.Overflow;
        tmpro.rectTransform.sizeDelta = new Vector2(100, 10);
        
        if (customFont != null) tmpro.font = customFont;
        tmpro.fontStyle = FontStyles.Bold;
    }

    Vector3 LatLonToMeters(float lat, float lon)
    {
        float x = lon * 20037508.34f / 180f;
        float y = Mathf.Log(Mathf.Tan((90f + lat) * Mathf.PI / 360f)) / (Mathf.PI / 180f);
        y = y * 20037508.34f / 180f;
        return new Vector3(x, 0, y);
    }
}