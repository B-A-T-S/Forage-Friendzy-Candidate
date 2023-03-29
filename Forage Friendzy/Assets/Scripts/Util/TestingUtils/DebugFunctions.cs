using System.Collections;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class DebugFunctions : MonoBehaviour
{

    [Header("GUI Bounds")]
    public float widthOffset = 120;
    public float heightOffset = 120;
    public float GUIWidth = 100;
    public float GUIHeight = 100;

    [Header("Available Debug Buttons")]

    [Tooltip("Toggles all Prey's XRay isVisible value")]
    public bool toggleGlobalPreyXRay;

    [Tooltip("Toggles all Predator's XRay isVisible value")]
    public bool toggleGlobalPredatorXRay;

    [Tooltip("Toggles the client's personal canSeePrey bool")]
    public bool togglePersonalPreyXRayCamera;

    [Tooltip("Toggles the client's personal canSeePredator bool")]
    public bool togglePersonalPredatorXRayCamera;

    [Tooltip("Initiates a Scurry Process at current local position")]
    public bool exitGame;
    private void OnGUI()
    {


        GUILayout.BeginArea(new Rect(Screen.width - widthOffset, Screen.height - heightOffset, GUIWidth, GUIHeight));
        
        if(toggleGlobalPreyXRay)
            if(GUILayout.Button("All Prey X-Ray Geo"))
            {

                GameObject[] preysInGame = GameManager.Instance.preyTeam.Values.ToArray();

                Debug.Log($"[DEBUG] {preysInGame.Length} Prey X-Rays have been Toggled");

                foreach (GameObject prey in preysInGame)
                {
                    Controlled3DBody prey3DBody = prey.GetComponent<Controlled3DBody>();
                    prey3DBody.ToggleXRayGeometry(!prey3DBody.XRayIsVisible());
                }
            }

        if(toggleGlobalPredatorXRay) 
            if(GUILayout.Button("All Pred X-Ray Geo"))
            {
                GameObject[] predsInGame = GameManager.Instance.predatorTeam.Values.ToArray();

                Debug.Log($"[DEBUG] {predsInGame.Length} Predator X-Rays have been Toggled");
                foreach (GameObject pred in predsInGame)
                {
                    Controlled3DBody pred3DBody = pred.GetComponent<Controlled3DBody>();
                    pred3DBody.ToggleXRayGeometry(!pred3DBody.XRayIsVisible());
                }
            }

        if (togglePersonalPreyXRayCamera)
            if (GUILayout.Button("Local Prey X-Ray Cam"))
            {
                Controlled3DBody local3DBody = GetLocal3DBody();
                if (local3DBody.NetworkObject.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                    local3DBody.ToggleXRayCameras(!local3DBody.XRayCanSeePrey(), local3DBody.XRayCanSeePred());

            }

        if (togglePersonalPredatorXRayCamera)
            if (GUILayout.Button("Local Pred X-Ray Cam"))
            {
                Controlled3DBody local3DBody = GetLocal3DBody();
                if (local3DBody.NetworkObject.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                    local3DBody.ToggleXRayCameras(local3DBody.XRayCanSeePrey(), !local3DBody.XRayCanSeePred());

            }

        if(GUILayout.Button("Hit Yourself!"))
        {
            GetComponentOnLocalPlayer<PreyHealth>().HandleAttack(1);
        }

        //restrict commands to host
        if (!NetworkManager.Singleton.IsServer)
        {
            GUILayout.EndArea();
            return;
        }
            

        if (GUILayout.Button("Predator Win"))
            GameManager.Instance.PredatorWin();

        if (GUILayout.Button("Prey Win"))
            GameManager.Instance.PreyWin();

        if (exitGame)
            if(GUILayout.Button("Exit Game"))
                GameManager.Instance.TryExitMatch();
            

        GUILayout.EndArea();
    }
    
    private GameObject GetLocalPlayer()
    {
        return GameManager.Instance.localPlayer;
    }
    
    private Controlled3DBody GetLocal3DBody()
    {
        GameObject localPlayer = GameManager.Instance.localPlayer;
        return localPlayer.GetComponent<Controlled3DBody>();
    }

    private BodyMovement GetLocalBodyMovement()
    {
        GameObject localPlayer = GameManager.Instance.localPlayer;
        return localPlayer.GetComponent<BodyMovement>();
    }

    private T GetComponentOnLocalPlayer<T>()
    {
        var localPlayer = GetLocalPlayer();
        var someComponent = localPlayer.GetComponent<T>();
        if (someComponent != null)
            return someComponent;

        Debug.LogError($"[DEBUG] - Component of Type {typeof(T)} not found on Local Player");
        return default(T);

    }
}