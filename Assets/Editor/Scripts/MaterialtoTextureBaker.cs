using UnityEngine;
using UnityEditor;

public class MaterialToTextureBaker : EditorWindow
{
    private Material material;
    private int resolution = 512;

    [MenuItem("Tools/Material To Texture Baker")]
    public static void ShowWindow()
    {
        GetWindow<MaterialToTextureBaker>("Material To Texture");
    }

    void OnGUI()
    {
        GUILayout.Label("Bake Material to Texture2D", EditorStyles.boldLabel);

        material = (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), false);
        resolution = EditorGUILayout.IntField("Resolution", resolution);

        if (GUILayout.Button("Bake to Texture2D"))
        {
            if (material == null)
            {
                Debug.LogError("Please assign a material first.");
                return;
            }

            BakeMaterialToTexture(material, resolution);
        }
    }

    void BakeMaterialToTexture(Material mat, int res)
{
    // Create temporary camera
    var camGO = new GameObject("TempCam");
    var cam = camGO.AddComponent<Camera>();
    cam.orthographic = true;
    cam.orthographicSize = 0.5f;
    cam.clearFlags = CameraClearFlags.Color;
    cam.backgroundColor = Color.clear;
    cam.transform.position = new Vector3(0, 0, -10);

    // Setup render texture
    RenderTexture rt = new RenderTexture(res, res, 24);
    cam.targetTexture = rt;

    // Create quad
    var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
    quad.GetComponent<MeshRenderer>().sharedMaterial = mat;
    quad.transform.position = Vector3.zero;

    // Render
    cam.Render();

    // Read pixels
    RenderTexture.active = rt;
    Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
    tex.ReadPixels(new Rect(0, 0, res, res), 0, 0);
    tex.Apply();

    // Save
    byte[] bytes = tex.EncodeToPNG();
    string path = EditorUtility.SaveFilePanel("Save Texture", "Assets", mat.name + "_Baked.png", "png");
    if (!string.IsNullOrEmpty(path))
    {
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log("Saved baked texture to: " + path);
    }

    // --- Correct cleanup order ---
    RenderTexture.active = null;
    cam.targetTexture = null;

    DestroyImmediate(rt);
    DestroyImmediate(quad);
    DestroyImmediate(camGO);
}

}
