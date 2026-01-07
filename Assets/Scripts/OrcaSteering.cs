using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class OrcaSteering : MonoBehaviour
{

    public Vector2 position;
    public Vector2 velocity;
    public GameObject target;
    public Vector2 entrance;
    public float radius = 0.36f;
    public Vector2 preferredVelocity; 
    public float maxSpeed = 1.2f;



    struct OrcaLine
    {
        public Vector2 point;
        public Vector2 normal;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        entrance = new Vector2 (5, 10.5f);
        velocity = Vector2.zero; 
    }

    private void FixedUpdate()
    {
        position = new Vector2 (transform.position.x, transform.position.z);
        Finished(); 
        CalculateVelocity();
        var lines = ComputeOrcaLines();

        if (lines.Count == 0)
        {
            velocity = preferredVelocity; 
        }
        else
        {
            velocity = SolveLinearProgram(lines);
        }
            

        position += Vector2.ClampMagnitude(velocity, maxSpeed) * Time.fixedDeltaTime;
        transform.position = new Vector3(position.x, transform.position.y, position.y);
        transform.rotation = Quaternion.LookRotation(new Vector3(velocity.x, 0f, velocity.y), Vector3.up);

    }

    void CalculateVelocity()
    {
        Vector3 toTarget;
        if (targetVisible())
        {
            Vector2 target2D = new Vector2(target.transform.position.x, target.transform.position.z);
            toTarget = target2D - position;
        }
        else
        {
            toTarget = entrance - position;
        }

        preferredVelocity = Vector2.ClampMagnitude(toTarget, maxSpeed); 
        Debug.DrawRay(transform.position, new Vector3 (preferredVelocity.x, 0f, preferredVelocity.y), Color.yellow);
    }

    List<OrcaSteering> GetAgents()
    {
        List<OrcaSteering> orcas = new(); 
        var agents = AgentSpawner.instance.Agents;
        foreach (var agent in agents)
        {
            orcas.Add(agent.GetComponent<OrcaSteering>()); 
        }

        return orcas; 
    }

    void Finished()
    {
        if (transform.position.z > 20f)
        {
            AgentSpawner.instance.Agents.Remove(gameObject);
            Destroy(gameObject);
        }
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

    List<OrcaLine> ComputeOrcaLines()
    {
        List<OrcaLine> lines = new(); 

        var agents = GetAgents();
        float timeHorizon = 5f; 

        foreach (var other in agents)
        {
            if (other == this) continue;

            Vector2 p_ab = other.position - position;
            Vector2 v_ab = velocity - other.velocity;
            float dist = p_ab.magnitude;
            float combinedRadius = radius + other.radius;

            OrcaLine line;
            Vector2 u;
            Vector2 normal;

            if (dist > maxSpeed * 2f || Vector2.Dot(p_ab, velocity) < 0)
                continue;

            u = p_ab * (p_ab.magnitude - maxSpeed);
            normal = p_ab.normalized;
            line.point = u;
            line.normal = normal;
            lines.Add(line);
        }

        return lines;

    }

    Vector2 SolveLinearProgram(List<OrcaLine> lines)
    {
        Vector2 result = preferredVelocity;

        if (result.magnitude > maxSpeed)
            result = result.normalized * maxSpeed;

        float tMax = 0f; 

        for (int i = 0; i < lines.Count; i++)
        {
            float t = ProjectOntoLine(lines[i]);

            if (t > tMax)
                tMax = t;
        }

        if (tMax < 0f)
        {
            return preferredVelocity.normalized * maxSpeed; 
        }

        return preferredVelocity.normalized * Mathf.Clamp(tMax, 0f, maxSpeed);
    }

    float ProjectOntoLine(OrcaLine line)
    {
        float denom = Vector2.Dot(line.normal, preferredVelocity.normalized);

        if (Mathf.Abs(denom) < 1e-6f)
        {
            return -1f;
        }

        float t = Vector2.Dot(line.normal, line.point - position) / denom;
        return t;
    }

}
