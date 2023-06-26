using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using UnityEngine;
using System.Diagnostics;
using System.IO;

public class MazeGenerator : MonoBehaviour
{
    public Cell CellPrefab;
    public Vector3 CellSize = new Vector3(1, 1, 0);
    public int Width = 10;
    public int Height = 10;
    public Maze maze;
    public List<List<Cell>> cells;
    public static float MazeRenderTimeout = 0.01f;
    public bool Cycles = false;
    public bool Eller = false;

    public bool ReadyToSolve = false;
    public HintRenderer hintRenderer;
    public HintRenderer hintRenderer1;
    public HintRenderer hintRenderer2;

    private void Start()
    {
        if (!Eller)
        {
        	StartCoroutine(GenerateMaze());
        }
        else
        {
        	StartCoroutine(EllerGenerate());
        }
    }

    private void Update()
    {
        if (ReadyToSolve && Input.GetKey(KeyCode.H))
        {
            hintRenderer.Clear();
            hintRenderer1.Clear();
            hintRenderer2.Clear();
            StartCoroutine(hintRenderer.DrawPath(maze.finishPosition[0].x, maze.finishPosition[0].y));
            StartCoroutine(hintRenderer1.DrawPath(maze.finishPosition[1].x, maze.finishPosition[1].y));
            StartCoroutine(hintRenderer2.DrawPath(maze.finishPosition[2].x, maze.finishPosition[2].y));
        }
        if (ReadyToSolve && Input.GetKey(KeyCode.L))
        {
            hintRenderer.Clear();
            hintRenderer1.Clear();
            hintRenderer2.Clear();
            StartCoroutine(hintRenderer.Lee0(maze.finishPosition[0].x, maze.finishPosition[0].y));
            StartCoroutine(hintRenderer1.Lee0(maze.finishPosition[1].x, maze.finishPosition[1].y));
            StartCoroutine(hintRenderer2.Lee0(maze.finishPosition[2].x, maze.finishPosition[2].y));
        }
        if (ReadyToSolve && Input.GetKey(KeyCode.K))
        {
            StartCoroutine(hintRenderer.WallPath(maze.finishPosition[0].x, maze.finishPosition[0].y));
            StartCoroutine(hintRenderer1.WallPath(maze.finishPosition[1].x, maze.finishPosition[1].y));
            StartCoroutine(hintRenderer2.WallPath(maze.finishPosition[2].x, maze.finishPosition[2].y));
            hintRenderer.Clear();
            hintRenderer1.Clear();
            hintRenderer2.Clear();
        }
    }

