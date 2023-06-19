using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    public LineRenderer componentLineRenderer;
    // Start is called before the first frame update
    void Start()
    {
        componentLineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Change()
    {
        componentLineRenderer.SetColors(new Color(235, 100, 10), new Color(235, 100, 10));
    }
}
