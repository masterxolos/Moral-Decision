using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class tireRotate : MonoBehaviour
{
    private Transform _thisObject;
    // Start is called before the first frame update
    void Start()
    {
        _thisObject = gameObject.transform;
        doRotate();
    }

    private void doRotate()
    {
        _thisObject.DORotate(new Vector3(180,0,0), 0.2f).SetLoops(-1,LoopType.Incremental);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