    public Maze CreateMaze()
    {
        MazeGeneratorCell[,] Cells = new MazeGeneratorCell[Width, Height];

        for (int x = 0; x < Cells.GetLength(0); x++)
        {
            for (int y = 0; y < Cells.GetLength(1); y++)
            {
                Cells[x, y] = new MazeGeneratorCell {X = x, Y = y};
                Cells[x, y].Set = x + y * (Width - 1);
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

    IEnumerator GenerateMaze()
    {
        maze = CreateMaze();

        cells = new List<List<Cell>>();

        for (int x = 0; x < maze.cells.GetLength(0); x++)
        {
        	cells.Add(new List<Cell>());
            for (int y = 0; y < maze.cells.GetLength(1); y++)
            {
                Cell c = Instantiate(CellPrefab, new Vector3(x * CellSize.x, y * CellSize.y, y * CellSize.z), Quaternion.identity);

                c.WallLeft.SetActive(maze.cells[x, y].WallLeft);
                c.WallBottom.SetActive(maze.cells[x, y].WallBottom);
                cells[x].Add(c);
            }
        }

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
                RemoveWall(current, chosen);
                chosen.Visited = true;
                stack.Push(chosen);
                chosen.DistanceFromStart = current.DistanceFromStart + 1;
                current = chosen;
                yield return new WaitForSeconds(MazeRenderTimeout);
            }
            else
            {
                current = stack.Pop();
            }
        } while (stack.Count > 0);

        if (Cycles) PlaceCycles();
        CountDistance();

        maze.finishPosition = PlaceMazeExit();

        yield return new WaitForSeconds(MazeRenderTimeout);
        ReadyToSolve = true;
    }

    private void RemoveWall(MazeGeneratorCell a, MazeGeneratorCell b)
    {
        if (a.X == b.X)
        {
            if (a.Y > b.Y)
            {
            	a.WallBottom = false;
            	cells[a.X][a.Y].WallBottom.SetActive(false);
            }
            else
            {
            	b.WallBottom = false;
            	cells[b.X][b.Y].WallBottom.SetActive(false);
            }
        }
        else
        {
            if (a.X > b.X)
            {
            	a.WallLeft = false;
            	cells[a.X][a.Y].WallLeft.SetActive(false);
            }
            else
            {
            	b.WallLeft = false;
            	cells[b.X][b.Y].WallLeft.SetActive(false);
            }
        }
    }

    private void PlaceCycles()
    {
        for (int i = 0; i < Width / 3;)
        {
            int x = UnityEngine.Random.Range(2, Width - 2);
            int y = UnityEngine.Random.Range(2, Height - 2);
            if (maze.cells[x, y].WallBottom)
            {
            	i++;
                RemoveWall(new MazeGeneratorCell {X = x, Y = y}, new MazeGeneratorCell {X = x, Y = y - 1});
            	maze.cells[x, y].WallBottom = false;
            }
        }
    }

    private List<Vector2Int> PlaceMazeExit()
    {
        List<Vector2Int> finish = new List<Vector2Int>();
        Vector2Int finishPos = new Vector2Int();
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

        if (furthest.X == 0)
        {
        	furthest.WallLeft = false;
        	cells[furthest.X][furthest.Y].WallLeft.SetActive(false);
        }
        else if (furthest.Y == 0)
        {
        	furthest.WallBottom = false;
        	cells[furthest.X][furthest.Y].WallBottom.SetActive(false);
        }
        else if (furthest.X == Width - 2)
        {
        	maze.cells[furthest.X + 1, furthest.Y].WallLeft = false;
        	cells[furthest.X + 1][furthest.Y].WallLeft.SetActive(false);
        }
        else if (furthest.Y == Height - 2)
        {
        	maze.cells[furthest.X, furthest.Y + 1].WallBottom = false;
        	cells[furthest.X][furthest.Y + 1].WallBottom.SetActive(false);
        }
        finishPos.Set(furthest.X, furthest.Y);
        finish.Add(finishPos);
        MazeGeneratorCell finishCell;
        if (furthest.Y == Height - 2)
        {
            finishPos.Set(0, UnityEngine.Random.Range(3, Height - 3));
            finish.Add(finishPos);
            finishCell = maze.cells[finishPos[0], finishPos[1]];
            finishCell.WallLeft = false;
            cells[finishPos[0]][finishPos[1]].WallLeft.SetActive(false);
            
            finishPos.Set(Width - 2, UnityEngine.Random.Range(0, Height - 3));
            finish.Add(finishPos);
            finishCell = maze.cells[finishPos[0] + 1, finishPos[1]];
            finishCell.WallLeft = false;
            cells[finishPos[0] + 1][finishPos[1]].WallLeft.SetActive(false);
        }
        else if (furthest.Y == 0)
        {
            finishPos.Set(UnityEngine.Random.Range(0, Width - 3), Height - 2);
            finish.Add(finishPos);
            finishCell = maze.cells[finishPos[0], finishPos[1] + 1];
            finishCell.WallBottom = false;
            cells[finishPos[0]][finishPos[1] + 1].WallBottom.SetActive(false);
            
            finishPos.Set(Width - 2, UnityEngine.Random.Range(1, Height - 3));
            finish.Add(finishPos);
            finishCell = maze.cells[finishPos[0] + 1, finishPos[1]];
            finishCell.WallLeft = false;
            cells[finishPos[0] + 1][finishPos[1]].WallLeft.SetActive(false);
        }
        else if (furthest.X == Width - 2)
        {
            finishPos.Set(0, UnityEngine.Random.Range(3, Height - 3));
            finish.Add(finishPos);
            finishCell = maze.cells[finishPos[0], finishPos[1]];
            finishCell.WallLeft = false;
            cells[finishPos[0]][finishPos[1]].WallLeft.SetActive(false);
            
            finishPos.Set(UnityEngine.Random.Range(3, Width - 3), 0);
            finish.Add(finishPos);
            finishCell = maze.cells[finishPos[0], finishPos[1]];
            finishCell.WallBottom = false;
            cells[finishPos[0]][finishPos[1]].WallBottom.SetActive(false);
        }
        else if (furthest.X == 0)
        {
            finishPos.Set(UnityEngine.Random.Range(0, Height - 3), Height - 2);
            finish.Add(finishPos);
            finishCell = maze.cells[finishPos[0], finishPos[1] + 1];
            finishCell.WallBottom = false;
            cells[finishPos[0]][finishPos[1] + 1].WallBottom.SetActive(false);
            
            finishPos.Set(Width - 2, UnityEngine.Random.Range(0, Height - 3));
            finish.Add(finishPos);
            finishCell = maze.cells[finishPos[0] + 1, finishPos[1]];
            finishCell.WallLeft = false;
            cells[finishPos[0] + 1][finishPos[1]].WallLeft.SetActive(false);
        }
        return finish;
    }

    private void CountDistance()
    {
    	for (int x = 0; x < Width - 1; x++)
    	{
    		for (int y = 0; y < Height - 1; y++)
    		{
    			maze.cells[x, y].Visited = false;
    			maze.cells[x, y].DistanceFromStart = 1000;
    		}
    	}

    	MazeGeneratorCell current = maze.cells[0, 0];
        current.Visited = true;
        current.DistanceFromStart = 0;

        Stack<MazeGeneratorCell> stack = new Stack<MazeGeneratorCell>();
        do
        {
            List<MazeGeneratorCell> unvisitedNeighbours = new List<MazeGeneratorCell>();

            int x = current.X;
            int y = current.Y;

            if (x > 0 && !maze.cells[x - 1, y].Visited && !current.WallLeft) unvisitedNeighbours.Add(maze.cells[x - 1, y]);
            if (y > 0 && !maze.cells[x, y - 1].Visited && !current.WallBottom) unvisitedNeighbours.Add(maze.cells[x, y - 1]);
            if (x < Width - 2 && !maze.cells[x + 1, y].Visited && !maze.cells[x + 1, y].WallLeft) unvisitedNeighbours.Add(maze.cells[x + 1, y]);
            if (y < Height - 2 && !maze.cells[x, y + 1].Visited && !maze.cells[x, y + 1].WallBottom) unvisitedNeighbours.Add(maze.cells[x, y + 1]);

            if (unvisitedNeighbours.Count > 0)
            {
                MazeGeneratorCell chosen = unvisitedNeighbours[UnityEngine.Random.Range(0, unvisitedNeighbours.Count)];
                chosen.Visited = true;
                stack.Push(chosen);
                chosen.DistanceFromStart = current.DistanceFromStart + 1;
                current = chosen;
            }
            else
            {
            	current = stack.Pop();
            }
        } while (stack.Count > 0);
    }

    public IEnumerator EllerGenerate()
    {
    	maze = CreateMaze();

    	cells = new List<List<Cell>>();

        for (int x = 0; x < maze.cells.GetLength(0); x++)
        {
        	cells.Add(new List<Cell>());
            for (int y = 0; y < maze.cells.GetLength(1); y++)
            {
                Cell c = Instantiate(CellPrefab, new Vector3(x * CellSize.x, y * CellSize.y, y * CellSize.z), Quaternion.identity);

                c.WallLeft.SetActive(maze.cells[x, y].WallLeft);
                c.WallBottom.SetActive(maze.cells[x, y].WallBottom);
                cells[x].Add(c);
            }
        }

    	for (int i = 0; i < Height - 2; i++)
    	{
    		CreateRow(i);
    		yield return new WaitForSeconds(MazeRenderTimeout);

    		CreateVerticalConnection(i);
    		yield return new WaitForSeconds(MazeRenderTimeout);
    	}
    	CreateLastRow();

     	if (Cycles) PlaceCycles();
		CountDistance();
    	PlaceMazeExit();

    	ReadyToSolve = true;
    }
    
    private void ReplaceSetInRow(int oldSet, int newSet, int rowNum)
    {
        for(int i = 0; i < Width - 1; i++)
        {
            MazeGeneratorCell cell = maze.cells[i, rowNum];
            if (cell.Set == oldSet)
            {
                cell.Set = newSet;
            }
        }
    }

    private void CreateRow(int rowNum)
    {
    	for (int i = 0; i < Width - 2; i++)
    	{
    		MazeGeneratorCell cell, next;
    		cell = maze.cells[i, rowNum];
    		next = maze.cells[i + 1, rowNum];

    		if (cell.Set != next.Set)
    		{
    			if (UnityEngine.Random.Range(0, 2) > 0)
    			{
    				RemoveWall(cell, next);

	    			if (cell.Set < next.Set)
	    			{
	    				ReplaceSetInRow(next.Set, cell.Set, rowNum);
	    			}
	    			else
	    			{
	    				ReplaceSetInRow(cell.Set, next.Set, rowNum);
	    			}
    			}
    		}
    	}
    }

    private void CreateVerticalConnection(int rowNum)
    {
    	bool isAddedVertical = false;

    	for (int i = 0; i < Width - 2; i++)
    	{
    		MazeGeneratorCell cell, next, top;
    		cell = maze.cells[i, rowNum];
    		next = maze.cells[i + 1, rowNum];
    		top = maze.cells[i, rowNum + 1];

    		if (cell.Set != next.Set)
    		{
    			if (!isAddedVertical)
    			{
    				RemoveWall(cell, top);
    				top.Set = cell.Set;
    			}
    			isAddedVertical = false;
    		}
    		else
    		{
    			if (UnityEngine.Random.Range(0, 2) > 0)
    			{
    				RemoveWall(cell, top);
    				top.Set = cell.Set;
    				isAddedVertical = true;
    			}
    		}
    	}

    	CheckLastVertical(rowNum, isAddedVertical);
    }

    private void CheckLastVertical(int rowNum, bool isAddedVertical)
    {
    	MazeGeneratorCell last, prelast, top;
    	last = maze.cells[Width - 1 - 1, rowNum];
    	prelast = maze.cells[Width - 2 - 1, rowNum];
    	top = maze.cells[Width - 1 - 1, rowNum + 1];

    	if (last.Set != prelast.Set)
    	{
    		RemoveWall(last, top);
    		top.Set = last.Set;
    	}
    	else
    	{
    		if (isAddedVertical ? UnityEngine.Random.Range(0, 2) > 0 : true)
    		{
    			RemoveWall(last, top);
    			top.Set = last.Set;
    		}
    	}
    }

    private void CreateLastRow()
    {
    	int y = Height - 2;

    	for (int i = 0; i < Width - 2; i++)
    	{
    		MazeGeneratorCell cell, next;
    		cell = maze.cells[i, y];
    		next = maze.cells[i + 1, y];

    		if (cell.Set != next.Set)
    		{
    			RemoveWall(cell, next);

    			if (cell.Set < next.Set)
    			{
    				ReplaceSetInRow(next.Set, cell.Set, y);
    			}
    			else
    			{
    				ReplaceSetInRow(cell.Set, next.Set, y);
    			}
    		}
    	}
    }
}

