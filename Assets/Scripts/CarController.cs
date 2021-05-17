using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    private Transform[] sensors;
    private const float SENSOR_RANGE = 5.0f;

    private const float MAX_MOVE_SPEED_FWD = 0.05f;
    private const float MAX_MOVE_SPEED_BACK = 0.02f;
    private const float MAX_TURN_SPEED = 1.5f;
    private Rigidbody rb;


    [SerializeField] [Range(-1.0f, 1.0f)] private float speed = 0.0f;    //The car's speed [0,1]
    [SerializeField] [Range(-1.0f, 1.0f)] private float turning = 0.0f;  //The car's turning [-1,1]

    /// <summary>
    /// Property for setting this car's speed [0,1]
    /// </summary>
    public float Speed { set { speed = value; } }

    /// <summary>
    /// Property for setting this car's turning [-1,1]
    /// </summary>
    public float Turning { set { turning = value; } }



    private void Awake()
    {
        sensors = new Transform[transform.childCount];
        for (int i = 0; i < sensors.Length; i++)
            sensors[i] = transform.GetChild(i);

        rb = GetComponent<Rigidbody>();
    }


    public float[] GetSensorValues()
    {
        float[] sensorValues = new float[sensors.Length];
        for (int i = 0; i < sensors.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(sensors[i].position, sensors[i].forward, out hit, SENSOR_RANGE))
            {
                sensorValues[i] = 1.0f - hit.distance / SENSOR_RANGE;
            }
            else
            {
                sensorValues[i] = 0.0f;
            }
        }

        return sensorValues;
    }


    private void FixedUpdate()
    {
        Move(speed);
        Turn(turning);
    }

    private void Move(float speed)
    {
        float maxMoveSpeed = (speed > 0) ? MAX_MOVE_SPEED_FWD : MAX_MOVE_SPEED_BACK;
        rb.MovePosition(transform.position + transform.forward * speed * maxMoveSpeed);
    }

    public void Turn(float turning)
    {
        rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(turning * MAX_TURN_SPEED * speed, Vector3.up));
    }



    private void OnDrawGizmos()
    {
        try
        {
            float[] sensorValues = GetSensorValues();

            for (int i = 0; i < sensors.Length; i++)
            {
                if (sensorValues[i] == 0.0f)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = new Color(1.0f, 1.0f - sensorValues[i], 0.0f);
                }

                Vector3 hitPoint = sensors[i].position + sensors[i].forward * (1.0f - sensorValues[i]) * SENSOR_RANGE;
                Gizmos.DrawLine(sensors[i].position, hitPoint);

                Handles.Label(hitPoint, sensorValues[i].ToString());
            }
        }
        catch (System.Exception) { }

    }

}
