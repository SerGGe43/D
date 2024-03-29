﻿using System.Collections.Generic;
using UnityEngine;

public class HintRenderer : MonoBehaviour
{
    public MazeGenerator MazeSpawner;

    private LineRenderer componentLineRenderer;

    public Floor FloorPrefab;

    public List<List<Floor>> floors;

    public static float HintRenderTimeout = 0.1f;

    private void Start()
    {
        componentLineRenderer = GetComponent<LineRenderer>();
    }

    public void Clear() {
        componentLineRenderer.positionCount = 0;
        componentLineRenderer.SetPositions(new List<Vector3>().ToArray());
    }

    public IEnumerator<object> DrawPath(int x, int y)
    {
        Maze maze = MazeSpawner.maze;
        List<Vector3> positions = new List<Vector3>();

        while ((x != 0 || y != 0))
        {
            positions.Add(new Vector3((x) * MazeSpawner.CellSize.x, (y) * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));

            MazeGeneratorCell currentCell = maze.cells[x, y];

            if (x > 0 &&
                !currentCell.WallLeft &&
                maze.cells[x - 1, y].DistanceFromStart < currentCell.DistanceFromStart)
            {
                x--;
            }
            else if (y > 0 &&
                !currentCell.WallBottom &&
                maze.cells[x, y - 1].DistanceFromStart < currentCell.DistanceFromStart)
            {
                y--;
            }
            else if (x < maze.cells.GetLength(0) - 1 &&
                !maze.cells[x + 1, y].WallLeft &&
                maze.cells[x + 1, y].DistanceFromStart < currentCell.DistanceFromStart)
            {
                x++;
            }
            else if (y < maze.cells.GetLength(1) - 1 &&
                !maze.cells[x, y + 1].WallBottom &&
                maze.cells[x, y + 1].DistanceFromStart < currentCell.DistanceFromStart)
            {
                y++;
            }
        }

        List<Vector3> lines = new List<Vector3>();
        lines.Add(Vector3.zero);

        for (int i = positions.Count - 1; i >= 0; i--)
        {
        	yield return new WaitForSeconds(HintRenderTimeout);
        	lines.Add(positions[i]);

	        componentLineRenderer.positionCount = lines.Count;
	        componentLineRenderer.SetPositions(lines.ToArray());
        }
    }

