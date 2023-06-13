using UnityEngine;

public class ProtoCell : MonoBehaviour
{
    public GameObject WallLeft;
    public GameObject WallBottom;

    public void Start()
    {
        Destroy(this.gameObject, 0.04f);
    }
}