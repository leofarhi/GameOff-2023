using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platidroid : Enemy
{
    // Start is called before the first frame update
    void Start()
    {
        InitState();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        UpdateSate();
    }
}
