using UnityEngine;
using UnityEditor;
using System.Diagnostics;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    private Transform[] sensors;
    private const float SENSOR_RANGE = 5.0f;
    [SerializeField] private LayerMask sensorMask;

    [SerializeField] private Material aliveMaterial;
    [SerializeField] private Material deadMaterial;

    private const float MAX_MOVE_SPEED = 0.05f;
    private const float MAX_TURN_SPEED = 1.5f;
    private Rigidbody rb;


    public NeuralNetwork Network { get; set; }

    float timeAlive = 0f;
    float totalVelocity = 0f;
    int velocitySamples = 0;

    public bool Collided { get; private set; } = false;

    public bool FinishedCourse { get; private set; } = false;



    private void Awake()
    {
        sensors = new Transform[transform.childCount - 1];
        for (int i = 0; i < sensors.Length; i++)
            sensors[i] = transform.GetChild(i);

        GetComponent<MeshRenderer>().material = aliveMaterial;
        transform.GetChild(transform.childCount - 1).GetComponent<MeshRenderer>().material = aliveMaterial;

        rb = GetComponent<Rigidbody>();
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
        if (FinishedCourse)
        {
            Network.Fitness = 10000 - timeAlive + (totalVelocity / velocitySamples);
        }
        else
        {
            Network.Fitness = timeAlive + (totalVelocity / velocitySamples);
        }
    }


    private void FixedUpdate()
    {
        if (Network == null)
            return;

        if (!Collided)
        {
            float[] output = Network.FeedForward(GetSensorValues());
            output[0] = (output[0] + 1.0f) * 0.5f;  //Remaps output[0] from [-1,1] to [0,1]

            //output[0] = "speed"
            //output[1] = "turning"

            Move(output[0]);
            Turn(output[1], output[0]);

            timeAlive += Time.deltaTime;
            totalVelocity += rb.velocity.magnitude / Time.deltaTime;
            ++velocitySamples;

            SetFitness();

            if (timeAlive >= 5000.0f)
            {
                Kill();
            }
        }
    }

    private void Move(float speed)  //speed [0,1]
    {
        rb.MovePosition(transform.position + transform.forward * speed * MAX_MOVE_SPEED);
    }

    public void Turn(float turning, float speed)    //turning [-1,1]    speed [0,1]
    {
        rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(turning * MAX_TURN_SPEED * (1.0f - speed), Vector3.up));
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("CheckPoint"))  // The car finished the course
        {
            FinishedCourse = true;

            SetFitness();

            StartCoroutine(CarsManager.Instance.NextGeneration(true));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer != LayerMask.NameToLayer("Ground"))
        {
            Kill();
        }
    }

    private void Kill()
    {
        Collided = true;
        GetComponent<MeshRenderer>().material = deadMaterial;
        transform.GetChild(transform.childCount - 1).GetComponent<MeshRenderer>().material = deadMaterial;

        SetFitness();

        StartCoroutine(CarsManager.Instance.NextGeneration());
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
