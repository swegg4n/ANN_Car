using UnityEngine;
using UnityEditor;
using System.Diagnostics;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    private Transform[] sensors;
    private const float SENSOR_RANGE = 5.0f;
    [SerializeField] private LayerMask sensorMask;

    private const float MAX_MOVE_SPEED_FWD = 0.05f;
    private const float MAX_MOVE_SPEED_BACK = 0.02f;
    private const float MAX_TURN_SPEED = 1.5f;
    private Rigidbody rb;


    public NeuralNetwork Network { get; set; }

    public int checkpoints { private get; set; } = 1;
    private Stopwatch stopWatch = new Stopwatch();

    public bool Collided { get; private set; } = false;



    private void Awake()
    {
        sensors = new Transform[transform.childCount - 1];
        for (int i = 0; i < sensors.Length; i++)
            sensors[i] = transform.GetChild(i);

        rb = GetComponent<Rigidbody>();

        stopWatch.Restart();
    }


    public float[] GetSensorValues()
    {
        float[] sensorValues = new float[sensors.Length];
        for (int i = 0; i < sensors.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(sensors[i].position, sensors[i].forward, out hit, SENSOR_RANGE, sensorMask))
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
        Network.Fitness = checkpoints * (float)stopWatch.Elapsed.TotalSeconds;
    }


    private void FixedUpdate()
    {
        if (!Collided)
        {
            float[] output = Network.FeedForward(GetSensorValues());

            Move(1.0f);
            Turn(1.0f, output[0]);

            SetFitness();
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("CheckPoint"))
        {
            ++checkpoints;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer != LayerMask.NameToLayer("Ground"))
        {
            Collided = true;
            StartCoroutine(CarsManager.Instance.NextGeneration());
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

            if (DebugManager.Instance.DebugFitness)
                Handles.Label(transform.position, $"Fitness: {Network.Fitness}");
        }
        catch (System.Exception) { }

    }

}
