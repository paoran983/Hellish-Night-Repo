using System;
using UnityEngine;

public class Grid<T>
{
    private T[,] gridArray;
    private float nodeRadius, nodeDiameter, detectorOffest, worldSizeX, worldSizeY;
    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public event EventHandler<OnGridVisualChangedEventArgs> OnGridVisualChanged;
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    public class OnGridVisualChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
        public Vector2 uv00;
        public Vector2 uv11;
    }
    private int gridSizeX, gridSizeY;
    private Vector3 worldBottomLeft;
    public Grid(float worldSizeX, float worldSizeY, float nodeRadius, Func<T> createGridObject)
    {
        this.worldSizeX = worldSizeX;
        this.worldSizeY = worldSizeY;
        this.nodeRadius = nodeRadius;
        initializeData();
        gridArray = new T[gridSizeX, gridSizeY];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                gridArray[x, y] = createGridObject();
            }
        }

        drawDebug(false);
    }
    public Grid(int worldSizeX, int worldSizeY, float nodeRadius, Func<Grid<T>, int, int, T> createGridObject)
    {
        this.worldSizeX = worldSizeX;
        this.worldSizeY = worldSizeY;
        this.nodeRadius = nodeRadius;
        initializeData();
        gridArray = new T[gridSizeX, gridSizeY];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }

        drawDebug(false);
    }
    public void initializeData()
    {
        this.nodeDiameter = nodeRadius * 2;
        this.gridSizeX = Mathf.RoundToInt(this.worldSizeX / nodeDiameter);
        this.gridSizeY = Mathf.RoundToInt(this.worldSizeY / nodeDiameter);
        this.worldBottomLeft = Vector3.zero - Vector3.right * (worldSizeX) / 2 - Vector3.up * (worldSizeY) / 2;
    }
    public void drawDebug(bool showDebug)
    {
        if (showDebug)
        {
            TextMesh[,] debugTextArray = new TextMesh[gridSizeX, gridSizeY];

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    // debugTextArray[x, y] = UtilsClass.CreateWorldText(gridArray[x, y]?.ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f, 30, Color.white, TextAnchor.MiddleCenter);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + (int)nodeDiameter), Color.red, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + (int)nodeDiameter, y), Color.red, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, gridSizeY), GetWorldPosition(gridSizeX, gridSizeY), Color.red, 100f);
            Debug.DrawLine(GetWorldPosition(gridSizeX, 0), GetWorldPosition(gridSizeX, gridSizeY), Color.red, 100f);


        }
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }
    public void TriggerGridVisualChanged(int x, int y, Vector2 uv00, Vector2 uv11)
    {
        if (OnGridVisualChanged != null)
        {
            OnGridVisualChanged.Invoke(this, new OnGridVisualChangedEventArgs { x = x, y = y, uv00 = uv00, uv11 = uv11 });
        }

    }

    public int MaxSize()
    {
        return gridSizeX * gridSizeY;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * nodeDiameter + worldBottomLeft;
    }
    public T NodeFromWorldPoint(Vector3 worldPos)
    {
        float percentX = (worldPos.x + gridSizeX / 2) / gridSizeX;
        float percentY = (worldPos.y + gridSizeY / 2) / gridSizeY;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - nodeRadius) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - nodeRadius) * percentY);
        return gridArray[x, y];
    }

    public T GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < gridSizeX && y < gridSizeY)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(T);
        }
    }
    public void SetGridObject(Vector3 worldPos, T value)
    {
        float percentX = (worldPos.x + gridSizeX / 2) / gridSizeX;
        float percentY = (worldPos.y + gridSizeY / 2) / gridSizeY;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        SetGridObject(x, y, value);
    }
    public void SetGridObject(int x, int y, T value)
    {
        if (x >= 0 && y >= 0 && x < gridSizeX && y < gridSizeY)
        {
            gridArray[x, y] = value;
            TriggerGridObjectChanged(x, y);
        }
    }

    public int GridSizeX
    {
        get
        {
            return gridSizeX;
        }
    }
    public int GridSizeY
    {
        get
        {
            return gridSizeY;
        }
    }
    public T[,] GridArray
    {
        get
        {
            return gridArray;
        }
    }
    public float NodeDiameter
    {
        get
        {
            return nodeDiameter;
        }
    }


}

/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class Grid<TGridObject> {

    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs {
        public int x;
        public int y;
    }

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;

    public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++) {
            for (int y = 0; y < gridArray.GetLength(1); y++) {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }

        bool showDebug = false;
        if (showDebug) {
            TextMesh[,] debugTextArray = new TextMesh[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++) {
                for (int y = 0; y < gridArray.GetLength(1); y++) {
                    debugTextArray[x, y] = UtilsClass.CreateWorldText(gridArray[x, y]?.ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f, 30, Color.white, TextAnchor.MiddleCenter);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

            OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) => {
                debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
            };
        }
    }

    public int GetWidth() {
        return width;
    }

    public int GetHeight() {
        return height;
    }

    public float GetCellSize() {
        return cellSize;
    }

    public Vector3 GetWorldPosition(int x, int y) {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y) {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
    }

    public void SetGridObject(int x, int y, TGridObject value) {
        if (x >= 0 && y >= 0 && x < width && y < height) {
            gridArray[x, y] = value;
            TriggerGridObjectChanged(x, y);
        }
    }

    public void TriggerGridObjectChanged(int x, int y) {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value) {
        GetXY(worldPosition, out int x, out int y);
        SetGridObject(x, y, value);
    }

    public TGridObject GetGridObject(int x, int y) {
        if (x >= 0 && y >= 0 && x < width && y < height) {
            return gridArray[x, y];
        } else {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector3 worldPosition) {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }

}
*/