using UnityEngine;

public class Cell : MonoBehaviour
{
    public GameObject WallLeft;
    public GameObject WallBottom;

    public void Start()
    {
        Destroy(this.gameObject, 0.1f);
    }
}