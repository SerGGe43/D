using UnityEngine;

public class ProtoCell : MonoBehaviour
{
    public GameObject WallLeft;
    public GameObject WallBottom;

    public void Start()
    {
        Destroy(this.gameObject, MazeGenerator.RenderPause * 1.7f);
    }
}