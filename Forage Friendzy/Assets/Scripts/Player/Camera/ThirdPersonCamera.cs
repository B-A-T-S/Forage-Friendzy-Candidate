using UnityEngine;
using Unity.Netcode;
using System;

public class ThirdPersonCamera : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Controlled3DBody body;
    [SerializeField] private GameObject myCamera;
    public GameObject MyCamera
    {
        get { return myCamera; }
    }

    [Header("Cursor Lock State")]
    [SerializeField] private KeyCode toggleCursorLockState = KeyCode.LeftAlt;
    private bool isLocked = false;

    [Header("Rotation Settings")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float minimumPitch, maximumPitch;
    private Vector3 currentEulerAngles;
    public Vector3 CurrentEulerAngles { get; }

    [Header("X-Ray")]
    [SerializeField] private GameObject preyXRayCamera, predatorXRayCamera, allXRayCamera;
    public NetworkVariable<bool> xRay_CanSeePrey, xRay_CanSeePred;

    private float currentPitch, currentYaw;
    private float pitchOrigin, currentPitchOrigin;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            DisableMainCamera();
        }

        xRay_CanSeePrey.OnValueChanged += XRayBool_OnValueChanged;
        xRay_CanSeePred.OnValueChanged += XRayBool_OnValueChanged;

        UpdatePitchAndYaw();
        pitchOrigin = transform.rotation.eulerAngles.x;
        currentPitchOrigin = pitchOrigin;

        ToggleCursorLockState();
        
    }

    private void OnEnable()
    {
        if(body != null)
        {
            ListenToBodyLinks(body);
        }
    }

    private void ListenToBodyLinks(Controlled3DBody newBody)
    {
        newBody.OnBodyUnlink += Body_Unlinked;
        newBody.OnBodyLink += Body_Linked;
    }

    private void UpdatePitchAndYaw()
    {
        currentPitch = gameObject.transform.rotation.eulerAngles.x;
        currentYaw = gameObject.transform.rotation.eulerAngles.y;
    }


    // Update is called once per frame
    void Update()
    {
        if(myCamera == null)
            return;

        if (myCamera.gameObject.activeSelf == false || body.linkedController == null || !body.linkedController.isLinked)
            return;

        CameraInputs inputs = new CameraInputs(body.linkedController);
        RotateCamera(inputs);
        if (Input.GetKeyUp(toggleCursorLockState))
            ToggleCursorLockState();
    }

    private void ToggleCursorLockState()
    {
        //if locked, unlock
        if (isLocked)
            UnlockCursor();
        else
            LockCursor();
          
    }

    private void LockCursor()
    {
        isLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        isLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    #region Main Camera

    /*
    public void AdjustPitchOrigin()
    {
        if(body.OnSlope())
            currentPitchOrigin = pitchOrigin - body.currentSlopeAngle;
        else
            currentPitchOrigin = pitchOrigin;
    }
    */



    private void RotateCamera(CameraInputs inputs)
    {

        UpdatePitchAndYaw();

        //if mouse is not locked, do not rotate camera
        if (!isLocked)
            return;

        //Mouse-Based Rotation
        currentYaw += rotationSpeed * mouseSensitivity * inputs.mouseInputVector.x;
        currentPitch -= rotationSpeed * mouseSensitivity * inputs.mouseInputVector.y;

        //safety set
        if(currentYaw == 0)
            currentYaw = 0.01f;

        //clamp pitch
        //get the relative range of pitch restrictions

        float resultantMaximumPitch = maximumPitch - currentPitchOrigin;
        float resultantMinimumPitch = minimumPitch + currentPitchOrigin;

        float relRange = (resultantMaximumPitch - resultantMinimumPitch) / 2f;
        float offset = resultantMaximumPitch - relRange;
        //ensure pitch is relative
        currentPitch = ((currentPitch + 540) % 360) - 180 - offset;

        //is pitch over the alloted range?
        if(Mathf.Abs(currentPitch) > relRange)
        {
            currentPitch = relRange * Mathf.Sign(currentPitch) + offset;
        }

        Vector3 newRotationEuler = new Vector3(currentPitch, currentYaw, 0);
        currentEulerAngles = newRotationEuler;
        transform.rotation = Quaternion.Euler(newRotationEuler);

    }

    public void EnableMainCamera()
    {
        myCamera.SetActive(true);
    }

    public void DisableMainCamera()
    {
        myCamera.SetActive(false);
    }

    #endregion

    #region X Ray

    public void ToggleXRayCameras(bool canSeePrey, bool canSeePred, bool canSeeAll)
    {
        preyXRayCamera.SetActive(canSeePrey);
        predatorXRayCamera.SetActive(canSeePred);
        allXRayCamera.SetActive(canSeeAll);
    }


    private void XRayBool_OnValueChanged(bool previousValue, bool newValue)
    {
        bool canSeePrey = xRay_CanSeePrey.Value;
        bool canSeePred = xRay_CanSeePred.Value;

        //case: canSeePrey = true, canSeePred = false
        // result -> (true, true), (false, false), (true, false) -> just prey camera

        //case: canSeePrey = false, canSeePred = true
        // result -> (false, false), (true, true), (false, true) -> just predator camera

        //case: canSeePrey = true, canSeePred = true
        // result -> (true, false), (false, true), (true, true) -> just combined camera

        //case: canSeePrey = false, canSeePred = false
        // result -> (false, true), (true, false), (false, false) -> all disabled

        ToggleXRayCameras((canSeePrey && !canSeePred), (!canSeePrey && canSeePred), (canSeePrey && canSeePred));

    }

    #endregion

    private void Body_Linked()
    {
        EnableMainCamera();
    }

    private void Body_Unlinked()
    {
        DisableMainCamera();
    }
    
}

struct CameraInputs
{
    public Vector2 mouseInputVector;
    public Vector2 controllerInputVector;

    public CameraInputs(PlayerController controller) 
    {
        float mouseX = Input.GetAxis(controller.mouseX);
        float mouseY = Input.GetAxis(controller.mouseY);
        mouseInputVector = new Vector2(mouseX, mouseY);

        float horizontal = Input.GetAxis(controller.horizontal);
        float vertical = Input.GetAxis(controller.vertical);
        controllerInputVector = new Vector2(horizontal, vertical);
    }


}