using UnityEngine;
using UnityEngine.InputSystem;
using Ursaanimation.CubicFarmAnimals;

public class ReynoldsSteering : MonoBehaviour
{
    [SerializeField] float movementSpeed = 1.0f;
    [SerializeField] float linearDamping = 0.1f;
    public GameObject target;

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

    private Vector3 entrance;
    public bool chosen;

    bool avoidingObstacles;
    Vector3 goal;
    Vector3 previousForward;
    int prevDot;
    float totalAngle; 
    int oscillations;

    int collisions; 



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        previousForward = Vector3.zero;
        prevDot = 0;
        oscillations = 0;
        totalAngle = 0; 
        collisions = 0;
        entrance = new Vector3(5, 0, 10.5f);
        svm = new SimpleVehicleModel(transform.position);
        steeringDirection = Vector3.zero;
    }

    void FixedUpdate()
    {
        avoidingObstacles = false; 
        Finished(); 
        steeringDirection = Vector3.zero;
        bool obstacleAvoidanceSet;
        Seek();  
        ObstacleAvoidance(out obstacleAvoidanceSet);

        SetSteeringModel();
        ApplySteeringModel();

        NoteRotation();
        NoteCollisions();
    }

    void Finished()
    {
        if (transform.position.z > 20f)
        {
            AgentSpawner.instance.AddCollisions(collisions);
            AgentSpawner.instance.AddRotation(oscillations, totalAngle);
            AgentSpawner.instance.AddTime();
            AgentSpawner.instance.Agents.Remove(gameObject);
            Destroy(gameObject);
        }
    }

    void NoteRotation()
    {

        Vector3 forward = transform.forward; 
        forward.y = 0;
        int dot = (int)Mathf.Sign(Vector3.Dot(forward, (goal - transform.position).normalized));

        if (previousForward != Vector3.zero)
        {
            float angle = Vector3.Angle(forward, previousForward);
            totalAngle += angle;
            previousForward = forward;
        }
        else
        {
            previousForward = forward;
        }

        if (dot != prevDot && prevDot == 1)
        {
            oscillations += 1;
        }
        prevDot = dot; 
    }

    void NoteCollisions()
    {
        var agents = AgentSpawner.instance.Agents;
        foreach (var agent in agents)
        {
            if (agent == gameObject)
                continue;

            float dist = Vector3.Distance(agent.transform.position, transform.position);
            if (dist < 0.36f)
            {
                collisions += 1; 
            }
        }
    }


    void SetSteeringModel()
    {
        // Controlling speed. 
        if (steeringDirection != Vector3.zero)
        {
            var steeringForce = Vector3.ClampMagnitude(steeringDirection, svm.max_force);
            var acceleration = (steeringForce / svm.mass);

            
            svm.velocity = Vector3.ClampMagnitude(svm.velocity + acceleration, svm.max_speed);
        }

        Vector3 projectedPosition = Vector3.zero;
        if (avoidingObstacles)
            projectedPosition = svm.position + svm.velocity * Time.fixedDeltaTime;
        else
            projectedPosition = svm.position + svm.velocity * Time.fixedDeltaTime;

        if (allowedMovement(projectedPosition))
            svm.position = projectedPosition;

        // Controlling direction. 
        if (svm.velocity.magnitude < 0.1f) return;

        svm.orientation[0] = svm.velocity.normalized;
        svm.orientation[2] = Vector3.Cross(svm.orientation[1], svm.orientation[0]).normalized;
    }

    bool allowedMovement(Vector3 position)
    {
        if (position.z > 10.82f && position.x >= 4.1f && position.x <= 5.9f)
            return true;
        else if (position.z <= 10.82f || position.z > 18.5f)
            return true;
        else 
            return false; 
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

    void Seek()
    {
        if (targetVisible())
        {
            goal = target.transform.position;
        }
        else
        {
            goal = entrance; 
        }

        goal.y = 0f;
        var desiredVelocity = (goal - svm.position).normalized * svm.max_speed;
        steeringDirection = desiredVelocity - svm.velocity;
    }

    bool targetVisible()
    {
        RaycastHit hitInfo;
        Vector3 ToTarget = target.transform.position - transform.position;
        if (Physics.Raycast(transform.position + Vector3.up * 0.35f, ToTarget.normalized, out hitInfo, ToTarget.magnitude - 1f))
        {
            if (hitInfo.collider.CompareTag("Obstacle") || hitInfo.collider.CompareTag("TargetHelp"))
                return false;
        }
        return true;
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
        float angle = hitSide * -Mathf.Atan2(obstacleR, hitDistance) * ( 1 - hitDistance / maxObstacleDist);//Mathf.Asin(R / hitDistance)

        Vector3 offset_target = new Vector3(
            Mathf.Cos(angle) * toTarget.x - Mathf.Sin(angle) * toTarget.z,
            0,
            Mathf.Sin(angle) * toTarget.x + Mathf.Cos(angle) * toTarget.z);
        Debug.DrawRay(transform.position, offset_target, Color.yellow);

        var desiredVelocity = offset_target.normalized * svm.max_speed;

        avoidingObstacles = true; 
        Vector3 obstacleSteering = (desiredVelocity - svm.velocity) * obstacleAvoidanceStrength;
        steeringDirection = obstacleSteering; 
    }

    int ObstacleDetection(out float hitDistance)
    {
        RaycastHit hitInfo;
        Vector3 origin = transform.position + Vector3.up * 0.35f;
        Vector3 offset = transform.right * 0.15f;
        Debug.DrawRay(origin + offset, transform.forward * maxObstacleDist, Color.green);
        Debug.DrawRay(origin - offset, transform.forward * maxObstacleDist, Color.green);
        float hitDistanceRight = maxObstacleDist + 1;
        float hitDistanceLeft = maxObstacleDist + 1;

        if (Physics.Raycast(origin + offset, transform.forward, out hitInfo, maxObstacleDist))
        {
            // Collision right. 
            if (hitInfo.collider.CompareTag("Obstacle") || hitInfo.collider.CompareTag("Sheep"))
            {
                Debug.DrawRay(origin + offset, transform.forward * hitInfo.distance, Color.red);
                hitDistanceRight = hitInfo.distance;
            }
                
        }

        if (Physics.Raycast(origin - offset, transform.forward, out hitInfo, maxObstacleDist))
        {
            // Collision Left.
            if (hitInfo.collider.CompareTag("Obstacle") || hitInfo.collider.CompareTag("Sheep"))
            {
                Debug.DrawRay(origin - offset, transform.forward * hitInfo.distance, Color.red);
                hitDistanceLeft = hitInfo.distance;
            }
        }

        if (hitDistanceRight > maxObstacleDist && hitDistanceLeft > maxObstacleDist)
        {
            hitDistance = 0;
            return 0;
        }
        else if (hitDistanceLeft > hitDistanceRight)
        {
            hitDistance = hitDistanceRight;
            return -1;
        }
        else
        {
            hitDistance = hitDistanceLeft;
            return 1;
        }
    }
}
