using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    IA_Input iA_Input;
    Rigidbody rb;


    [SerializeField,Header("FLOATING")] float floatingRayCastLength = 3f;
    [SerializeField] float targetFloatingHeight = 1.5f;
    [SerializeField] float targetLandingHeight = 1.7f;
    [SerializeField] float floatingSpringStrength = 1000f;
    [SerializeField] float CharacterGravityForce = 1000f;
    [Range(0f, 10f)] [SerializeField] float floatingSpringDamper = 0.07f;
    [SerializeField] float JumpForce = 1000f;


    [Range(-10f, 10.0f)][SerializeField, Header("UPRIGHT FORCE")] float CGOffset;
    [SerializeField] float uprightJointSpringStrength = 1000.0f;
    [SerializeField] float uprightJointSpringDamper = 100.0f;


    [SerializeField, Header("Movement")] AnimationCurve accelerationFactorFromDot;
    [SerializeField] float AccelerationValue = 200.0f;
    public float maxSpeed = 8.0f;
    [field: SerializeField] public float speedFactor = 1.0f;
    [field: SerializeField] public float maxAccelForce = 150f;
    [field: SerializeField] public AnimationCurve maxAccelerationForceFactorFromDot;
    [field: SerializeField] public float maxAccelForceFactor = 1f;




    float JumpWaitTime = 0f;
    RaycastHit OutHit;
    bool isGravity = true;
    Vector2 InputVector2Value;
    Vector3 forwardDir ;

    Vector3 goalVelocity = Vector3.zero;
    Vector3 groundVel = Vector3.zero;
    Vector3 InputUnitmoveAmount;



    void Start()
    {
        iA_Input = new IA_Input();
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3 (rb.centerOfMass.x,rb.centerOfMass.y + CGOffset, rb.centerOfMass.z);
        forwardDir = rb.transform.forward;
    }

    private void FixedUpdate()
    {
        DrawCG();
        DrawVelocityLine();
        DrawInputLine();


        LevitateIfApplicable();

        Move();

        SetCharacterRotation();

        GiveCharacterGravityIfApplicable(CharacterGravityForce);


    }


    void OnJump()
    {
        isGravity = true;
        JumpWaitTime = 0.5f;
        rb.AddForce(Vector3.up * JumpForce);
    }
    void OnMove(InputValue inputValue)
    {
        InputVector2Value = inputValue.Get<Vector2>();


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
        Debug.DrawLine(transform.position, transform.position + InputUnitmoveAmount * 20, Color.green);
    }

    bool CharacterRayCast(float Length)
    {
         bool rayCastable = Physics.Raycast(transform.position, Vector3.down, out RaycastHit outHit, Length);

        Debug.DrawLine(transform.position, transform.position + Vector3.down * floatingRayCastLength, Color.red);

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

        return false;

    }
    void GiveCharacterGravityIfApplicable(float GravityForce)
    {
        if (!isGravity) { return; }

        rb.AddForce(Vector3.down * GravityForce *Time.fixedDeltaTime);

    }
    void LevitateIfApplicable()
    {
        bool rayCastable = CharacterRayCast(floatingRayCastLength); ;

        if (!JumpingStateCheck())
        {
            if (rayCastable)
            {

                if (OutHit.distance < targetLandingHeight)
                {
                    Levitate(OutHit);
                    if (OutHit.rigidbody != null)
                    {
                        groundVel = OutHit.rigidbody.velocity;
                    }
                }

            }
            else
            {
                isGravity = true;
            }

        }
    }
    void Levitate(RaycastHit OutHit)
    {

        isGravity = false;

            
            //raycast结果高度和期望悬浮高度的差，+太高了，-太低了
            float targetHeightDiff = OutHit.distance - targetFloatingHeight;

            //计算弹簧力
            float springForce =  targetHeightDiff * floatingSpringStrength;

            //相对速度= 我的速度（dot变垂直）-脚下物体的速度（dot变垂直）
            float downVelocity = Vector3.Dot(Vector3.down, rb.velocity);

            float downOutHitVelocity = 0f;

            if (OutHit.rigidbody)
            {
                 downOutHitVelocity = Vector3.Dot(Vector3.down, OutHit.rigidbody.velocity);
            }

            float relativeVelocity = downVelocity - downOutHitVelocity;

            

            //让力有衰减，加上damper（速度与风阻正相关）
            springForce = springForce - relativeVelocity * floatingSpringStrength * floatingSpringDamper;

            //施力
            rb.AddForce(Vector3.down * springForce);



        }
    void Move()
    {

        if (InputVector2Value.magnitude <= 0)
        {
            goalVelocity = new Vector3 (0, 0, 0);
        }


        forwardDir = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        Vector2 inputMoveDir = InputVector2Value;
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        InputUnitmoveAmount = inputMoveDir.y * cameraForward + inputMoveDir.x * cameraRight;
       
        if (InputUnitmoveAmount.magnitude > 1.0f)
        {
            InputUnitmoveAmount.Normalize();
        }






        Vector3 UnitVelocity = goalVelocity.normalized;

        float VelocityDot = Vector3.Dot(UnitVelocity,InputUnitmoveAmount );

        float acceleration = AccelerationValue * accelerationFactorFromDot.Evaluate(VelocityDot);

        Vector3 PotentialMaximumVelocity = InputUnitmoveAmount * maxSpeed * speedFactor;

        goalVelocity = Vector3.MoveTowards(goalVelocity, PotentialMaximumVelocity + groundVel, acceleration* Time.fixedDeltaTime);

        Vector3 neededAcceleration = (goalVelocity - rb.velocity)/Time.fixedDeltaTime;

        float maxAccel = maxAccelForce * maxAccelerationForceFactorFromDot.Evaluate(VelocityDot) * maxAccelForceFactor;

        neededAcceleration = Vector3.ClampMagnitude(neededAcceleration, maxAccel);

        Debug.Log("Velocity");
        Debug.Log(rb.velocity);
        Debug.Log("goalVelocity");
        Debug.Log(goalVelocity);
        Debug.Log("neededAcceleration");
        Debug.Log(neededAcceleration);
        Debug.Log("acceleration");
        Debug.Log(acceleration);

        rb.AddForce(Vector3.Scale(neededAcceleration* rb.mass*Time.deltaTime, new(1,0,1)));







    }
    void SetCharacterRotation()
    {                    

            Quaternion currentRot = transform.rotation;
            
            Quaternion forwardDirRot = Quaternion.LookRotation(forwardDir,Vector3.up);

            Quaternion toGoal = ShortestRotation(forwardDirRot, currentRot);

            Vector3 rotAxis;
            float rotDegrees;

            toGoal.ToAngleAxis(out rotDegrees, out rotAxis);
            rotAxis.Normalize();

            float rotRadians = rotDegrees * Mathf.Deg2Rad;

            rb.AddTorque((rotAxis * (rotRadians * uprightJointSpringStrength)) - (rb.angularVelocity * uprightJointSpringDamper));

    }
    Quaternion ShortestRotation(Quaternion goalQuanternion, Quaternion currentQuanternion)
    {
        {
            if (Quaternion.Dot(goalQuanternion, currentQuanternion) < 0)
            {
                return goalQuanternion * Quaternion.Inverse(Multiply(currentQuanternion, -1));
            }
            else return goalQuanternion * Quaternion.Inverse(currentQuanternion);
        }
    }
    static Quaternion Multiply(Quaternion input, float scalar)

    {

        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);

    }


}

