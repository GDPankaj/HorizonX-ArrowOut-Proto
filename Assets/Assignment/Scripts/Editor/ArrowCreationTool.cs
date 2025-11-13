using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

[EditorTool("Arrow Brush")]
public class ArrowCreationTool : EditorTool
{
    private Dot startDot;
    private List<Dot> currentDots = new List<Dot>();
    private bool isDrawing = false;
    private DotGrid dotGrid;

    public override void OnToolGUI(EditorWindow window)
    {
        Event e = Event.current;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive)); // disables default scene selection

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Dot clickedDot = GetDotUnderMouse(e.mousePosition);
            if (clickedDot == null) return;

            startDot = clickedDot;
            currentDots.Clear();
            currentDots.Add(startDot);
            isDrawing = true;
            e.Use();
        }

        if (e.type == EventType.MouseDrag && isDrawing)
        {
            Dot hoveredDot = GetDotUnderMouse(e.mousePosition);
            if (hoveredDot != null && hoveredDot != currentDots[^1])
            {
                Dot last = currentDots[^1];

                if (IsValidNextDot(last, hoveredDot))
                {
                    // Only add if it's aligned and not already in the list
                    if (!currentDots.Contains(hoveredDot))
                        currentDots.Add(hoveredDot);

                    SceneView.RepaintAll();
                }
            }
        }

        if (e.type == EventType.MouseUp && isDrawing)
        {
            if (currentDots.Count > 1)
            {
                CreateArrow(currentDots);
            }

            isDrawing = false;
            currentDots.Clear();
            startDot = null;
            e.Use();
        }

        // Draw live preview line
        if (isDrawing && currentDots.Count > 1)
        {
            Handles.color = Color.cyan;
            for (int i = 1; i < currentDots.Count; i++)
            {
                Handles.DrawLine(currentDots[i - 1].transform.position, currentDots[i].transform.position);
            }
        }
    }

    private Dot GetDotUnderMouse(Vector2 mousePos)
    {
        // Cast a ray into the scene from the scene view camera
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

        // Use HandleUtility to find the nearest GameObject under the cursor
        GameObject go = HandleUtility.PickGameObject(mousePos, false);
        if (go != null)
            return go.GetComponent<Dot>();

        return null;
    }

    private bool IsValidNextDot(Dot a, Dot b)
    {
        // must be straight line continuation (same row or same column)
        bool sameX = a.GetX() == b.GetX();
        bool sameY = a.GetY() == b.GetY();
        bool diagonal = !sameX && !sameY;

        if (diagonal) return false;

        // Optional: prevent backtracking
        if (currentDots.Count >= 2 && b == currentDots[^2])
            return false;

        return true;
    }

    private void CreateArrow(List<Dot> dots)
    {
        GameObject arrowObj = new GameObject($"Arrow_{dots[0].GetX()}_{dots[0].GetY()}_to_{dots[^1].GetX()}_{dots[^1].GetY()}");
        arrowObj.transform.position = dots[0].transform.position;
        arrowObj.transform.localScale = Vector3.one;
        arrowObj.AddComponent<PolygonCollider2D>();
        SpriteRenderer sr = arrowObj.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("Triangle");
        sr.color = Color.black;
        Undo.RegisterCreatedObjectUndo(arrowObj, "Create Arrow");
        foreach (Dot dot in dots)
        {
            Debug.Log(dot.gameObject.name);
        }
        Arrow arrow = arrowObj.AddComponent<Arrow>();
        arrow.AddComponent<LineRenderer>();
        arrow.SetupWithDots(dots);

        GameObject arrowGridLineChild = new GameObject("ArrowGridLineChild");
        arrowGridLineChild.AddComponent<LineRenderer>();
        arrowGridLineChild.transform.SetParent(arrowObj.transform);
    }
}
