using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtFlag : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.LookAt(gameObject.transform.position + GameObject.FindGameObjectWithTag("MainCamera").transform.rotation * Vector3.forward, GameObject.FindGameObjectWithTag("MainCamera").transform.rotation * Vector3.up);
    }
}
