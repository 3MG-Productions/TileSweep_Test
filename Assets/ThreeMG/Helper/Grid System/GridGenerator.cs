using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
#endif

namespace ThreeMG.Helper.GridSystem
{
    public class GridGenerator : MonoBehaviour
    {
        // Enum for specifying the starting corner of the grid
        public enum StartCorner
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        // Enum for specifying if we spawn rows first or columns first
        public enum SpawnDirection
        {
            RowsFirst,
            ColumnsFirst
        }

        // Grid configuration variables
        public int rows = 5;  // Number of rows
        public int columns = 5;  // Number of columns
        public Vector2 cellSize = new Vector2(1f, 1f);  // Size of each grid cell
        public Vector2 spacing = new Vector2(0.1f, 0.1f);  // Space between grid cells
        public StartCorner startCorner = StartCorner.TopLeft;  // Starting corner
        public SpawnDirection spawnDirection = SpawnDirection.RowsFirst;  // Direction of spawning

        public void GenerateGrid(Vector3 origin)
        {
            // List to store the grid positions
            List<Vector3> gridPositions = new List<Vector3>();

            // Adjust the origin based on the starting corner
            Vector3 adjustedOrigin = AdjustOriginForStartCorner(origin);

            // Generate the grid positions
            gridPositions = GenerateGridPositions(adjustedOrigin);

            // Spawn objects or handle positions (for example purposes, just draw gizmos here)
            for (int i = 0; i < gridPositions.Count; i++)
            {
                // For now, we'll draw the grid positions using gizmos, but you can instantiate objects here
                Gizmos.DrawWireCube(gridPositions[i], new Vector3(cellSize.x, cellSize.y, 0.1f));
            }
        }

        // Method to adjust the origin based on the starting corner
        private Vector3 AdjustOriginForStartCorner(Vector3 origin)
        {
            Vector3 adjustedOrigin = origin;

            // Offset the origin based on the start corner and grid dimensions
            float width = (columns - 1) * (cellSize.x + spacing.x);
            float height = (rows - 1) * (cellSize.y + spacing.y);

            switch (startCorner)
            {
                case StartCorner.TopLeft:
                    // No changes needed, origin is at the top left
                    break;
                case StartCorner.TopRight:
                    adjustedOrigin.x -= width;  // Shift origin to the left by grid width
                    break;
                case StartCorner.BottomLeft:
                    adjustedOrigin.y += height;  // Shift origin up by grid height
                    break;
                case StartCorner.BottomRight:
                    adjustedOrigin.x -= width;  // Shift origin to the left
                    adjustedOrigin.y += height;  // Shift origin up
                    break;
            }

            return adjustedOrigin;
        }

        // Method to generate grid positions based on the spawn direction
        private List<Vector3> GenerateGridPositions(Vector3 origin)
        {
            List<Vector3> positions = new List<Vector3>();

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    Vector3 position = CalculatePosition(r, c, origin);
                    positions.Add(position);
                }
            }

            // Rearrange positions if we want to spawn columns first
            if (spawnDirection == SpawnDirection.ColumnsFirst)
            {
                positions = RearrangeForColumnsFirst(positions);
            }

            return positions;
        }

        // Calculate individual grid positions based on row and column
        private Vector3 CalculatePosition(int row, int column, Vector3 origin)
        {
            float x = origin.x + (column * (cellSize.x + spacing.x));
            float y = origin.y - (row * (cellSize.y + spacing.y));
            return new Vector3(x, y, 0f);
        }

        // Method to rearrange the positions to handle columns first spawning
        private List<Vector3> RearrangeForColumnsFirst(List<Vector3> originalPositions)
        {
            List<Vector3> rearrangedPositions = new List<Vector3>();

            for (int c = 0; c < columns; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    int index = r * columns + c;
                    rearrangedPositions.Add(originalPositions[index]);
                }
            }

            return rearrangedPositions;
        }

        // Draw grid gizmos in the scene (for visualization in the Unity editor)
        private void OnDrawGizmos()
        {
            GenerateGrid(transform.position);
        }
    }
}