    public IEnumerator<object> Lee0(int bx, int by)
    {
        MazeSpawner.ReadyToSolve = false;
        Maze maze = MazeSpawner.maze;
        int width = MazeSpawner.Width;
        int height = MazeSpawner.Height;
        int ax = 0;
        int ay = 0;

        bool[,] visited = new bool[width-1, height-1];

        for (int i = 0; i < width-1; i++)
        {
            for (int j = 0; j < height - 1; j++) visited[i, j] = false;
        }
        int d, x, y, k, len;
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> positions1 = new List<Vector3>();
        bool stop = true;
        d = 0;

        maze.cells[ax,ay].DistanceFromStart = 0;
        do
        {
            stop = true;
            for (x = 0; x < width-1; x++)
            {
                for (y = 0; y < height-1; y++)
                {
                    if (maze.cells[x,y].DistanceFromStart == d)
                    {
                        visited[x, y] = true;
                        for (k = 0; k < 4; k++)
                        {
                            int iy, ix;
                            if (k == 0) 
                            {
                                iy = y + 0;
                                ix = x + 1;
                                if (iy >= 0 && iy < height-1 && ix >= 0 && ix < width-1 && !maze.cells[ix, iy].WallLeft && !visited[ix,iy])
                                {
                                    stop = false;
                                    maze.cells[ix, iy].DistanceFromStart = d + 1;      
                                }
                            }
                            else if (k == 1)
                            {
                                iy = y + 1;
                                ix = x + 0;
                                if (iy >= 0 && iy < height-1 && ix >= 0 && ix < width-1 && !maze.cells[ix, iy].WallBottom && !visited[ix, iy])
                                {
                                    stop = false;
                                    maze.cells[ix, iy].DistanceFromStart = d + 1;      
                                }
                            }
                            else if (k == 2)
                            {
                                iy = y + 0;
                                ix = x - 1;
                                if (iy >= 0 && iy < height-1 && ix >= 0 && ix < width-1 && !maze.cells[x, y].WallLeft && !visited[ix, iy])
                                {
                                    stop = false;
                                    maze.cells[ix, iy].DistanceFromStart = d + 1;
                                }
                            }
                            else
                            {
                                iy = y - 1;
                                ix = x + 0;
                                if (iy >= 0 && iy < height-1 && ix >= 0 && ix < width-1 && !maze.cells[x, y].WallBottom && !visited[ix, iy])
                                {
                                    stop = false;
                                    maze.cells[ix, iy].DistanceFromStart = d + 1;
                                }
                            }
                        }
                    }
                }
            }
            d++;
        } while (!stop);
        len = maze.cells[bx, by].DistanceFromStart;

	    d = 0;
        floors = new List<List<Floor>>();
        for (x = 0; x < width - 1; x++)
        {
            floors.Add(new List<Floor>());
            for (y = 0; y < height - 1; y++)
            {
                Floor f = Instantiate(FloorPrefab, new Vector3(x * MazeSpawner.CellSize.x, y * MazeSpawner.CellSize.y, y * MazeSpawner.CellSize.z), Quaternion.identity);
                f.floor.SetActive(false);
                floors[x].Add(f);

            }
        }
        yield return new WaitForSeconds(0.5f);
        while(d < len)
        {
            for (x = 0; x < width - 1; x++)
            {
                for (y = 0; y < height - 1; y++)
                {
                    if (maze.cells[x, y].DistanceFromStart == d)
                    {
                        floors[x][y].floor.SetActive(true);
                        LineRenderer FloorColor;
                        FloorColor = floors[x][y].floor.GetComponent<LineRenderer>();
                        
                            FloorColor.SetColors(new Color(1 - d * 0.01f, d * 0.001f, d * 0.01f), 
                                new Color(1 - d * 0.01f, d * 0.001f, d * 0.01f));
                    }
                }
            }
            yield return new WaitForSeconds(HintRenderTimeout);
            d++;
        }
        d = len;

	    for (x = width - 2; x > -1; x--)
        {
            for (y = height - 2; y > -1; y--)
            {
                floors[x][y].floor.SetActive(false);
            }
        }


        x = bx;
        y = by;
        if (!MazeSpawner.cells[x][y + 1].WallBottom)
        {
            positions.Add(new Vector3((x) * MazeSpawner.CellSize.x, (y + 1) * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));
            componentLineRenderer.positionCount = positions.Count;
            componentLineRenderer.SetPositions(positions.ToArray());
        }
        else if (!MazeSpawner.cells[x + 1][y].WallLeft)
        {
            positions.Add(new Vector3((x + 1) * MazeSpawner.CellSize.x, (y) * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));
            componentLineRenderer.positionCount = positions.Count;
            componentLineRenderer.SetPositions(positions.ToArray());
        }
        else if (!MazeSpawner.cells[x][y].WallBottom && y == 0)
        {
            positions.Add(new Vector3((x) * MazeSpawner.CellSize.x, (y-1) * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));
            componentLineRenderer.positionCount = positions.Count;
            componentLineRenderer.SetPositions(positions.ToArray());
        }
        else if(!MazeSpawner.cells[x][y].WallLeft && x == 0)
        {
            positions.Add(new Vector3((x - 1) * MazeSpawner.CellSize.x, (y) * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));
            componentLineRenderer.positionCount = positions.Count;
            componentLineRenderer.SetPositions(positions.ToArray());
        }
        while (d > 0)
        {
            positions.Add(new Vector3((x) * MazeSpawner.CellSize.x, (y) * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));
            componentLineRenderer.positionCount = positions.Count;
            componentLineRenderer.SetPositions(positions.ToArray());
            yield return new WaitForSeconds(HintRenderTimeout);
            d--;
            for (k = 0; k < 4; ++k)
            {
                int iy, ix;
                if (k == 0)
                {
                    iy = y + 0;
                    ix = x + 1;
                    if (iy >= 0 && iy < height-1 && ix >= 0 && ix < width-1 && maze.cells[ix, iy].DistanceFromStart == d && !maze.cells[ix, iy].WallLeft)
                    {
                        x += 1;
                        y += 0;
                        break;
                    }
                }
                if (k == 1)
                {
                    iy = y + 1;
                    ix = x + 0;
                    if (iy >= 0 && iy < height-1 && ix >= 0 && ix < width-1 && maze.cells[ix, iy].DistanceFromStart == d && !maze.cells[ix, iy].WallBottom)
                    {
                        x += 0;
                        y += 1;
                        break;
                    }
                }
                if (k == 2)
                {
                    iy = y + 0;
                    ix = x - 1;
                    if (iy >= 0 && iy < height-1 && ix >= 0 && ix < width-1 && maze.cells[ix, iy].DistanceFromStart == d && !maze.cells[x, y].WallLeft)
                    {
                        x += -1;
                        y += 0;
                        break;
                    }
                }
                if (k == 3)
                {
                    iy = y - 1;
                    ix = x + 0;
                    if (iy >= 0 && iy < height-1 && ix >= 0 && ix < width-1 && maze.cells[ix, iy].DistanceFromStart == d && !maze.cells[x, y].WallBottom)
                    {
                        x += 0;
                        y += -1;
                        break;
                    }
                }
            }
        }
        floors[0][0].floor.SetActive(false);
        positions.Add(Vector3.zero);
        componentLineRenderer.positionCount = positions.Count;
        componentLineRenderer.SetPositions(positions.ToArray());
        positions.Clear();

        MazeSpawner.ReadyToSolve = true;
    }

