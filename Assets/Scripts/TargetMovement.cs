using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    [SerializeField] float movementSpeed;
    [SerializeField] float changeDistance;
    [SerializeField] bool pause; 

    int dir; 
    Vector3 origin; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dir = 1; 
        origin = transform.position; 
    }

    // Update is called once per frame
    void Update()
    {
        Move(); 
    }

    void Move()
    {
        if (pause)
            return; 

        float dist = Vector3.Distance(origin, transform.position);
        if (dist > changeDistance)
        {
            dir *= -1; 
        }
        transform.position += transform.forward * Time.deltaTime * movementSpeed * dir;
    }
}
