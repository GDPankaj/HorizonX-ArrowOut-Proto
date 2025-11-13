using UnityEngine;

public class Dot : MonoBehaviour
{
    [SerializeField]private Vector2Int positionInGrid = new Vector2Int();
    [SerializeField]private bool isOccupied = true;
    SpriteRenderer _renderer;
    public bool IsOccupied => isOccupied;
    private DotGrid grid;
    public DotGrid Grid => grid;
    private void Awake()
    {
        grid = GetComponentInParent<DotGrid>();
        _renderer = GetComponent<SpriteRenderer>();
        if (isOccupied)
        {
            Occupy();
        }
        else
        {
            DeOccupy();
        }
    }
    public void DeOccupy()
    {
        isOccupied = false;
        if (_renderer)
        {
            _renderer.enabled = true;
            _renderer.color = Color.gray;
        }
    }
    public void Occupy()
    {
        isOccupied = true;
        if(_renderer)
        _renderer.enabled = false;
    }

    public void SetupDot(int x, int y)
    {
        positionInGrid.x = x;
        positionInGrid.y = y;
        Debug.Log(transform.position + " this is world position I guess of " + name);
        Debug.Log(transform.localPosition + " this is local position I guess of " + name);
    }

    public int GetX() { return positionInGrid.x; }
    public int GetY() { return positionInGrid.y; }

    public Dot GetDotOnLeft()
    {
        return grid.GetDotInGrid(new Vector2Int(positionInGrid.x - 1, positionInGrid.y));
    }
    public Dot GetDotOnRight()
    {
        return grid.GetDotInGrid(new Vector2Int(positionInGrid.x + 1, positionInGrid.y));
    }
    public Dot GetDotOnTop()
    {
        return grid.GetDotInGrid(new Vector2Int(positionInGrid.x, positionInGrid.y + 1));
    }
    public Dot GetDotOnBottom()
    {
        return grid.GetDotInGrid(new Vector2Int(positionInGrid.x, positionInGrid.y - 1));
    }
}
