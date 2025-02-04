/*---------------------------------------------------------------------
 *  Class: Tilemap
 *
 *  Purpose: Controls the underdlying grid of a tileMap
 *           does this by making a Grid of TilemapObjects.
 *           This tileMap will control an array of sprites that make up a floor
 *-------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using UnityEngine;


public class Tilemap
{

    public event EventHandler OnLoaded;

    private Grid<TilemapObject> grid;

    public Tilemap(int width, int height, float cellSize)
    {
        grid = new Grid<TilemapObject>(width, height, cellSize, (Grid<TilemapObject> g, int x, int y) => new TilemapObject(g, x, y));
    }


    /*---------------------------------------------------------------------
     *  Method:  SetTilemapVisual(TilemapVisual tilemapVisual)
     *
     *  Purpose: Sets the tileMapVisual for the tileMap
     *  Parameters: TilemapVisual tilemapVisual = the visual the assign the tilemap to
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void SetTilemapVisual(TilemapVisual tilemapVisual)
    {
        tilemapVisual.SetGrid(this, grid);
    }


    public void ChangeTilemapNode(int x, int y, Vector2 uv00, Vector2 uv11)
    {
        grid.TriggerGridVisualChanged(x, y, uv00, uv11);


    }
    /*---------------------------------------------------------------------
     *  Method:  ActivateTilemap(Vector3 worldPos)
     *
     *  Purpose: Sets the sprite for a tileMapObject to be a specific part of a mesh material
     *  Parameters: Vector3 worldPos = position of the tielMapObject to be cleared
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    public void ActivateTilemap(Vector3 worldPos)
    {

        TilemapObject tilemapObject = grid.NodeFromWorldPoint(worldPos);
        if (tilemapObject != null)
        {

            grid.TriggerGridVisualChanged(tilemapObject.X, tilemapObject.Y, new Vector2(1, 0), new Vector2(0, 1));
        }

    }

    /*---------------------------------------------------------------------
     *  Method: ClearTileMap(Vector3 worldPos)
     *
     *  Purpose: Sets the sprite for a tileMapObject to be a default part of a mesh material
     *  Parameters: Vector3 worldPos = position of the tielMapObject to be cleared
     *
     *  Returns: none
     *-------------------------------------------------------------------*/
    internal void ClearTileMap(Vector3 worldPos)
    {
        TilemapObject tilemapObject = grid.NodeFromWorldPoint(worldPos);
        if (tilemapObject != null)
        {
            grid.TriggerGridVisualChanged(tilemapObject.X, tilemapObject.Y, new Vector2(0, 0), new Vector2(0, 0));
        }
    }
    public TilemapObject NodeFromWorldPoint(Vector3 worldPos)
    {
        return grid.NodeFromWorldPoint(worldPos);

    }


    /*---------------------------------------------------------------------
     *  Class: TilemapObject
     *
     *  Purpose: Represents a single Tilemap Object that 
     *           exists in each Grid Cell Position
     *-------------------------------------------------------------------*/
    public class TilemapObject
    {

        public enum TilemapSprite
        {
            None,
            InRange,
            Attack,
            Selected,
            CurTurn,
            UpOrDown,
            LeftOrRIght,
            UpToLeft,
            UpToRight,
            DownToLeft,
            DownToRight,
            ArrowUp,
            ArrowDown,
            ArrowLeft,
            ArrowRight,
            DiagLeft,
            DiagRight,

        }

        private Grid<TilemapObject> grid;
        private int x;
        private int y;
        private TilemapSprite lastTilemapSprite;
        private TilemapSprite tilemapSprite;

        public TilemapObject(Grid<TilemapObject> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
        }

        /*---------------------------------------------------------------------
         *  Method: SetTilemapSprite(TilemapSprite tilemapSprite)
         *
         *  Purpose: Sets the sprite for a tileMapObject
         *  Parameters: TilemapSprite tilemapSprite = the sprite to assign
         *
         *  Returns: none
         *-------------------------------------------------------------------*/
        public void SetTilemapSprite(TilemapSprite tilemapSprite)
        {
            if (tilemapSprite == this.tilemapSprite)
            {
                return;
            }
            TilemapSprite cur = this.tilemapSprite;
            this.lastTilemapSprite = cur;
            this.tilemapSprite = tilemapSprite;

            grid.TriggerGridObjectChanged(x, y);
        }
        public void RevertTilemapSprite()
        {
            if (lastTilemapSprite.Equals(this.tilemapSprite)) { return; }
            TilemapSprite last = this.tilemapSprite;
            this.tilemapSprite = this.lastTilemapSprite;
            this.lastTilemapSprite = last;
            grid.TriggerGridObjectChanged(x, y);
        }
        public TilemapSprite GetTilemapSprite()
        {
            return tilemapSprite;
        }
        public TilemapSprite GetLastTilemapSprite()
        {
            return lastTilemapSprite;
        }

        public override string ToString()
        {
            return tilemapSprite.ToString();
        }


        /*
        * Save - Load
        * */
        [System.Serializable]
        public class SaveObject
        {
            public TilemapSprite tilemapSprite;
            public int x;
            public int y;
        }

        public SaveObject Save()
        {
            return new SaveObject
            {
                tilemapSprite = tilemapSprite,
                x = x,
                y = y,
            };
        }

        public void Load(SaveObject saveObject)
        {
            tilemapSprite = saveObject.tilemapSprite;
        }
        public int X
        {
            get { return x; }
            set
            {
                x = value;
            }
        }
        public int Y
        {
            get { return y; }
            set
            {
                y = value;
            }
        }
    }

    /*
     * Save - Load
     * */
    public class SaveObject
    {
        public TilemapObject.SaveObject[] tilemapObjectSaveObjectArray;
    }

    public void Save()
    {
        List<TilemapObject.SaveObject> tilemapObjectSaveObjectList = new List<TilemapObject.SaveObject>();
        for (int x = 0; x < grid.GridSizeX; x++)
        {
            for (int y = 0; y < grid.GridSizeY; y++)
            {
                TilemapObject tilemapObject = grid.GetGridObject(x, y);
                tilemapObjectSaveObjectList.Add(tilemapObject.Save());
            }
        }

        SaveObject saveObject = new SaveObject { tilemapObjectSaveObjectArray = tilemapObjectSaveObjectList.ToArray() };

        SaveSystem.SaveObject(saveObject);
    }

    public void Load()
    {
        SaveObject saveObject = SaveSystem.LoadMostRecentObject<SaveObject>();
        foreach (TilemapObject.SaveObject tilemapObjectSaveObject in saveObject.tilemapObjectSaveObjectArray)
        {
            TilemapObject tilemapObject = grid.GetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.y);
            tilemapObject.Load(tilemapObjectSaveObject);
        }
        OnLoaded?.Invoke(this, EventArgs.Empty);
    }

}

