using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    IA_Input iA_Input;
    public Rigidbody rb;

    [SerializeField, Header("FLOATING")] float CharacterGravityForce = 1000f;
    [SerializeField] float floatingRayCastLength = 3f;
    [SerializeField] float targetFloatingHeight = 1.5f;
    [SerializeField] float floatingSpringStrength = 10000f;
    [SerializeField] float floatingSpringDamper = 300f;




    [SerializeField, Header("Movement")]public float AccelerationFactor = 1.0f;
    [SerializeField] float JumpForce = 2000f;


    float JumpWaitTime = 0f;
    RaycastHit OutHit;
    bool isGravity = true;
    float GravityMultiplier = 1f;
    bool isJumping = false;
    bool isFalling = false;
    bool shouldLevitate = false;
    float verticalSpringFactor = 0f;



    Vector3 movementAmount = Vector3.zero;

    Vector2 InputVector2Value;
    Vector3 forwardDir ;


    void Start()
    {
        iA_Input = new IA_Input();
        rb = GetComponent<Rigidbody>();
        forwardDir = rb.transform.forward;
    }



    private void Update()
    {
      //Calculations
        isJumping = JumpingStateCheck();

        isFalling = FallingStateCheck();

        isGravity = GravityCheck();

        shouldLevitate = CheckIfShouldLevitate();

        verticalSpringFactor = CalculateLevitationParameters();

        movementAmount = CalculateMovementAmount();


      //Debug Tools
        DrawCG();
        DrawVelocityLine();
        DrawInputLine();
    }

    private void FixedUpdate()
    {
        AddGravityIfApplicable();

        LevitateIfApplicable();

        MoveIfApplicable();
    }


    //Input Action Calls
    void OnJump()
    {
        isGravity = true;
        JumpWaitTime = 0.5f;
        rb.AddForce(Vector3.up * JumpForce,ForceMode.Impulse);
    }
    void OnMove(InputValue inputValue)
    {
        InputVector2Value = inputValue.Get<Vector2>();


    }




    //Calculations Run in Update
    bool CharacterSphereCast(float Length)
    {
        bool rayCastable = Physics.SphereCast(transform.position, 0.5f, Vector3.down, out RaycastHit outHit, Length);

        Debug.DrawLine(transform.position, transform.position + Vector3.down * floatingRayCastLength, Color.red);


        DebugDraw.DrawSphere(new Vector4(
                                             (transform.position + Vector3.down * floatingRayCastLength).x,
                                             (transform.position + Vector3.down * floatingRayCastLength).y,
                                             (transform.position + Vector3.down * floatingRayCastLength).z,
                                           1), 0.5f, Color.red);
        OutHit = outHit;

        return rayCastable;
    }
    bool JumpingStateCheck()
    {
        if (JumpWaitTime > 0f)
        {
            JumpWaitTime -= Time.deltaTime;

            return true;
        }
        else
        {
            JumpWaitTime = 0f;
            return false;
        }
        

    }

    bool FallingStateCheck()
    {
        if (isJumping)
        { 
            return true;
        }

        else
        {
            return !CharacterSphereCast(floatingRayCastLength);
        }
    }

    bool GravityCheck()
    {
        if(isFalling)
        {
            return true;
        }

        else 
        { 
            return false; 
        }

    }

    bool CheckIfShouldLevitate()
    {
        if(!isFalling)
        {
            return true;
        }

        else
        {
            return false;
        }
        /*
                if (OutHit.distance < targetLandingHeight)
                {
                    Levitate(OutHit);
                    if (OutHit.rigidbody != null)
                    {
                        groundVel = OutHit.rigidbody.velocity;
                    }
        */
    }

    float CalculateLevitationParameters()
    {
        if (!shouldLevitate) { return 0f; }

        //Current Distance(float) to Default Point.
        float CurrentDistanceToDefault = OutHit.distance;

        //Target Distance(float) to Default Point == targetFloatingHeight.

        //Current Linear Velocity(float) Towards Default Point.
        float CurrentLinearVelocity = Vector3.Dot(Vector3.down, rb.velocity);

        //Target Linear Velocity(float) Towards Default Point.
        float TargetLinearVelocity;

        if (OutHit.rigidbody)
        {
            TargetLinearVelocity = Vector3.Dot(Vector3.down, OutHit.rigidbody.velocity);
        }
        else
        {
            TargetLinearVelocity = 0f;
        }

        //Calculate SpringActionFactor with Previous Variables.
        return Spring.SpringCalutation( CurrentDistanceToDefault, 
                                        targetFloatingHeight,
                                        CurrentLinearVelocity,
                                        TargetLinearVelocity, 
                                        floatingSpringStrength, 
                                        floatingSpringDamper);

    }

    Vector3 CalculateMovementAmount()
    {
        Vector3 inputMovementAmount = Vector3.zero;

        if (InputVector2Value.magnitude <= 0)
        {
           return inputMovementAmount = Vector3.zero;

        }

        forwardDir = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        Vector2 inputMoveDir = InputVector2Value;
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        inputMovementAmount = inputMoveDir.y * cameraForward + inputMoveDir.x * cameraRight;

        if (inputMovementAmount.magnitude > 1.0f)
        {
            inputMovementAmount.Normalize();
        }

        inputMovementAmount = inputMovementAmount * AccelerationFactor;

        return inputMovementAmount;

    }


    //Physics Actions Run in FixedUpdate
    void AddGravityIfApplicable()
    {
        if (!isGravity) {  return; }
        
        rb.AddForce(Vector3.down * 100 * GravityMultiplier, ForceMode.Acceleration);
         

    }
    void LevitateIfApplicable()
    {
        if(!shouldLevitate) { return; }

        rb.AddForce(Vector3.down * verticalSpringFactor);
    }
    void MoveIfApplicable()
    {



        rb.AddForce(Vector3.Scale(movementAmount, new(1,0,1)),ForceMode.Acceleration);



        

    }


    void DrawCG()
    {
        Vector3 worldCGPosition = transform.TransformPoint(rb.centerOfMass);
        DebugDraw.DrawSphere(new Vector4(worldCGPosition.x, worldCGPosition.y, worldCGPosition.z, 1), 0.2f, Color.yellow);
    }
    void DrawVelocityLine()
    {
        Debug.DrawLine(transform.position, transform.position + new Vector3(rb.velocity.x, 0, rb.velocity.z) * 50, Color.blue);
    }
    void DrawInputLine()
    {
        Debug.DrawLine(transform.position, transform.position + movementAmount * 20, Color.green);
    }

}

