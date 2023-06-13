using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public ProtoCell ProtoCellPrefab;
    public Cell CellPrefab;
    public Vector3 CellSize = new Vector3(1, 1, 0);
    public HintRenderer hintRenderer;
    public int Width = 10;
    public int Height = 10;
    public bool isRemoved = false;
    public Maze maze;

    private void Start()
    {
       // MazeGenerator generator = new MazeGenerator();
        maze = GenerateMaze();
        MazeUpdate();
        StartCoroutine(RemoveWallsWithBacktracker());
    }

    private void MazeUpdate()
    {
        for (int x = 0; x < maze.cells.GetLength(0); x++)
        {
            for (int y = 0; y < maze.cells.GetLength(1); y++)
            {
                ProtoCell c = Instantiate(ProtoCellPrefab, new Vector3(x * CellSize.x, y * CellSize.y, y * CellSize.z), Quaternion.identity);

                c.WallLeft.SetActive(maze.cells[x, y].WallLeft);
                c.WallBottom.SetActive(maze.cells[x, y].WallBottom);
            }
        }
    }

    public Maze GenerateMaze()
    {
        MazeGeneratorCell[,] Cells = new MazeGeneratorCell[Width, Height];

        for (int x = 0; x < Cells.GetLength(0); x++)
        {
            for (int y = 0; y < Cells.GetLength(1); y++)
            {
                Cells[x, y] = new MazeGeneratorCell {X = x, Y = y};
            }
        }

        for (int x = 0; x < Cells.GetLength(0); x++)
        {
            Cells[x, Height - 1].WallLeft = false;
        }

        for (int y = 0; y < Cells.GetLength(1); y++)
        {
            Cells[Width - 1, y].WallBottom = false;
        }

        Maze maze = new Maze();
        maze.cells = Cells;

        return maze;
    }

    IEnumerator RemoveWallsWithBacktracker()
    {
        MazeGeneratorCell current = maze.cells[0, 0];
        current.Visited = true;
        current.DistanceFromStart = 0;

        Stack<MazeGeneratorCell> stack = new Stack<MazeGeneratorCell>();
        do
        {
            List<MazeGeneratorCell> unvisitedNeighbours = new List<MazeGeneratorCell>();

            int x = current.X;
            int y = current.Y;

            if (x > 0 && !maze.cells[x - 1, y].Visited) unvisitedNeighbours.Add(maze.cells[x - 1, y]);
            if (y > 0 && !maze.cells[x, y - 1].Visited) unvisitedNeighbours.Add(maze.cells[x, y - 1]);
            if (x < Width - 2 && !maze.cells[x + 1, y].Visited) unvisitedNeighbours.Add(maze.cells[x + 1, y]);
            if (y < Height - 2 && !maze.cells[x, y + 1].Visited) unvisitedNeighbours.Add(maze.cells[x, y + 1]);

            if (unvisitedNeighbours.Count > 0)
            {
                MazeGeneratorCell chosen = unvisitedNeighbours[UnityEngine.Random.Range(0, unvisitedNeighbours.Count)];
                //yield return new WaitForSeconds(0.5f);
                //Invoke("pause", 20f);
                RemoveWall(current, chosen);
                chosen.Visited = true;
                stack.Push(chosen);
                chosen.DistanceFromStart = current.DistanceFromStart + 1;
                current = chosen;
                Debug.Log("Before");
                yield return new WaitForSeconds(0.03f);
                Debug.Log("after");
            }
            else
            {
                current = stack.Pop();
            }

            MazeUpdate();

        } while (stack.Count > 0);

        maze.finishPosition = PlaceMazeExit();
        MazeUpdate();

        for (int x = 0; x < maze.cells.GetLength(0); x++)
        {
            for (int y = 0; y < maze.cells.GetLength(1); y++)
            {
                Cell c = Instantiate(CellPrefab, new Vector3(x * CellSize.x, y * CellSize.y, y * CellSize.z), Quaternion.identity);

                c.WallLeft.SetActive(maze.cells[x, y].WallLeft);
                c.WallBottom.SetActive(maze.cells[x, y].WallBottom);
            }
        }
        hintRenderer = GetComponent<HintRenderer>();

        // hintRenderer.DrawPath();
    }

    private void RemoveWall(MazeGeneratorCell a, MazeGeneratorCell b)
    {
        if (a.X == b.X)
        {
            if (a.Y > b.Y) a.WallBottom = false;
            else b.WallBottom = false;
        }
        else
        {
            if (a.X > b.X) a.WallLeft = false;
            else b.WallLeft = false;
        }
    }

    private Vector2Int PlaceMazeExit()
    {
        MazeGeneratorCell furthest = maze.cells[0, 0];

        for (int x = 0; x < maze.cells.GetLength(0); x++)
        {
            if (maze.cells[x, Height - 2].DistanceFromStart > furthest.DistanceFromStart) furthest = maze.cells[x, Height - 2];
            if (maze.cells[x, 0].DistanceFromStart > furthest.DistanceFromStart) furthest = maze.cells[x, 0];
        }

        for (int y = 0; y < maze.cells.GetLength(1); y++)
        {
            if (maze.cells[Width - 2, y].DistanceFromStart > furthest.DistanceFromStart) furthest = maze.cells[Width - 2, y];
            if (maze.cells[0, y].DistanceFromStart > furthest.DistanceFromStart) furthest = maze.cells[0, y];
        }

        if (furthest.X == 0) furthest.WallLeft = false;
        else if (furthest.Y == 0) furthest.WallBottom = false;
        else if (furthest.X == Width - 2) maze.cells[furthest.X + 1, furthest.Y].WallLeft = false;
        else if (furthest.Y == Height - 2) maze.cells[furthest.X, furthest.Y + 1].WallBottom = false;

        return new Vector2Int(furthest.X, furthest.Y);
    }

    void pause ()
    {

    }
}