    public IEnumerator<object> WallPath(int bx, int by)
    {
        MazeSpawner.ReadyToSolve = false;
        List<Vector3> positions = new List<Vector3>();
        int x = 0;
        int y = 0;
        long wall = 0;
        positions.Add(Vector3.zero);
        componentLineRenderer.positionCount = positions.Count;
        componentLineRenderer.SetPositions(positions.ToArray());
        while (x != bx || y != by)
        { 

            while (wall == 0) // вверх
            {
                if (MazeSpawner.maze.cells[x + 1, y].WallLeft)
                {


                    if (!MazeSpawner.maze.cells[x, y + 1].WallBottom && y + 1 < MazeSpawner.Height - 1)
                    {
                        y++;
                        positions.Add(new Vector3((x) * MazeSpawner.CellSize.x, y * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));
                        componentLineRenderer.positionCount = positions.Count;
                        componentLineRenderer.SetPositions(positions.ToArray());
                        yield return new WaitForSeconds(0.05f);
                    }
                    if ((MazeSpawner.maze.cells[x, y + 1].WallBottom || y + 1 >= MazeSpawner.Height - 1) && (MazeSpawner.maze.cells[x + 1, y].WallLeft || x + 1 >= MazeSpawner.Width - 1))
                    {
                        wall = 3;
                    }

                }
                else if ((x + 1) < MazeSpawner.Width - 1)
                {
                    x++;
                    positions.Add(new Vector3((x) * MazeSpawner.CellSize.x, y * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));
                    componentLineRenderer.positionCount = positions.Count;
                    componentLineRenderer.SetPositions(positions.ToArray());
                    yield return new WaitForSeconds(0.05f);
                    wall = 1;
                }
                else wall = 3;
              
            }

            if (x == bx && y == by) break;

            while (wall == 1) // вправо
            {
                if (MazeSpawner.maze.cells[x, y].WallBottom)
                {


                    if (!MazeSpawner.maze.cells[x + 1, y].WallLeft && (x + 1) < MazeSpawner.Width - 1)
                    {
                        x++;
                        positions.Add(new Vector3((x) * MazeSpawner.CellSize.x, y * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));
                        componentLineRenderer.positionCount = positions.Count;
                        componentLineRenderer.SetPositions(positions.ToArray());
                        yield return new WaitForSeconds(0.05f);
                    }
                    if ((MazeSpawner.maze.cells[x, y].WallBottom || y - 1 < 0) && (MazeSpawner.maze.cells[x + 1, y].WallLeft || x + 1 >= MazeSpawner.Width - 1))
                    {
                        wall = 0;
                    }
                }
                else if (y > 0)
                {
                    wall = 2;
                    y--;
                    positions.Add(new Vector3((x) * MazeSpawner.CellSize.x, y * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));
                    componentLineRenderer.positionCount = positions.Count;
                    componentLineRenderer.SetPositions(positions.ToArray());
                    yield return new WaitForSeconds(0.05f);
                }
                else wall = 0;
            }
            if (x == bx && y == by) break;
            while (wall == 2) // вниз
            {
                if (MazeSpawner.maze.cells[x, y].WallLeft)
                {
                    if (!MazeSpawner.maze.cells[x, y].WallBottom && (y - 1) > -1)
                    {
                        y--;
                        positions.Add(new Vector3((x) * MazeSpawner.CellSize.x, y * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));
                        componentLineRenderer.positionCount = positions.Count;
                        componentLineRenderer.SetPositions(positions.ToArray());
                        yield return new WaitForSeconds(0.05f);
                    }
                    if ((MazeSpawner.maze.cells[x, y].WallBottom || y - 1 < 0) && (MazeSpawner.maze.cells[x, y].WallLeft || x - 1 < 0))
                    {
                        wall = 1;
                    }
                }
                else if (x > 0)
                {
                    x--;
                    positions.Add(new Vector3((x) * MazeSpawner.CellSize.x, y * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));
                    componentLineRenderer.positionCount = positions.Count;
                    componentLineRenderer.SetPositions(positions.ToArray());
                    yield return new WaitForSeconds(0.05f);
                    wall = 3;
                }
                else wall = 1;
               



            }
            if (x == bx && y == by) break;
            while (wall == 3) // влево
            {
                if (MazeSpawner.maze.cells[x, y + 1].WallBottom)
                {


                    if (!MazeSpawner.maze.cells[x, y].WallLeft && (x - 1) > -1)
                    {
                        x--;
                        positions.Add(new Vector3((x) * MazeSpawner.CellSize.x, y * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));
                        componentLineRenderer.positionCount = positions.Count;
                        componentLineRenderer.SetPositions(positions.ToArray());
                        yield return new WaitForSeconds(0.05f);
                    }
                    if ((MazeSpawner.maze.cells[x, y + 1].WallBottom || y + 1 >= MazeSpawner.Height - 1) && (MazeSpawner.maze.cells[x, y].WallLeft || x - 1 < 0))
                    {
                        wall = 2;
                    }
                }
                else if ((y + 1) < MazeSpawner.Height - 1)
                {
                    y++;
                    positions.Add(new Vector3((x) * MazeSpawner.CellSize.x, y * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));
                    componentLineRenderer.positionCount = positions.Count;
                    componentLineRenderer.SetPositions(positions.ToArray());
                    yield return new WaitForSeconds(0.05f);
                    wall = 0;
                }
                else wall = 2;
            }
            if (x == bx && y == by) break;
            yield return null;

        }
        componentLineRenderer.positionCount = positions.Count;
        componentLineRenderer.SetPositions(positions.ToArray());
        yield return null;
        MazeSpawner.ReadyToSolve = true;
    }
}
