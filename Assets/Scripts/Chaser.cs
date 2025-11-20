using UnityEngine;
using UnityEngine.InputSystem;
using Ursaanimation.CubicFarmAnimals; 

public class Chaser : MonoBehaviour
{
    [SerializeField] float movementSpeed;
    [SerializeField] float rotationSpeed; 

    AnimationController controller;
    Vector3 pastPosition;
    public float velocity; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<AnimationController>(); 
        pastPosition = transform.position;
        
    }

    // Update is called once per frame
    void Update()
    {
        Move(); 
    }

    void Move()
    {
        var kb = Keyboard.current;

        if (kb.wKey.isPressed)
            transform.position += transform.forward * Time.deltaTime * movementSpeed;

        if (kb.sKey.isPressed)
            transform.position -= transform.forward * Time.deltaTime * movementSpeed;

        if (kb.aKey.isPressed)
            transform.rotation *= Quaternion.Euler(0f, -rotationSpeed * 10f * Time.deltaTime, 0f);

        if (kb.dKey.isPressed)
            transform.rotation *= Quaternion.Euler(0f, rotationSpeed * 10f * Time.deltaTime, 0f);

        velocity = (pastPosition - transform.position).magnitude / Time.deltaTime; 
        pastPosition = transform.position;

        if (velocity > 1f)
            controller.animator.Play(controller.runForwardAnimation);
        else
            controller.animator.Play(controller.idle);

    }
}
