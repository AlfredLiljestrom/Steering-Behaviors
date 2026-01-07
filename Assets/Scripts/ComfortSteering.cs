using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class ComfortSteering : MonoBehaviour
{
    [Header("Model Parameters")]
    public float l = 0.36f; // Diameter of actor. 
    public float T = 0.86f; // Acceleration. 
    public float freeSpeedMean = 1.2f;
    public float freeSpeedStdev = 0.249f;
    float freeSpeed = 1.0f;

    private Vector3 desiredDirection;
    private Vector3 desiredDirTranspose;

    private float s_i = float.PositiveInfinity;
    private Vector3 closestAgentPos;

    private float velocity;
    public Vector3 direction;

    public GameObject target;

    public bool chosen = false; 
    private Vector3 entrance;
    Vector3 previousForward;
    int prevDot;
    float totalAngle;
    int oscillations;
    int collisions; 



    private void Start()
    {
        SetFreeSpeed();
        prevDot = 0;
        totalAngle = 0f;
        oscillations = 0;
        collisions = 0;
        entrance = new Vector3 (5, 0, 10.5f);
    }

    private void FixedUpdate()
    {
        Finished(); 
        ComputeDesiredDirections(); 

        GetClosestAgent();

        float sc = freeSpeed * 0.4f;
        float delthaTheta = 0f;

        if (s_i < sc && closestAgentPos != Vector3.zero)
        {
            delthaTheta = GetAngle();
        }

        direction = RotateVector(desiredDirection, delthaTheta).normalized;
        velocity = CalculateVelocity(); 

        ApplySteering();

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
        int dot = (int)Mathf.Sign(Vector3.Dot(forward, desiredDirection));

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

    void SetFreeSpeed()
    {
        float u1 = 1.0f - Random.value;
        float u2 = 1.0f - Random.value;

        float randStdNormal =
            Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
            Mathf.Cos(2.0f * Mathf.PI * u2);

        freeSpeed = freeSpeedMean; //+ freeSpeedStdev * randStdNormal;
    }

    void ComputeDesiredDirections()
    {
        if (targetVisible())
        {
            desiredDirection = (target.transform.position - transform.position).normalized;
        }
        else
        {
            desiredDirection = (entrance - transform.position).normalized;
        }

        desiredDirTranspose = new Vector3(-desiredDirection.z, 0f, desiredDirection.x);
    }

    bool targetVisible()
    {
        RaycastHit hitInfo;
        Vector3 ToTarget = target.transform.position - transform.position;
        if (Physics.Raycast(transform.position, ToTarget.normalized, out hitInfo, ToTarget.magnitude - 1f))
        {
            if (hitInfo.collider.CompareTag("Obstacle") || hitInfo.collider.CompareTag("TargetHelp"))
                return false;
        }
        return true;
    }

    void ApplySteering()
    {
        transform.position += direction.normalized * velocity * Time.fixedDeltaTime;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    void GetClosestAgent()
    {
        var AgentList = AgentSpawner.instance.Agents;

        Vector3 closestPos = Vector3.zero; 
        float closestDist = float.PositiveInfinity;

        foreach (var agent in AgentList)
        {
            if (agent == gameObject) continue;

            Vector3 eij = (transform.position - agent.transform.position);
            float sij = eij.magnitude;

            if (sij < 0.01f) continue;

            eij /= sij; 


            if (Vector3.Dot(desiredDirection, eij) >= 0) continue; 
            if (Mathf.Abs(Vector3.Dot(desiredDirTranspose, eij)) >= l / sij) continue;
             
            if (sij < closestDist)
            {
                closestPos = agent.transform.position; 
                closestDist = sij;
            }
        }
        
        s_i = closestDist;
        
        closestAgentPos = closestPos;
    }

    float GetAngle()
    {
        Vector3 eij = (transform.position - closestAgentPos).normalized;
        Vector3 eji = (closestAgentPos - transform.position).normalized;
        float s = Vector3.Distance(transform.position, closestAgentPos);

        float asin = Mathf.Asin(Mathf.Clamp(l / s, -1f, 1f));
        float acos = Mathf.Acos(Mathf.Clamp(Vector3.Dot(desiredDirection, eji), -1f, 1f));
        float gdot = g(Vector3.Dot(desiredDirTranspose, eij));

        float thetao = (asin - acos) * gdot;
        float thetas = -(asin + acos) * gdot;

        float ko = CalculateKvalues(thetao);
        float ks = CalculateKvalues(thetas);

        float deltaTheta = ko * thetao + (1 - ko) * ks * thetas;
        
        return deltaTheta; 
    }

    float g(float x)
    {
        if (Mathf.Abs(x) < 1e-4f) return 1;
        return Mathf.Sign(x);
    }

    float CalculateKvalues(float theta)
    {
        // Wall calculations. 
        Vector3 direction = RotateVector(desiredDirection, theta).normalized;
        Vector3 pdir = Vector3.Cross(direction, Vector3.up).normalized;
        Vector3 leftStart = transform.position + pdir * (l * 0.45f) + Vector3.up * 0.35f;
        Vector3 rightStart = transform.position - pdir * (l * 0.45f) + Vector3.up * 0.35f;

        float swl = distanceToObject(leftStart, direction);
        float swr = distanceToObject(rightStart, direction);

        float sw = -1f;
        if (swl < 0f && swr < 0f) return 1;
        else if (swl > 0f && swr > 0f) sw = Mathf.Min(swl, swr);
        else if (swl > 0f) sw = swl;
        else if (swr > 0f) sw = swr;

        return (sw - l / 2f > freeSpeed * Time.fixedDeltaTime) ? 1 : 0; 
    }

    float distanceToObject(Vector3 origin, Vector3 direction)
    {

        RaycastHit hitInfo;
        if (Physics.Raycast(origin, direction, out hitInfo, freeSpeed * 0.1f + l / 2f))
        {
            if (hitInfo.collider.CompareTag("Obstacle") || hitInfo.collider.CompareTag("Sheep"))
                return hitInfo.distance;
            
        }
        return -1f;
    }

    Vector3 RotateVector(Vector3 vec, float angle)
    {
        Vector3 rotated = new Vector3(
          Mathf.Cos(angle) * vec.x - Mathf.Sin(angle) * vec.z,
          0,
          Mathf.Sin(angle) * vec.x + Mathf.Cos(angle) * vec.z);

        return rotated;
    }

    bool ForwardAble()
    {
        Vector3 pdir = Vector3.Cross(direction, Vector3.up).normalized;
        Vector3 leftStart = transform.position + pdir * (l * 0.45f) + Vector3.up * 0.35f;
        Vector3 rightStart = transform.position - pdir * (l * 0.45f) + Vector3.up * 0.35f;

        RaycastHit hitInfo;
        if (Physics.Raycast(leftStart, direction, out hitInfo, freeSpeed * 0.1f + l / 2f))
        {
            if (hitInfo.collider.CompareTag("Obstacle") || hitInfo.collider.CompareTag("Sheep"))
                return false;

        }
        if (Physics.Raycast(rightStart, direction, out hitInfo, freeSpeed * 0.1f + l / 2f))
        {
            if (hitInfo.collider.CompareTag("Obstacle") || hitInfo.collider.CompareTag("Sheep"))
                return false;

        }
        return true;
    }

    float AngleToClosest()
    {
        Vector3 ToClosest = closestAgentPos - transform.position; 
        return Vector3.Angle(direction, ToClosest);
    }

    float CalculateVelocity()
    {
         

        float siw = distanceToObject(transform.position, direction);
        float vw = freeSpeed;

        if (siw > 0)
            vw = Mathf.Min(freeSpeed, siw / T);

        float vp = Mathf.Min(vw, Mathf.Max(0.05f, (s_i - l) / T));

        if (vp < 0.1f && AngleToClosest() > 80f && ForwardAble())
        {
            return vw; 
        }

        return vp; 
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(transform.position, transform.position + desiredDirection * 2f);

        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(transform.position, transform.position + desiredDirTranspose * 2f);
    }
}
