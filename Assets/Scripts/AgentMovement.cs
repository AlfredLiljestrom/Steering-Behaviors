using UnityEngine;
using UnityEngine.InputSystem;
using Ursaanimation.CubicFarmAnimals;

public class AgentMovement : MonoBehaviour
{
    [Header("Fleeing Behavior")]
    [SerializeField] GameObject chaser;
    [SerializeField] float fleeDistance; 

    [SerializeField] float movementSpeed = 1.0f;
    [SerializeField] float rotationSpeed = 1.0f;
    [SerializeField] float linearDamping = 0.1f;
    [SerializeField] GameObject target;

    [Header("Pursuit Modifier")]
    [SerializeField] float T;

    [Header("Arrival Modifiers")]
    [SerializeField] float slowing_distance;

    [Header("Offset Pursuit")]
    [SerializeField] float R;

    [Header("Obstacle Avoidance")]
    [SerializeField] float player_radius;
    [SerializeField] float maxObstacleDist;
    [SerializeField] float obstacleR;
    [SerializeField] float obstacleAvoidanceStrength;

    [Header("Wander")]
    [SerializeField] bool wanderEnable;
    [SerializeField] float wanderRadius;
    [SerializeField] float wanderRandomness;
    [SerializeField] float wanderStrength;

    public SimpleVehicleModel svm;
    Vector3 steeringDirection;

