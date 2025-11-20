using UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers;
using UnityEngine;
using UnityEngine.InputSystem; 

public class GeneralMovement : MonoBehaviour
{
    [SerializeField] float movementSpeed = 1.0f;
    [SerializeField] float rotationSpeed = 1.0f;
    [SerializeField] Transform target;
    [SerializeField] bool moveTowardsTarget;

    private void Update()
    {
        Move(); 
        MoveTowardsTarget();    
    }

    void Move()
    {
        if (moveTowardsTarget)
            return; 

        var kb = Keyboard.current; 

        if (kb.wKey.isPressed)
            transform.position += transform.forward * Time.deltaTime * movementSpeed;  

        if (kb.sKey.isPressed)
            transform.position -= transform.forward * Time.deltaTime * movementSpeed;

        if (kb.aKey.isPressed)
            transform.rotation *= Quaternion.Euler(0f, -rotationSpeed * 10f * Time.deltaTime, 0f);

        if (kb.dKey.isPressed)
            transform.rotation *= Quaternion.Euler(0f, rotationSpeed * 10f * Time.deltaTime, 0f);
    }

    void MoveTowardsTarget()
    {
        if (moveTowardsTarget)
        {
            // Get the direction to the target in 2D normalized. 
            var direction = target.position - transform.position;
            direction.y = 0f;
            direction = direction.normalized; 

            // Move towards target. 
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
            transform.position += transform.forward * Time.deltaTime * movementSpeed;
        }   
    }
}


