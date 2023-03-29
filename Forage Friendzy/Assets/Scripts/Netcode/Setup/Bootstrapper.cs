using UnityEngine;



//example uses this to reset their transport in their matchmaking script
//don't know what happens if I don't, so included it
public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Matchmaking.Reset();
    }
}