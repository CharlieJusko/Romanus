using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Gate : MonoBehaviour
{
    public Door[] gateDoors;
    public float openingSpeed = 1.5f;
    public bool trigger = false;

    private float timeElapsed = 0;


    private void FixedUpdate()
    {
        if(trigger)
            Open();
    }

    public void Open()
    {
        if(timeElapsed < openingSpeed)
        {
            float t = timeElapsed / openingSpeed;
            t = t * t * (3f - 1f * t);
            foreach(Door door in gateDoors)
            {
                door.Lerp(t);
            }
            timeElapsed += Time.deltaTime;
        }
    }
}

public enum Movement 
{
    Translation,
    Rotation
}

public enum Axis 
{ 
    x, 
    y, 
    z
}



[Serializable]
public class Door
{
    public Transform door;
    public Movement movement;
    public Axis localAxis;
    public float openValue;
    public float closedValue;
    public bool open;
    public bool Open { get { return open; } set { open = value; } }

    public void Lerp(float t)
    {
        float startingValue = Open ? openValue : closedValue;
        float targetValue = !Open ? openValue : closedValue;

        if (movement == Movement.Translation) 
        {
            float value = Mathf.Lerp(startingValue, targetValue, t);
            Vector3 position = door.localPosition;

            if(localAxis == Axis.x)
                position.x = value;

            else if(localAxis == Axis.y)
                position.y = value;

            else if(localAxis == Axis.z)
                position.z = value;

            door.localPosition = position;
        }
        else if(movement == Movement.Rotation)
        {
            float value = Mathf.Lerp(startingValue, targetValue, t);
            Vector3 euler = door.localEulerAngles;

            if(localAxis == Axis.x)
                euler.x = value;

            else if(localAxis == Axis.y)
                euler.y = value;

            else if(localAxis == Axis.z)
                euler.z = value;

            door.localEulerAngles = euler;
        }
    }
}
