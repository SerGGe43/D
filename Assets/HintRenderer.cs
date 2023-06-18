using System.Collections.Generic;
using UnityEngine;

public class HintRenderer : MonoBehaviour
{
    public MazeGenerator MazeSpawner;

    private LineRenderer componentLineRenderer;

    public Floor FloorPrefab;

    private void Start()
    {
        componentLineRenderer = GetComponent<LineRenderer>();
    }

    public IEnumerator<object> DrawPath()
    {
        componentLineRenderer.SetColors(Color.green, Color.green);
        Maze maze = MazeSpawner.maze;
        int x = maze.finishPosition.x;
        int y = maze.finishPosition.y;
        List<Vector3> positions = new List<Vector3>();

        while ((x != 0 || y != 0))
        {
            positions.Add(new Vector3((x) * MazeSpawner.CellSize.x, (y) * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));

        	// yield return new WaitForSeconds(0.1f);

        	// componentLineRenderer.positionCount = positions.Count;
        	// componentLineRenderer.SetPositions(positions.ToArray());
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
        	yield return new WaitForSeconds(0.02f);
        	lines.Add(positions[i]);

	        componentLineRenderer.positionCount = lines.Count;
	        componentLineRenderer.SetPositions(lines.ToArray());
        }

        // positions.Add(Vector3.zero);
        // componentLineRenderer.positionCount = positions.Count;
        // componentLineRenderer.SetPositions(positions.ToArray());
    }

    public IEnumerator<object> Lee()
    {
        componentLineRenderer.SetColors(Color.red, Color.red);
        Maze maze = MazeSpawner.maze;
        int width = MazeSpawner.Width;
        int height = MazeSpawner.Height;
        int ax = 0;
        int ay = 0;
        int bx = maze.finishPosition.x;
        int by = maze.finishPosition.y;
        bool[,] visited = new bool[width-1, height-1];
        //int[] dx = { 1, 0, -1, 0 };   
        //int[] dy = { 0, 1, 0, -1 };
        for (int i = 0; i < width-1; i++)
        {
            for (int j = 0; j < height - 1; j++) visited[i, j] = false;
        }
        int d, x, y, k, len;
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> positions1 = new List<Vector3>();
        bool stop = true;
        d = 0;
        //int[,] distance = new int[width,height];
        maze.cells[ax,ay].DistanceFromStart = 0;
        Debug.Log("Do");
        do
        {
            Debug.Log("Cycle");
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
        Debug.Log("Done");
        len = maze.cells[bx, by].DistanceFromStart;
        d = 0;
        do
        {
            for (x = 0; x < width - 1; x++)
            {
                for (y = 0; y < height - 1; y++)
                {
                    if (maze.cells[x, y].DistanceFromStart == d)
                    {
                        Debug.Log(x);
                        Debug.Log(y);
                        Floor c = Instantiate(FloorPrefab, new Vector3(x * MazeSpawner.CellSize.x, y * MazeSpawner.CellSize.y, y * MazeSpawner.CellSize.z), Quaternion.identity);
                        //c.floor.SetActive(true);
                        yield return new WaitForSeconds(0.02f);
                    }
                }
            }
            d++;
        } while (d < len);
        x = bx;
        y = by;
        d = len;
        Debug.Log("While");
        while (d > 0)
        {
            Debug.Log("Cycle2");
            positions.Add(new Vector3((x) * MazeSpawner.CellSize.x, (y) * MazeSpawner.CellSize.y, MazeSpawner.CellSize.z));
            componentLineRenderer.positionCount = positions.Count;
            componentLineRenderer.SetPositions(positions.ToArray());
            yield return new WaitForSeconds(0.02f);
            d--;
            for (k = 0; k < 4; ++k)
            {
                int iy, ix;
                if (k == 0)
                {
                    iy = y + 0;
                    ix = x + 1;
                    if (iy >= 0 && iy < height-1 && ix >= 0 && ix < width-1 && maze.cells[ix, iy].DistanceFromStart == d)
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
                    if (iy >= 0 && iy < height-1 && ix >= 0 && ix < width-1 && maze.cells[ix, iy].DistanceFromStart == d)
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
                    if (iy >= 0 && iy < height-1 && ix >= 0 && ix < width-1 && maze.cells[ix, iy].DistanceFromStart == d)
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
                    if (iy >= 0 && iy < height-1 && ix >= 0 && ix < width-1 && maze.cells[ix, iy].DistanceFromStart == d)
                    {
                        x += 0;
                        y += -1;
                        break;
                    }
                }
            }
        }
        //positions.Clear();
        Debug.Log("NeWhile");
        positions.Add(Vector3.zero);
        componentLineRenderer.positionCount = positions.Count;
        componentLineRenderer.SetPositions(positions.ToArray());
        positions.Clear();
    }
}
