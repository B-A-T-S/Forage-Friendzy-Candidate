using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class BodyMovement : Controlled3DBody
{

    private PreyInteractActor interactActor;
    private SelfHealActor healActor;

    [Header("Mechanic Bools")]
    [SerializeField] private bool canSprint;
    [SerializeField] private bool canSneak;

    [Header("Movement")]
    [SerializeField] float walkSpeed = 8;
    [SerializeField] float sprintSpeed = 12;
    [SerializeField] float sneakSpeed = 4;
    public float WalkSpeed { get { return walkSpeed; } set { walkSpeed = value; } }
    public float SprintSpeed { get { return sprintSpeed; } set { sprintSpeed = value; } }
    private bool sprinting;

    NetworkVariable<float> speedVariable=new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);



    private float topSpeed;

    [SerializeField] float accelSmoothTime = 0.2f;
    [SerializeField] float deccelSmoothTime = 0.01f;

    [Header("Elevation")]
    [SerializeField] CustomLayers whatIsGround;
    [SerializeField] float groundCheckRayDistance;
    [SerializeField] float gravityForceMultiplier = 1f;

    private float gravityForce = -13f;
    private float velocityY = 0f;

    [Header("Velocity Conversion")]
    [SerializeField] float velocityConversionFactor = 0.8f;
    [SerializeField] float velocityConversionThreshold = 0.50f;

    [Header("Information")]
    [SerializeField] MovementTypes currentMovementType;
    [SerializeField] Vector3 currentVelocity;
    public Vector3 CurrentVelocity
    {
        get { return currentVelocity; }
    }

    [SerializeField] Vector3 currentDir;
    [SerializeField] Vector3 currentForward;
    [SerializeField] Vector3 currentRight;

    private CharacterController controller;

    #region Previous Frame Data
    private Vector3 previousVelocity;
    private Vector3 previousDirection;
    private Vector3 previousMoveDir;
    private Vector3 previousCameraForward;
    private Vector3 previousCameraRight;
    private float previousXVelocityMax;
    private float previousZVelocityMax;


    #endregion

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        preyHealth = GetComponent<PreyHealth>();
        preyRescue = GetComponent<PreyRescue>();
        scurryComponent = GetComponent<AnimalScurry>();
        predatorAttack = GetComponent<PredatorAttack>();
        perkComponent = GetComponent<Perks>();
        interactActor = GetComponent<PreyInteractActor>();

        topSpeed = Mathf.Max(walkSpeed, sprintSpeed, sneakSpeed);
        sprinting = false;

        speedVariable.OnValueChanged += SpeedOVC;
    }

    private void SpeedOVC(float previousValue, float newValue)
    {
        Anim_SetFloat(animFloat_Speed, newValue);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        InputPayload inputPayload = GetInputs(new Inputs(linkedController), 0);
        ProcessMovement(inputPayload);
    }

    #region Movement
    StatePayload ProcessMovement(InputPayload input)
    {
        //snap geo to slope
        geoUtility.AlignToNormal();

        //move via character controller
        Move(input);

        //Set Animator Velocity Value
        Vector3 velocityNoY = currentVelocity;
        velocityNoY.y = 0;
        speedVariable.Value = velocityNoY.magnitude / topSpeed;
        
        

        //Construct StatePayload
        return new StatePayload
        {
            tick = input.tick,
            position = transform.position,
            velocity = currentVelocity
        };

    }

    private void Move(InputPayload inputPayload)
    {
        Vector3 targetDirection = inputPayload.inputVector;
        targetDirection.Normalize();

        Vector3 newVelocity;
        //if not fainted, not scurrying, input exists
        if ((!IsSelfHealing() && (ClientLaunchInfo.Instance.character == 2 && ClientLaunchInfo.Instance.role == 0)) && !IsInteracting() && !IsStunned()
            && !IsFainted() && !IsScurrying() && targetDirection != Vector3.zero)
            newVelocity = Accelerate(inputPayload);
        else
            newVelocity = Deccelerate();

        //Apply Gravity
        if (IsGrounded())
        {
            velocityY = 0;
        }
        else
            velocityY += gravityForce * gravityForceMultiplier * Time.deltaTime;
        
        //Create Movement Vector
        currentVelocity = newVelocity + (Vector3.up * velocityY);

        controller.Move(currentVelocity * Time.deltaTime);
        previousVelocity = currentVelocity;
    }

    private Vector3 Deccelerate()
    {

        if (currentVelocity == Vector3.zero)
            return currentVelocity;

        Vector3 decceleratedVelocity = currentVelocity;
        Vector3 deccelerationPerSecond = previousFrameIntendedVelocity / deccelSmoothTime;
        decceleratedVelocity -= deccelerationPerSecond * Time.deltaTime;

        #region Clamp

        if (previousFrameIntendedVelocity.x > 0)
            decceleratedVelocity.x = Mathf.Clamp(decceleratedVelocity.x, 0f, previousFrameIntendedVelocity.x);
        else
            decceleratedVelocity.x = Mathf.Clamp(decceleratedVelocity.x, previousFrameIntendedVelocity.x, 0f);

        if (previousFrameIntendedVelocity.z > 0)
            decceleratedVelocity.z = Mathf.Clamp(decceleratedVelocity.z, 0f, previousFrameIntendedVelocity.z);
        else
            decceleratedVelocity.z = Mathf.Clamp(decceleratedVelocity.z, previousFrameIntendedVelocity.z, 0f);

        #endregion

        return decceleratedVelocity;
    }

    Vector3 previousFrameIntendedVelocity; 

    private Vector3 Accelerate(InputPayload payload)
    {
        geoUtility.DetermineDirectionViaInput(payload);
        Vector3 acceleratedVelocity = currentVelocity;
        float maxSpeed = /*GetMaxSpeed(payload.moveType);*/walkSpeed;

        Vector3 intendedForward = payload.cameraForward * payload.inputVector.z;
        Vector3 intendedRight = payload.cameraRight * payload.inputVector.x;

        Vector3 intendedVelocity = (intendedForward * maxSpeed) + (intendedRight * maxSpeed);

        #region Conservation

        Vector3 direction = intendedForward + intendedRight;
        if (direction != previousDirection && acceleratedVelocity != Vector3.zero)
        {
            //Debug.Log("Changed Direction.");
            float progressionToMaxSpeed = acceleratedVelocity.magnitude / maxSpeed;
            float conversionFactor = Vector3.Distance(direction, previousDirection) >= velocityConversionThreshold ? velocityConversionFactor : 1f;
            acceleratedVelocity.x = intendedVelocity.x * progressionToMaxSpeed * conversionFactor;
            acceleratedVelocity.z = intendedVelocity.z * progressionToMaxSpeed * conversionFactor;
        }

        #endregion

        acceleratedVelocity += (intendedVelocity / accelSmoothTime) * Time.deltaTime;
        acceleratedVelocity = Vector3.ClampMagnitude(acceleratedVelocity, maxSpeed);

        previousDirection = direction;
        previousFrameIntendedVelocity = intendedVelocity;
        return acceleratedVelocity;
    }
    #endregion

    #region Helpers

    private float GetSignAsFloat(float n)
    {
        if (n < 0) return -1.0f;
        if (n == 0) return 0.0f;
        return 1.0f;
    }

    //Given an Input struct, create and fill in
    //an InputPayload instance
    InputPayload GetInputs(Inputs inputs, int currentTick)
    {

        InputPayload inputPayload = new InputPayload();
        inputPayload.tick = currentTick;
        inputPayload.clientId = NetworkManager.Singleton.LocalClientId;

        inputPayload.inputVector = DetermineInputVector(inputs);

        inputPayload.cameraForward = new Vector3(cameraReference.transform.forward.x, 0, cameraReference.transform.forward.z);
        inputPayload.cameraRight = new Vector3(cameraReference.transform.right.x, 0, cameraReference.transform.right.z);
        inputPayload.cameraPosition = cameraReference.MyCamera.transform.position;

        //inputPayload.moveType = DetermineMovementType(inputs);
        //currentMovementType = inputPayload.moveType;

        return inputPayload;

    }

    /*
    MovementTypes DetermineMovementType(Inputs inputs)
    {
        if (!sprinting && inputs.isSprintKeyPressed)
        {
            sprinting = true;
            perkComponent.HopToIt();
        }else if (sprinting && !inputs.isSprintKeyPressed) {
            sprinting = false; 
        }

        if ((inputs.isSprintKeyPressed && !inputs.isSneakKeyPressed) && canSprint)
            return MovementTypes.Sprinting;
        else if ((inputs.isSneakKeyPressed && !inputs.isSprintKeyPressed) && canSneak)
            return MovementTypes.Sneaking;
        
        return MovementTypes.Walking;
    }
    

    float GetMaxSpeed(MovementTypes moveType)
    {
        switch (moveType)
        {
            case MovementTypes.Sprinting:
                return sprintSpeed;
            case MovementTypes.Sneaking:
                return sneakSpeed;
            case MovementTypes.Walking:
                return walkSpeed;
        }


        return 0f;
    }
    */

    Vector3 DetermineInputVector(Inputs inputs)
    {
        Vector3 inputVector = Vector3.zero;
        inputVector.x = Convert.ToInt32(inputs.isRightKeyPressed);
        inputVector.x -= Convert.ToInt32(inputs.isLeftKeyPressed);
        inputVector.z = Convert.ToInt32(inputs.isUpKeyPressed);
        inputVector.z -= Convert.ToInt32(inputs.isDownKeyPressed);

        return inputVector;
    }

    public void ToggleCollisionsWithCollection(Collider[] colliders, bool allowUnignore)
    {
        // for each collider in given collection...
        foreach(Collider collider in colliders) 
        {
            //check if we are already ignoring this collider and we are allowed to reactivate if we are ignoring
            if (Physics.GetIgnoreCollision(controller, collider))
                Physics.IgnoreCollision(controller, collider, !allowUnignore);
            else //if we are not ignoring, then begin ignoring
                Physics.IgnoreCollision(controller, collider, true);
        }
    }

    public void ToggleCharacterController(bool active)
    {
        controller.enabled = active;
    }

    public CharacterController GetCharacterController()
    {
        return controller;
    }

    #endregion

    #region Getters

    private bool IsFainted()
    {
        if (preyHealth != null)
            if (preyHealth.isFainted.Value)
                return true;

        return false;
    }

    private bool IsInteracting()
    {
        if (interactActor == null)
            return false;

        return interactActor.isInteracting;
    }

    private bool IsSelfHealing()
    {
        if (healActor == null)
            return false;

        return healActor.isHealing;
    }

    private bool IsScurrying()
    {
        return scurryComponent.IsScurrying;
    }

    private bool IsStunned()
    {
        if (predatorAttack != null)
            return predatorAttack.isStunned.Value;
        return false;
    }

    private bool IsGrounded()
    {
        RaycastHit slopeHit;
        if (Physics.Raycast(new Ray(transform.position, Vector3.down), out slopeHit, groundCheckRayDistance/*, (int) whatIsGround*/))
        {
            return true;
        }

        return false;
    }

    #endregion
}

public struct Inputs
{
    public bool isRightKeyPressed;
    public bool isLeftKeyPressed;
    public bool isUpKeyPressed;
    public bool isDownKeyPressed;
    public bool isInteractKeyPressed;
    public bool isSprintKeyPressed;
    public bool isSneakKeyPressed;

    public Inputs(PlayerController controller)
    {
        if (controller != null)
        {
            isRightKeyPressed = Input.GetKey(controller.rightKey);
            isLeftKeyPressed = Input.GetKey(controller.leftKey);
            isUpKeyPressed = Input.GetKey(controller.upKey);
            isDownKeyPressed = Input.GetKey(controller.downKey);
            isInteractKeyPressed = Input.GetKey(controller.interact);
            isSprintKeyPressed = Input.GetKey(controller.sprint);
            isSneakKeyPressed = Input.GetKey(controller.sneak);
        }
        else
        {
            isRightKeyPressed = false;
            isLeftKeyPressed = false;
            isUpKeyPressed = false;
            isDownKeyPressed = false;
            isInteractKeyPressed = false;
            isSprintKeyPressed = false;
            isSneakKeyPressed = false;
        }

    }

}

public enum MovementTypes
{
    Sneaking,
    Walking,
    Sprinting
}