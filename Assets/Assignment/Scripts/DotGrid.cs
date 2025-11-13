using System.Collections.Generic;
using UnityEngine;

public class DotGrid : MonoBehaviour
{
    public Dictionary<Vector2Int, Dot> dotsInGrid = new Dictionary<Vector2Int, Dot>();
    private int XLimit = 0;
    private int YLimit = 0;

    private void Awake()
    {
        foreach(Transform child in transform)
        {
            Dot childDot = child.GetComponent<Dot>();
            AddElement(childDot.GetX(), childDot.GetY(), childDot);
        }

    }

    public void Init()
    {
        dotsInGrid = new Dictionary<Vector2Int, Dot>();
    }

    public Dot GetDotInGrid(Vector2Int x)
    {
        if(dotsInGrid.ContainsKey(x)) return dotsInGrid[x];
        else return null;
    }
    public void AddElement(int x, int y, Dot dot)
    {
        if(x > XLimit)
        {
            XLimit = x;
        }
        if(y > YLimit)
        {
            YLimit = y;
        }
        Vector2Int temp = new Vector2Int(x, y);
        dotsInGrid.Add(temp, dot);
    }

    public void DebugEachElement()
    {
        foreach(var v in dotsInGrid)
        {
            Debug.Log($"{v.Key[0]} {v.Key[1]} {v.Value.IsOccupied} at {v.Value.GetX()} {v.Value.GetY()}");
        }
    }

    public bool CanArrowEscapeAt(Dot dot, ArrowFacingDirection directionFacing)
    {
        bool canGo = false;
        switch (directionFacing)
        {
            case ArrowFacingDirection.Left:
                canGo = CheckDotsOnLeft(dot);
                break;
            case ArrowFacingDirection.Right:
                canGo = CheckDotsOnRight(dot);
                break;
            case ArrowFacingDirection.Up:
                canGo = CheckDotsOnTop(dot);
                break;
            case ArrowFacingDirection.Down:
                canGo = CheckDotsOnBottom(dot);
                break;
        }
        return canGo;
    }
    private bool CheckDotsOnLeft(Dot dot)
    {
        bool canGo = false;
        if(dot.GetX() == 0)
        {
            canGo = true;
            return canGo;
        }
        for(int i = dot.GetX() - 1;  i >= 0; i--)
        {
            Vector2Int x = new Vector2Int(i, dot.GetY());
            if (dotsInGrid.ContainsKey(x))
            {
                if (dotsInGrid[x].IsOccupied)
                {
                    canGo = false;
                    break;
                }
                else
                {
                    canGo = true;
                    continue;
                }
            }
            else
            {
                canGo = true;
                break;
            }
        }

        return canGo;
    }
    private bool CheckDotsOnRight(Dot dot)
    {
        bool canGo = false;
        if (dot.GetX() == XLimit)
        {
            canGo = true;
            return canGo;
        }
        for (int i = dot.GetX() + 1; i <= XLimit; i++)
        {
            Vector2Int x = new Vector2Int(i, dot.GetY());
            if (dotsInGrid.ContainsKey(x))
            {
                Debug.Log(x + dotsInGrid[x].gameObject.name);
                if (dotsInGrid[x].IsOccupied)
                {
                    canGo = false;
                    break;
                }
                else
                {
                    canGo = true;
                    continue;
                }
            }
            else
            {
                canGo = true;
                break;
            }
        }

        return canGo;
    }
    private bool CheckDotsOnTop(Dot dot)
    {
        bool canGo = false;
        if (dot.GetY() == YLimit)
        {
            canGo = true;
            return canGo;
        }
        for (int i = dot.GetY() + 1; i <= YLimit; i++)
        {
            Vector2Int x = new Vector2Int(dot.GetX(), i);
            if (dotsInGrid.ContainsKey(x))
            {
                if (dotsInGrid[x].IsOccupied)
                {
                    canGo = false;
                    break;
                }
                else
                {
                    canGo = true;
                    continue;
                }
            }
            else
            {
                canGo = true;
                break;
            }
        }

        return canGo;
    }
    private bool CheckDotsOnBottom(Dot dot)
    {
        bool canGo = false;
        if (dot.GetY() == 0)
        {
            canGo = true;
            return canGo;
        }
        for (int i = dot.GetY() - 1; i >= 0; i--)
        {
            Vector2Int x = new Vector2Int(dot.GetX(), i);
            if (dotsInGrid.ContainsKey(x))
            {
                if (dotsInGrid[x].IsOccupied)
                {
                    canGo = false;
                    break;
                }
                else
                {
                    canGo = true;
                    continue;
                }
            }
            else
            {
                canGo = true;
                break;
            }
        }

        return canGo;
    }

    public Dot GetClosesetDotOnRight(Dot dot)
    {
        Dot closesetDot = null;
        for(int i = dot.GetX() + 1; i<= XLimit; i++)
        {
            Vector2Int x = new Vector2Int(i, dot.GetY());
            if (dotsInGrid.ContainsKey(x))
            {
                if (dotsInGrid[x].IsOccupied)
                {
                    closesetDot = dotsInGrid[x];
                    break;
                }
            }
        }
        return closesetDot;
    }
    public Dot GetClosesetDotOnLeft(Dot dot)
    {
        Dot closesetDot = null;
        for (int i = dot.GetX() - 1; i >= 0; i++)
        {
            Vector2Int x = new Vector2Int(i, dot.GetY());
            if (dotsInGrid.ContainsKey(x))
            {
                if (dotsInGrid[x].IsOccupied)
                {
                    closesetDot = dotsInGrid[x];
                    break;
                }
            }
        }
        return closesetDot;
    }
    public Dot GetClosesetDotOnTop(Dot dot)
    {
        Dot closesetDot = null;
        for (int i = dot.GetY() + 1; i <= YLimit; i++)
        {
            Vector2Int x = new Vector2Int(dot.GetX(), i); 
            if (dotsInGrid.ContainsKey(x))
            {
                if (dotsInGrid[x].IsOccupied)
                {
                    closesetDot = dotsInGrid[x];
                    break;
                }
            }
        }
        return closesetDot;
    }
    public Dot GetClosesetDotOnBottom(Dot dot)
    {
        Dot closesetDot = null;
        for (int i = dot.GetY() - 1; i >= 0; i++)
        {
            Vector2Int x = new Vector2Int(dot.GetX(), i);
            if (dotsInGrid.ContainsKey(x))
            {
                if (dotsInGrid[x].IsOccupied)
                {
                    closesetDot = dotsInGrid[x];
                    break;
                }
            }
        }
        return closesetDot;
    }
}
