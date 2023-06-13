using UnityEngine;

public class ProtoCell : Cell
{
    public void Start()
    {
        Destroy(this.gameObject, 0.05f);
    }
}