using UnityEngine;
using Sirenix.OdinInspector;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ThreeMG.Helper.GridSystem
{
    // Enum to specify the starting direction of the grid generation
    public enum GridStartDirection
    {
        TopToBottom,
        BottomToTop
    }

    // Enum to specify the traversal order of the grid
    public enum GridTraversalOrder
    {
        RowsFirst,   // Fill by rows (left to right)
        ColumnsFirst // Fill by columns (top to bottom or bottom to top)
    }

    public class GridManager : Singleton<GridManager>
    {
        public GridConfig GridConfig;
        private GameObject gridParent;
        private GameObject[][] tiles;

        public GridStartDirection StartDirection = GridStartDirection.BottomToTop;
        public GridTraversalOrder TraversalOrder = GridTraversalOrder.RowsFirst;

        [Button]
        public void GenerateGrid()
        {
            GenerateGrid(GridConfig, StartDirection, TraversalOrder);
        }

        public void GenerateGrid(GridConfig gridConfig, GridStartDirection startDirection, GridTraversalOrder traversalOrder, GameObject parent = null)
        {
            GameObject tilePrefab = gridConfig.TilePrefab;
            Vector2 GridSize = gridConfig.GridSize; // GridSize.x is rows, GridSize.y is columns
            Vector2 tileScale = gridConfig.TileScale;
            Vector2 Offset = gridConfig.TileOffset;

            if (gridParent != null)
            {
                DestroyImmediate(gridParent);
            }

            gridParent = parent == null ? new GameObject("Grid") : parent;

            // Initialize the tiles array
            tiles = new GameObject[(int)GridSize.x][];  // x (rows) will define the first dimension

            // Calculate the origin position (bottom-left of the grid)
            Vector3 originPos = new Vector3(-GridSize.y * tileScale.y / 2 + tileScale.y / 2, 0, -GridSize.x * tileScale.x / 2 + tileScale.x / 2);

            if (traversalOrder == GridTraversalOrder.RowsFirst)
            {
                GenerateByRows(gridConfig, originPos, tilePrefab, GridSize, tileScale, Offset, startDirection);
            }
            else
            {
                GenerateByColumns(gridConfig, originPos, tilePrefab, GridSize, tileScale, Offset, startDirection);
            }
        }

        // This method generates the grid row by row (x is rows, y is columns)
        private void GenerateByRows(GridConfig gridConfig, Vector3 originPos, GameObject tilePrefab, Vector2 gridSize, Vector2 tileScale, Vector2 offset, GridStartDirection startDirection)
        {
            // Rows are handled by the outer loop (x axis)
            int rowStart = startDirection == GridStartDirection.BottomToTop ? 0 : (int)gridSize.x - 1;
            int rowEnd = startDirection == GridStartDirection.BottomToTop ? (int)gridSize.x : -1;
            int rowStep = startDirection == GridStartDirection.BottomToTop ? 1 : -1;

            for (int row = rowStart; row != rowEnd; row += rowStep)
            {
                tiles[row] = new GameObject[(int)gridSize.y]; // Each row has gridSize.y columns

                for (int col = 0; col < gridSize.y; col++)
                {
                    Vector3 position = new Vector3(col * tileScale.y + offset.y, 0, row * tileScale.x + offset.x);
                    position = originPos + position;

                    GameObject tile = InstantiateTile(tilePrefab, position);
                    tile.name = $"Tile ({row}, {col})";
                    tiles[row][col] = tile;
                }
            }
        }

        // This method generates the grid column by column (x is rows, y is columns)
        private void GenerateByColumns(GridConfig gridConfig, Vector3 originPos, GameObject tilePrefab, Vector2 gridSize, Vector2 tileScale, Vector2 offset, GridStartDirection startDirection)
        {
            // Columns are handled by the outer loop (y axis)
            int colStart = startDirection == GridStartDirection.BottomToTop ? 0 : (int)gridSize.y - 1;
            int colEnd = startDirection == GridStartDirection.BottomToTop ? (int)gridSize.y : -1;
            int colStep = startDirection == GridStartDirection.BottomToTop ? 1 : -1;

            for (int col = colStart; col != colEnd; col += colStep)
            {
                // Initialize each column array (since x is rows)
                tiles[col] = new GameObject[(int)gridSize.x];

                for (int row = 0; row < gridSize.x; row++)
                {
                    Vector3 position = new Vector3(col * tileScale.y + offset.y, 0, row * tileScale.x + offset.x);
                    position = originPos + position;

                    GameObject tile = InstantiateTile(tilePrefab, position);
                    tile.name = $"Tile ({row}, {col})";
                    tiles[row][col] = tile;
                }
            }
        }

        private GameObject InstantiateTile(GameObject tilePrefab, Vector3 position)
        {
#if UNITY_EDITOR
            if (GridConfig.IsPrefabLinked)
            {
                GameObject tile = PrefabUtility.InstantiatePrefab(tilePrefab) as GameObject;
                tile.transform.position = position;
                tile.transform.rotation = Quaternion.identity;
                tile.transform.SetParent(gridParent.transform);
                return tile;
            }
            else
            {
                return Instantiate(tilePrefab, position, Quaternion.identity, gridParent.transform);
            }
#else
            return Instantiate(tilePrefab, position, Quaternion.identity, gridParent.transform);
#endif
        }

        public GameObject[][] GetTiles(Vector2Int gridSize)
        {
            GridConfig.GridSize = gridSize;
            GenerateGrid();
            return tiles;
        }
    }
}