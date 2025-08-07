using UnityEngine;
using System;

public class CastleManager : MonoBehaviour
{
    public static CastleManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject Castle;



   
}
