using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    private Transform[] sensors = new Transform[5];
    private const float SENSOR_RANGE = 5.0f;

    private const float MOVE_SPEED = 0.05f;
    private const float MAX_TURN_SPEED = 1.0f;
    private Rigidbody rb;


    private void Awake()
    {
        for (int i = 0; i < sensors.Length; i++)
            sensors[i] = transform.GetChild(i);

        rb = GetComponent<Rigidbody>();
    }


    public float[] SensorRanges()
    {
        float[] sensorRanges = new float[sensors.Length];
        for (int i = 0; i < sensors.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(sensors[i].position, sensors[i].forward, out hit, SENSOR_RANGE))
            {
                sensorRanges[i] = SENSOR_RANGE - hit.distance;
            }
            else
            {
                sensorRanges[i] = -1.0f;
            }
        }

        return sensorRanges;
    }


    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        rb.MovePosition(transform.position + transform.forward * MOVE_SPEED);
    }

    public void Turn(float turning)
    {
        rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(turning * MAX_TURN_SPEED, Vector3.up));
    }



    private void OnDrawGizmos()
    {
        try
        {
            float[] sensorRanges = SensorRanges();

            for (int i = 0; i < sensors.Length; i++)
            {
                if (sensorRanges[i] == -1.0f)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = new Color(1.0f, (SENSOR_RANGE - sensorRanges[i]) / (SENSOR_RANGE * 2.0f), 0.0f);
                }

                float rayLength = (sensorRanges[i] == -1.0f) ? SENSOR_RANGE : SENSOR_RANGE - sensorRanges[i];
                Gizmos.DrawLine(sensors[i].position, sensors[i].position + sensors[i].forward * rayLength);
            }
        }
        catch (System.Exception) { }

    }

}
