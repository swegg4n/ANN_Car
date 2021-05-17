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


    public NeuralNetwork Network { private get; set; }

    public int checkpoints { private get; set; }

    private bool collided = false;



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

    public void SetFitness()
    {
        Network.Fitness = checkpoints;
    }


    private void FixedUpdate()
    {
        if (!collided)
        {
            float[] output = Network.FeedForward(GetSensorValues());

            Move(output[0]);
            Turn(output[0], output[1]);
        }
    }

    private void Move(float speed)
    {
        float maxMoveSpeed = (speed > 0) ? MAX_MOVE_SPEED_FWD : MAX_MOVE_SPEED_BACK;
        rb.MovePosition(transform.position + transform.forward * speed * maxMoveSpeed);
    }

    public void Turn(float speed, float turning)
    {
        rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(turning * MAX_TURN_SPEED * speed, Vector3.up));
    }



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("CheckPoint"))
        {
            ++checkpoints;
        }
        else if (collision.collider.gameObject.layer != LayerMask.NameToLayer("Ground"))
        {
            collided = true;
        }
    }



    private void OnDrawGizmos()
    {
        try
        {
            if (DebugManager.Instance.DebugRays || DebugManager.Instance.DebugDistances)
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


                    if (DebugManager.Instance.DebugRays)
                        Gizmos.DrawLine(sensors[i].position, hitPoint);

                    if (DebugManager.Instance.DebugDistances)
                        Handles.Label(hitPoint, sensorValues[i].ToString());
                }
            }
        }
        catch (System.Exception) { }

    }

}
