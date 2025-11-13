using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class BaseGridGenerator : EditorWindow
{
    private int width = 5;
    private int height = 5;
    private float spacing = 0.3f;
    private float dotSize = 0.1f;
    private bool[,] gridToggles;
    private bool showLayout = true;
    private GameObject dotRef;

    [MenuItem("Tools/Base Level Grid Tool")]
    public static void ShowWindow()
    {
        GetWindow<BaseGridGenerator>("Base Level Grid");
    }

    private void OnEnable()
    {
        InitGrid();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void InitGrid()
    {
        gridToggles = new bool[height, width];
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        spacing = EditorGUILayout.FloatField("Spacing", spacing);
        dotSize = EditorGUILayout.FloatField("DotSize", dotSize);

        if (EditorGUI.EndChangeCheck())
            InitGrid();

        EditorGUILayout.Space();

        // Grid of checkboxes (top-down visually)
        for (int y = height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < width; x++)
            {
                gridToggles[y, x] = GUILayout.Toggle(gridToggles[y, x], "", GUILayout.Width(20));
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(10);
        showLayout = GUILayout.Toggle(showLayout, "Show Layout Preview");

        if (GUILayout.Button("Generate Dots"))
            GenerateDots();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Checked cells will be previewed as circles in Scene view.", MessageType.Info);
        dotRef = (GameObject)EditorGUILayout.ObjectField("Dot Prefab", dotRef, typeof(GameObject), false);
    }

    private void GenerateDots()
    {
        GameObject parent = new GameObject("DotGrid");
        DotGrid dg = parent.AddComponent<DotGrid>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!gridToggles[y, x]) continue;

                Vector3 pos = new Vector3(x * spacing, y * spacing, 0f);
                GameObject dot = Instantiate(dotRef);
                dot.name = $"Dot_{x}_{y}";
                dot.transform.position = pos;
                dot.transform.localScale = Vector3.one * dotSize;
                dot.transform.SetParent(parent.transform);
                Dot dotComp = dot.AddComponent<Dot>();
                Debug.Log($"{x} {y}");
                dotComp.SetupDot(x, y);
                dg.AddElement(x, y, dotComp);
            }
        }

        Debug.Log("Generated 2D grid dots!");
        dg.DebugEachElement();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!showLayout) return;

        Handles.color = new Color(0, 1, 1, 0.8f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!gridToggles[y, x]) continue;

                Vector3 pos = new Vector3(x * spacing, y * spacing, 0f);

                // Draw wire circle for checked cell
                Handles.DrawWireDisc(pos, Vector3.forward, 0.1f);

                // Optional: draw faint cross inside for clarity
                Handles.DrawLine(pos + Vector3.left * 0.1f, pos + Vector3.right * 0.1f);
                Handles.DrawLine(pos + Vector3.up * 0.1f, pos + Vector3.down * 0.1f);
            }
        }

        SceneView.RepaintAll();
    }
}
