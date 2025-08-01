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



    /*public static event Action<GameObject> OnCastlePlaced;
    public static event Action OnCastleDestroyed;

    public static GameObject CurrentCastle { get; private set; }

    public static void RegisterCastle(GameObject castle)
    {
        CurrentCastle = castle;
        Debug.Log($"Castle registered at position: {castle.transform.position}");
        OnCastlePlaced?.Invoke(castle);
    }

    public static void UnregisterCastle()
    {
        Debug.Log("Castle unregistered");
        CurrentCastle = null;
        OnCastleDestroyed?.Invoke();
    }

    public static bool HasCastle()
    {
        return CurrentCastle != null;
    }*/
}
