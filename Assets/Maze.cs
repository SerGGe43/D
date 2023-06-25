using UnityEngine;
using System.Collections.Generic;
public class Maze
{
    public MazeGeneratorCell[,] cells;
    public List<Vector2Int> finishPosition = new List<Vector2Int>(3);
}

public class MazeGeneratorCell
{
    public int X;
    public int Y;

    public int Set;

    public bool WallLeft = true;
    public bool WallBottom = true;

    public bool Visited = false;
    public int DistanceFromStart;
}