    AnimationController controller; 



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<AnimationController>();
        svm = new SimpleVehicleModel(transform.position);
        steeringDirection = Vector3.zero;
    }

    void FixedUpdate()
    {
        steeringDirection = Vector3.zero;
        bool obstacleAvoidanceSet;
        KeyBinds();
        FleeFromChaser();
        ObstacleAvoidance(out obstacleAvoidanceSet);


        if (!obstacleAvoidanceSet && steeringDirection == Vector3.zero)
        {
            Wander();
        }
        

        SetSteeringModel();
        ApplySteeringModel();
    }


    void SetSteeringModel()
    {
        // Controlling speed. 
        if (steeringDirection == Vector3.zero) return;

        var steeringForce = Vector3.ClampMagnitude(steeringDirection, svm.max_force);
        var acceleration = (steeringForce / svm.mass);

        svm.velocity = Vector3.ClampMagnitude(svm.velocity + acceleration - linearDamping * svm.velocity, svm.max_speed);
        svm.position = svm.position + svm.velocity * movementSpeed;

        // Controlling direction. 
        if (svm.velocity.magnitude < 0.1f) return;

        svm.orientation[0] = svm.velocity.normalized;
        svm.orientation[2] = Vector3.Cross(svm.orientation[1], svm.orientation[0]).normalized;
    }

    void ApplySteeringModel()
    {
        transform.position = svm.position;
        transform.rotation = transformRotation().rotation;
    }

    Matrix4x4 transformRotation()
    {
        Vector3 forward = svm.orientation[0];
        Vector3 up = svm.orientation[1];
        Vector3 right = svm.orientation[2];

        Matrix4x4 m = new Matrix4x4();
        m.SetColumn(0, new Vector4(right.x, right.y, right.z, 0));
        m.SetColumn(1, new Vector4(up.x, up.y, up.z, 0));
        m.SetColumn(2, new Vector4(forward.x, forward.y, forward.z, 0));
        m.SetColumn(3, new Vector4(0, 0, 0, 1));
        return m;
    }

    void FleeFromChaser()
    {
        Vector3 chaserPosition2D = chaser.transform.position;
        chaserPosition2D.y = 0;

        Vector3 sheep2D = svm.position;
        sheep2D.y = 0; 

        float distanceToChaser = Vector3.Distance(sheep2D, chaserPosition2D); 
        if (distanceToChaser > fleeDistance)
        {
            return;
        }

        controller.animator.Play(controller.runForwardAnimation); 

        var desiredVelocity = (sheep2D - chaserPosition2D).normalized * svm.max_speed;
        steeringDirection = desiredVelocity - svm.velocity;
    }

    void KeyBinds()
    {
        var kb = Keyboard.current;

        if (kb.qKey.isPressed)
            Seek();
        else if (kb.eKey.isPressed)
            Flee();
        if (kb.rKey.isPressed)
            Pursuit();
        if (kb.tKey.isPressed)
            Evasion();
        if (kb.fKey.isPressed)
            Arrival();
        if (kb.gKey.isPressed)
            OffsetPursuit();
    }

    void Wander()
    {
        if (!wanderEnable)
            return;

        float randomDisplacement = (Mathf.PerlinNoise(svm.position.x * wanderRandomness, svm.position.z * wanderRandomness) * 2f) - 1f;
        float randomDisplacement2 = Mathf.PerlinNoise(svm.position.z * wanderRandomness, svm.position.x * wanderRandomness) * 2f;

        Vector3 randomWander = transform.forward + transform.right * Mathf.Sin(randomDisplacement) * (wanderRadius + randomDisplacement2 * wanderRadius);
        steeringDirection = randomWander * wanderStrength;

        controller.animator.Play(controller.walkForwardAnimation);
        Debug.DrawLine(transform.position, transform.position + randomWander * 10f);
    }

    void OffsetPursuit()
    {
        var toTarget = target.transform.position - svm.position;
        var distance = toTarget.magnitude;
        float R_Clamped = Mathf.Clamp(R, -distance, distance);
        float angle = Mathf.Asin(R_Clamped / distance);
        Vector3 offset_target = new Vector3(
            Mathf.Cos(angle) * toTarget.x - Mathf.Sin(angle) * toTarget.z,
            0,
            Mathf.Sin(angle) * toTarget.x + Mathf.Cos(angle) * toTarget.z);

        var desiredVelocity = offset_target.normalized * svm.max_speed;
        steeringDirection = desiredVelocity - svm.velocity;
    }

    void Arrival()
    {

        var target_offset = target.transform.position - svm.position;
        var distance = target_offset.magnitude;
        target_offset.y = 0;
        var ramped_speed = svm.max_speed * (distance / slowing_distance);
        var clipped_speed = Mathf.Min(ramped_speed, svm.max_speed);
        var desired_velocity = (clipped_speed / distance) * target_offset;
        steeringDirection = desired_velocity - svm.velocity;
    }

    void Pursuit()
    {
        SimpleVehicleModel svmTarget = target.GetComponent<AgentMovement>().svm;
        var targetPosition = svmTarget.position;
        var targetVelocity = svmTarget.velocity;

        var targetFuture = targetPosition + targetVelocity * T;
        targetFuture.y = 0f;

        var desiredVelocity = (targetFuture - svm.position).normalized * svm.max_speed;
        steeringDirection = desiredVelocity - svm.velocity;
    }

    void Evasion()
    {
        SimpleVehicleModel svmTarget = target.GetComponent<AgentMovement>().svm;
        var targetPosition = svmTarget.position;
        var targetVelocity = svmTarget.velocity;

        var targetFuture = targetPosition + targetVelocity * T;
        targetFuture.y = 0f;

        var desiredVelocity = (svm.position - targetFuture).normalized * svm.max_speed;
        steeringDirection = desiredVelocity - svm.velocity;
    }

    void Seek()
    {
        Vector3 target2D = target.transform.position;
        target2D.y = 0f;

        var desiredVelocity = (target2D - svm.position).normalized * svm.max_speed;
        steeringDirection = desiredVelocity - svm.velocity;
    }

    void Flee()
    {
        Vector3 target2D = target.transform.position;
        target2D.y = 0f;

        var desiredVelocity = (svm.position - target2D).normalized * svm.max_speed;
        steeringDirection = desiredVelocity - svm.velocity;
    }

    void ObstacleAvoidance(out bool obstacleAvoidanceSet)
    {
        obstacleAvoidanceSet = false;
        float hitDistance;
        int hitSide = ObstacleDetection(out hitDistance);

        // Nothing hit. 
        if (hitSide == 0) return;
        obstacleAvoidanceSet = true;

        var toTarget = transform.forward * hitDistance;
        float R = Mathf.Clamp(obstacleR, 0f, hitDistance);
        float angle = hitSide * Mathf.Asin(R / hitDistance) * (maxObstacleDist - hitDistance / maxObstacleDist);

        Vector3 offset_target = new Vector3(
            Mathf.Cos(angle) * toTarget.x - Mathf.Sin(angle) * toTarget.z,
            0,
            Mathf.Sin(angle) * toTarget.x + Mathf.Cos(angle) * toTarget.z);

        var desiredVelocity = offset_target.normalized * svm.max_speed;

        steeringDirection = (desiredVelocity - svm.velocity) * obstacleAvoidanceStrength;
    }

    int ObstacleDetection(out float hitDistance)
    {
        RaycastHit hitInfo;
        Vector3 origin = svm.position;
        Vector3 offset = transform.right * player_radius;
        Debug.DrawRay(origin + offset + Vector3.up, transform.forward * maxObstacleDist, Color.green);
        Debug.DrawRay(origin - offset + Vector3.up, transform.forward * maxObstacleDist, Color.green);
        float hitDistanceRight = 0;
        float hitDistanceLeft = 0;

        if (Physics.Raycast(origin + offset + Vector3.up, transform.forward, out hitInfo, maxObstacleDist))
        {
            // Collision right. 
            Debug.DrawRay(origin + offset + Vector3.up, transform.forward * hitInfo.distance, Color.red);
            if (hitInfo.collider.tag == "Obstacle")
                hitDistanceRight = hitInfo.distance;
        }

        if (Physics.Raycast(origin - offset + Vector3.up, transform.forward, out hitInfo, maxObstacleDist))
        {
            // Collision Left.
            Debug.DrawRay(origin - offset + Vector3.up, transform.forward * hitInfo.distance, Color.red);
            if (hitInfo.collider.tag == "Obstacle")
                hitDistanceLeft = hitInfo.distance;
        }

        if (hitDistanceRight == 0 && hitDistanceLeft == 0)
        {
            hitDistance = 0;
            return 0;
        }
        else if (hitDistanceRight > hitDistanceLeft)
        {
            hitDistance = hitDistanceRight;
            return 1;
        }
        else
        {
            hitDistance = hitDistanceLeft;
            return -1;
        }


    }
}
