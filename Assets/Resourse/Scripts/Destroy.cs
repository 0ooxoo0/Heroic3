using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    [SerializeField] float timeDestroy = 5;
    void Start()
    {
        Destroy(gameObject, timeDestroy);
    }
}
