using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField]private float arrowSpeed = 100f;
    [SerializeField]private List<Dot> pointsOfArrow;
    [SerializeField]private ArrowFacingDirection facingDirection;
    private LineRenderer lineRenderer;
    private SpriteRenderer spriteRenderer;
    private Transform gridLineChild;
    private LineRenderer childLineRenderer;
    private Level level;
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        gridLineChild = transform.GetChild(0);
        childLineRenderer = gridLineChild.GetComponent<LineRenderer>();

        SetupGridLines();
    }
    public void Init(Level l)
    {
        level = l;
        HidLines();
        GameManager.OnToggleGridLines += ToggleLinesVisibility;
    }
    #region grid_line_functionality
    public void ShowLines()
    {
        Debug.Log("Showing Lines");
        if(gridLineChild == null || childLineRenderer == null) 
        {
            gridLineChild = transform.GetChild(0);
            childLineRenderer = gridLineChild.GetComponent<LineRenderer>();
            SetupGridLines();
        }
        gridLineChild.gameObject.SetActive(true);
        
    }
    public void HidLines()
    {
        Debug.Log("HidingLines");
        if (gridLineChild == null || childLineRenderer == null)
        {
            gridLineChild = transform.GetChild(0);
            childLineRenderer = gridLineChild.GetComponent<LineRenderer>();
            SetupGridLines();
        }
        gridLineChild.gameObject.SetActive(false);
        
    }
    public void SetupGridLines()
    {
        childLineRenderer.positionCount = 2;
        switch (facingDirection)
        {
            case ArrowFacingDirection.Left:
                childLineRenderer.SetPosition(0, transform.position);
                Vector2 farleftPos =transform.position;
                farleftPos.x = -100f;
                childLineRenderer.SetPosition(1, farleftPos);
                break;
            case ArrowFacingDirection.Right:
                childLineRenderer.SetPosition(0, transform.position);
                Vector2 farRightPos = transform.position;
                farRightPos.x = 100f;
                childLineRenderer.SetPosition(1, farRightPos);
                break;
            case ArrowFacingDirection.Up:
                childLineRenderer.SetPosition(0, transform.position);
                Vector2 farTopPos = transform.position;
                farTopPos.y = 100f;
                childLineRenderer.SetPosition(1, farTopPos);
                break;
            case ArrowFacingDirection.Down:
                childLineRenderer.SetPosition(0, transform.position);
                Vector2 farBottomPos = transform.position;
                farBottomPos.y = -100f;
                childLineRenderer.SetPosition(1, farBottomPos);
                break;
        }
        childLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        childLineRenderer.startColor = Color.cyan;
        childLineRenderer.endColor = Color.cyan;
        childLineRenderer.startWidth = 0.1f;
    }
    private void ToggleLinesVisibility()
    {
        if (!gridLineChild.gameObject.activeInHierarchy)
        {
            ShowLines();
        }
        else
        {
            HidLines();
        }
    }
    #endregion
    private void Update()
    {
        Vector3 pos = lineRenderer.GetPosition(0);
        switch (facingDirection)
        {
            case ArrowFacingDirection.Left:
                pos.x -= 0.05f;
                break;
            case ArrowFacingDirection.Right:
                pos.x += 0.05f;
                break;
            case ArrowFacingDirection.Up:
                pos.y += 0.05f;
                break;
            case ArrowFacingDirection.Down:
                pos.y -= 0.05f;
                break;
        }
        transform.position = pos;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowLines();
        }
    }
    private void OnMouseDown()
    {
        if(pointsOfArrow[0].Grid.CanArrowEscapeAt(pointsOfArrow[0], facingDirection))
        {
            lineRenderer.startColor = Color.black;
            lineRenderer.endColor = Color.black;
            spriteRenderer.color = Color.black;
            foreach (Dot dot in pointsOfArrow)
            {
                dot.DeOccupy();
            }
            switch (facingDirection)
            {
                case ArrowFacingDirection.Left:
                    StartCoroutine(RemoveFromLeft());
                    break;
                case ArrowFacingDirection.Right:
                    StartCoroutine(RemoveFromRight());
                    break;
                case ArrowFacingDirection.Up:
                    StartCoroutine(RemoveFromTop());
                    break;
                case ArrowFacingDirection.Down:
                    StartCoroutine(RemoveFromBottom());
                    break;
            }
            level.RemoveArrow(this);
            HidLines();
            GameManager.OnToggleGridLines -= ToggleLinesVisibility;
        }
        else
        {
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            spriteRenderer.color = Color.red;
            switch (facingDirection)
            {
                case ArrowFacingDirection.Left:
                    StartCoroutine(RemoveFromLeftFail());
                    break;
                case ArrowFacingDirection.Right:
                    StartCoroutine(RemoveFromRightFail());
                    break;
                case ArrowFacingDirection.Up:
                    StartCoroutine(RemoveFromTopFail());
                    break;
                case ArrowFacingDirection.Down:
                    StartCoroutine(RemoveFromBottomFail());
                    break;
            }
        }
    }
    public void SetupWithDots(List<Dot> connectedDots)
    {
        if (connectedDots == null || connectedDots.Count < 2)
        {
            Debug.LogWarning("Arrow needs at least two dots.");
            return;
        }
        pointsOfArrow = new List<Dot>();
        foreach (var dot in connectedDots)
        {
            pointsOfArrow.Add(dot);
        }
        // Validate all connections
        for (int i = 1; i < pointsOfArrow.Count; i++)
        {
            int dx = pointsOfArrow[i].GetX() - pointsOfArrow[i - 1].GetX();
            int dy = pointsOfArrow[i].GetY() - pointsOfArrow[i - 1].GetY();

            if (dx != 0 && dy != 0)
            {
                Debug.LogError($"Invalid diagonal connection between {pointsOfArrow[i - 1].name} and {pointsOfArrow[i].name}");
                return;
            }
        }
        // Determine main direction (first segment only)
        Vector2Int dir = new Vector2Int(
            pointsOfArrow[0].GetX() - pointsOfArrow[1].GetX(),
            pointsOfArrow[0].GetY() - pointsOfArrow[1].GetY()
        );
        if (dir.x > 0) { facingDirection = ArrowFacingDirection.Right; transform.rotation = Quaternion.Euler(0f, 0f, -90f); }
        else if (dir.x < 0) { facingDirection = ArrowFacingDirection.Left; transform.rotation = Quaternion.Euler(0f, 0f, 90f); }
        else if (dir.y > 0) { facingDirection = ArrowFacingDirection.Up; transform.rotation = Quaternion.Euler(0f, 0f, 0f); }
        else if (dir.y < 0) { facingDirection = ArrowFacingDirection.Down; transform.rotation = Quaternion.Euler(0f, 0f, 180f); }
        // Setup line renderer
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = pointsOfArrow.Count;
        lineRenderer.useWorldSpace = true;

        for (int i = 0; i < pointsOfArrow.Count; i++)
        {
            lineRenderer.SetPosition(i, pointsOfArrow[i].transform.position);
            pointsOfArrow[i].Occupy();
        }
        // Optional visuals
        lineRenderer.numCornerVertices = 20;
        lineRenderer.numCapVertices = 20;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;

        Vector3 pos = lineRenderer.GetPosition(0);
        switch (facingDirection)
        {
            case ArrowFacingDirection.Left:
                pos.x -= 0.05f;
                break;
            case ArrowFacingDirection.Right:
                pos.x += 0.05f;
                break;
            case ArrowFacingDirection.Up:
                pos.y += 0.05f;
                break;
            case ArrowFacingDirection.Down:
                pos.y -= 0.05f;
                break;
        }
        transform.position = pos;
    }
    public ArrowFacingDirection GetDirection() => facingDirection;
    //spawn aa
    public IEnumerator RemoveFromLeft()
    {
        Collider2D collider = GetComponent<Collider2D>();
        collider.enabled = false;
        Dot previousDot = pointsOfArrow[0];
        while (lineRenderer.GetPosition(lineRenderer.positionCount - 1).x > -50)
        {
            for (int i = 0; i < pointsOfArrow.Count; i++)
            {
                Dot temp = pointsOfArrow[i];
                if (i > 0)
                {
                    pointsOfArrow[i] = previousDot;
                }
                else
                {
                    if (pointsOfArrow[i] != null)
                        pointsOfArrow[i] = pointsOfArrow[i].GetDotOnLeft();
                    else
                        pointsOfArrow[i] = null;
                }
                previousDot = temp;
            }
            for (int i = lineRenderer.positionCount - 1; i >= 0; i--)
            {
                if (pointsOfArrow[i] != null)
                {
                    while(Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pointsOfArrow[i].transform.position);
                }
                else
                {
                    Vector3 pos = lineRenderer.GetPosition(i);
                    pos.x -= 0.3f;
                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pos)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pos, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pos);
                }
            }
            yield return null;
        }
    }
    public IEnumerator RemoveFromRight()
    {
        Collider2D collider = GetComponent<Collider2D>();
        collider.enabled = false;
        Dot previousDot = pointsOfArrow[0];
        while (lineRenderer.GetPosition(lineRenderer.positionCount - 1).x < 50)
        {
            for (int i = 0; i < pointsOfArrow.Count; i++)
            {
                Dot temp = pointsOfArrow[i];
                if (i > 0)
                {
                    pointsOfArrow[i] = previousDot;
                }
                else
                {
                    if (pointsOfArrow[i] != null)
                        pointsOfArrow[i] = pointsOfArrow[i].GetDotOnRight();
                    else
                        pointsOfArrow[i] = null;
                }
                previousDot = temp;
            }
            for (int i = lineRenderer.positionCount - 1; i >= 0; i--)
            {
                if (pointsOfArrow[i] != null)
                {
                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pointsOfArrow[i].transform.position);
                }
                else
                {
                    Vector3 pos = lineRenderer.GetPosition(i);
                    pos.x += 0.3f;

                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pos)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pos, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pos);
                }
            }
            yield return null;
        }
    }
    public IEnumerator RemoveFromTop()
    {
        Collider2D collider = GetComponent<Collider2D>();
        collider.enabled = false;
        Dot previousDot = pointsOfArrow[0];
        while (lineRenderer.GetPosition(lineRenderer.positionCount - 1).y < 50)
        {
            for (int i = 0; i < pointsOfArrow.Count; i++)
            {
                Dot temp = pointsOfArrow[i];
                if (i > 0)
                {
                    pointsOfArrow[i] = previousDot;
                }
                else
                {
                    if (pointsOfArrow[i] != null)
                        pointsOfArrow[i] = pointsOfArrow[i].GetDotOnTop();
                    else
                        pointsOfArrow[i] = null;
                }
                previousDot = temp;
            }
            for (int i = lineRenderer.positionCount - 1; i >= 0; i--)
            {
                if (pointsOfArrow[i] != null)
                {
                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pointsOfArrow[i].transform.position);
                }
                else
                {
                    Vector3 pos = lineRenderer.GetPosition(i);
                    pos.y += 0.3f;

                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pos)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pos, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pos);
                }
            }
            yield return null;
        }
    }
    public IEnumerator RemoveFromBottom()
    {
        Collider2D collider = GetComponent<Collider2D>();
        collider.enabled = false;
        Dot previousDot = pointsOfArrow[0];
        while (lineRenderer.GetPosition(lineRenderer.positionCount - 1).y > -50)
        {
            for (int i = 0; i < pointsOfArrow.Count; i++)
            {
                Dot temp = pointsOfArrow[i];
                if (i > 0)
                {
                    pointsOfArrow[i] = previousDot;
                }
                else
                {
                    if (pointsOfArrow[i] != null)
                        pointsOfArrow[i] = pointsOfArrow[i].GetDotOnBottom();
                    else
                        pointsOfArrow[i] = null;
                }
                previousDot = temp;
            }
            for (int i = lineRenderer.positionCount - 1; i >= 0; i--)
            {
                if (pointsOfArrow[i] != null)
                {
                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pointsOfArrow[i].transform.position);
                }
                else
                {
                    Vector3 pos = lineRenderer.GetPosition(i);
                    pos.y -= 0.3f;

                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pos)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pos, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pos);
                }
            }
            yield return null;
        }
    }
    public IEnumerator RemoveFromBottomFail()
    {
        Collider2D collider = GetComponent<Collider2D>();
        collider.enabled = false;
        Dot previousDot = pointsOfArrow[0];
        Stack<Dot> lastDotsFromLastPoint = new Stack<Dot>();
        float failPosY = pointsOfArrow[0].Grid.GetClosesetDotOnBottom(pointsOfArrow[0]).transform.position.y;

        while (lineRenderer.GetPosition(0).y > failPosY)
        {
            for (int i = 0; i < pointsOfArrow.Count; i++)
            {
                Dot temp = pointsOfArrow[i];
                if (i > 0)
                {
                    if(i == pointsOfArrow.Count - 1) { lastDotsFromLastPoint.Push(pointsOfArrow[i]); }
                    pointsOfArrow[i] = previousDot;
                }
                else
                {
                    if (pointsOfArrow[i] != null)
                        pointsOfArrow[i] = pointsOfArrow[i].GetDotOnBottom();
                    else
                        pointsOfArrow[i] = null;
                }
                previousDot = temp;
            }
            for (int i = lineRenderer.positionCount - 1; i >= 0; i--)
            {
                if (pointsOfArrow[i] != null)
                {
                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pointsOfArrow[i].transform.position);
                }
                else
                {
                    Vector3 pos = lineRenderer.GetPosition(i);
                    pos.y -= 0.3f;

                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pos)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pos, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pos);
                }
            }
            yield return null;
        }
        while(lastDotsFromLastPoint.Count > 0)
        {
            for (int i = pointsOfArrow.Count - 1; i >= 0; i--)
            {
                Dot temp = pointsOfArrow[i];
                if (i < pointsOfArrow.Count -1)
                {
                    pointsOfArrow[i] = previousDot;
                }
                else
                {
                    if (i == pointsOfArrow.Count - 1)
                        pointsOfArrow[i] = lastDotsFromLastPoint.Pop();
                    else
                        pointsOfArrow[i] = null;
                }
                previousDot = temp;
            }
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                if (pointsOfArrow[i] != null)
                {
                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pointsOfArrow[i].transform.position);
                }
                else
                {
                    Vector3 pos = lineRenderer.GetPosition(i);
                    pos.y += 0.3f;

                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pos)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pos, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pos);
                }
            }
        }
        collider.enabled = true;
    }
    public IEnumerator RemoveFromTopFail()
    {
        Collider2D collider = GetComponent<Collider2D>();
        collider.enabled = false;
        Dot previousDot = pointsOfArrow[0];
        Stack<Dot> lastDotsFromLastPoint = new Stack<Dot>();
        float failPosY = pointsOfArrow[0].Grid.GetClosesetDotOnTop(pointsOfArrow[0]).transform.position.y;
        Debug.Log(failPosY + " this is fail pos y");

        while (lineRenderer.GetPosition(0).y < failPosY)
        {
            Debug.Log(lineRenderer.GetPosition(0).y);
            for (int i = 0; i < pointsOfArrow.Count; i++)
            {
                Dot temp = pointsOfArrow[i];
                if (i > 0)
                {
                    if (i == pointsOfArrow.Count - 1) { lastDotsFromLastPoint.Push(pointsOfArrow[i]); }
                    pointsOfArrow[i] = previousDot;
                }
                else
                {
                    if (pointsOfArrow[i] != null)
                    {
                        pointsOfArrow[i] = pointsOfArrow[i].GetDotOnTop();
                    }
                    else
                    {
                        pointsOfArrow[i] = null;
                    }
                }
                previousDot = temp;
            }
            for (int i = lineRenderer.positionCount - 1; i >= 0; i--)
            {
                if (pointsOfArrow[i] != null)
                {
                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pointsOfArrow[i].transform.position);
                }
                else
                {
                    Vector3 pos = lineRenderer.GetPosition(i);
                    pos.y += 0.3f;

                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pos)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pos, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pos);
                }
            }
            yield return null;
        }
        while (lastDotsFromLastPoint.Count > 0)
        {
            for (int i = pointsOfArrow.Count - 1; i >= 0; i--)
            {
                Dot temp = pointsOfArrow[i];
                if (i < pointsOfArrow.Count - 1)
                {
                    pointsOfArrow[i] = previousDot;
                }
                else
                {
                    if (pointsOfArrow[i] != null)
                    {
                        if(i == pointsOfArrow.Count - 1) { pointsOfArrow[i] = lastDotsFromLastPoint.Pop(); }
                    }

                    else
                    {
                        pointsOfArrow[i] = null;
                    }
                }
                previousDot = temp;
            }
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                if (pointsOfArrow[i] != null)
                {
                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pointsOfArrow[i].transform.position);
                }
                else
                {
                    Vector3 pos = lineRenderer.GetPosition(i);
                    pos.y -= 0.3f;

                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pos)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pos, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pos);
                }
            }
        }
        collider.enabled = true;
    }
    public IEnumerator RemoveFromRightFail()
    {
        Collider2D collider = GetComponent<Collider2D>();
        collider.enabled = false;
        Dot previousDot = pointsOfArrow[0];
        Stack<Dot> lastDotsFromLastPoint = new();
        float failPosX = pointsOfArrow[0].Grid.GetClosesetDotOnRight(pointsOfArrow[0]).transform.position.x;

        while (lineRenderer.GetPosition(0).x < failPosX)
        {
            for (int i = 0; i < pointsOfArrow.Count; i++)
            {
                Dot temp = pointsOfArrow[i];
                if (i > 0)
                {
                    if(i == pointsOfArrow.Count - 1) { lastDotsFromLastPoint.Push(pointsOfArrow[i]); }
                    pointsOfArrow[i] = previousDot;
                }
                else
                {
                    if (pointsOfArrow[i] != null)
                        pointsOfArrow[i] = pointsOfArrow[i].GetDotOnRight();
                    else
                        pointsOfArrow[i] = null;
                }
                previousDot = temp;
            }
            for (int i = lineRenderer.positionCount - 1; i >= 0; i--)
            {
                if (pointsOfArrow[i] != null)
                {
                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pointsOfArrow[i].transform.position);
                }
                else
                {
                    Vector3 pos = lineRenderer.GetPosition(i);
                    pos.y += 0.3f;

                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pos)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pos, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pos);
                }
            }
            yield return null;
        }
        while (lastDotsFromLastPoint.Count > 0)
        {
            for (int i = pointsOfArrow.Count - 1; i >= 0; i--)
            {
                Dot temp = pointsOfArrow[i];
                if (i < pointsOfArrow.Count - 1)
                {
                    pointsOfArrow[i] = previousDot;
                }
                else
                {
                    if(i == pointsOfArrow.Count - 1) { pointsOfArrow[i] = lastDotsFromLastPoint.Pop(); }
                    else
                        pointsOfArrow[i] = null;
                }
                previousDot = temp;
            }
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                if (pointsOfArrow[i] != null)
                {
                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pointsOfArrow[i].transform.position);
                }
                else
                {
                    Vector3 pos = lineRenderer.GetPosition(i);
                    pos.y -= 0.3f;

                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pos)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pos, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pos);
                }
            }
        }
        collider.enabled = true;
    }
    public IEnumerator RemoveFromLeftFail()
    {
        Collider2D collider = GetComponent<Collider2D>();
        collider.enabled = false;
        Dot previousDot = pointsOfArrow[0];
        Stack<Dot> lastDotsFromLastPoint = new Stack<Dot>();
        float failPosX = pointsOfArrow[0].Grid.GetClosesetDotOnLeft(pointsOfArrow[0]).transform.position.x;

        while (lineRenderer.GetPosition(0).x > failPosX)
        {
            for (int i = 0; i < pointsOfArrow.Count; i++)
            {
                Dot temp = pointsOfArrow[i];
                if (i > 0)
                {
                    if(i == pointsOfArrow.Count - 1) { lastDotsFromLastPoint.Push(pointsOfArrow[i]); }
                    pointsOfArrow[i] = previousDot;
                }
                else
                {
                    if (pointsOfArrow[i] != null)
                        pointsOfArrow[i] = pointsOfArrow[i].GetDotOnLeft();
                    else
                        pointsOfArrow[i] = null;
                }
                previousDot = temp;
            }
            for (int i = lineRenderer.positionCount - 1; i >= 0; i--)
            {
                if (pointsOfArrow[i] != null)
                {
                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pointsOfArrow[i].transform.position);
                }
                else
                {
                    Vector3 pos = lineRenderer.GetPosition(i);
                    pos.y -= 0.3f;

                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pos)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pos, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pos);
                }
            }
            yield return null;
        }
        while (lastDotsFromLastPoint.Count > 0)
        {
            for (int i = pointsOfArrow.Count - 1; i >= 0; i--)
            {
                Dot temp = pointsOfArrow[i];
                if (i < pointsOfArrow.Count - 1)
                {
                    pointsOfArrow[i] = previousDot;
                }
                else
                {
                    if(i == pointsOfArrow.Count - 1)
                    {
                        pointsOfArrow[i] = lastDotsFromLastPoint.Pop();
                    }
                }
                previousDot = temp;
            }
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                if (pointsOfArrow[i] != null)
                {
                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pointsOfArrow[i].transform.position, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pointsOfArrow[i].transform.position);
                }
                else
                {
                    Vector3 pos = lineRenderer.GetPosition(i);
                    pos.y += 0.3f;

                    while (Mathf.Abs(Vector2.Distance(lineRenderer.GetPosition(i), pos)) > 0.1f)
                    {
                        Vector2 newPos = Vector2.MoveTowards(lineRenderer.GetPosition(i), pos, arrowSpeed * Time.deltaTime);
                        lineRenderer.SetPosition(i, newPos);
                        yield return null;
                    }
                    lineRenderer.SetPosition(i, pos);
                }
            }
        }
        collider.enabled = true;
    }

    private void OnDisable()
    {
        GameManager.OnToggleGridLines -= ToggleLinesVisibility;
    }
}
public enum ArrowFacingDirection { Left, Right, Up, Down}