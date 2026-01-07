using UnityEngine;

public class SimpleVehicleModel
{
    public float mass;
    public Vector3 position;
    public Vector3 velocity;
    public float max_force;
    public float max_speed;
    public Vector3[] orientation; // 0: Forward, 1: Up, 2: Right 

    public SimpleVehicleModel(Vector3 origin) 
    {   
        mass = 1.0f;
        position = origin;
        velocity = Vector3.zero;
        max_force = 20f;
        max_speed = 2.0f;
        orientation = new Vector3[] { Vector3.forward, Vector3.up, Vector3.right };
    }
}
