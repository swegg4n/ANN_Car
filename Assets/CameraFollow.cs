using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float offsetDistance = 1.0f;

    private void LateUpdate()
    {
        float highestFitness = float.MinValue;
        int highestFitnessIndex = -1;

        for (int i = 0; i < CarsManager.Instance.Networks.Count; i++)
        {
            if (CarsManager.Instance.Networks[i].Fitness >= highestFitness)
            {
                highestFitness = CarsManager.Instance.Networks[i].Fitness;
                highestFitnessIndex = i;
            }
        }

        CarController carToFollow = CarsManager.Instance.Cars[highestFitnessIndex];

        Vector3 offset = carToFollow.GetComponent<Rigidbody>().velocity * offsetDistance;
        Vector3 target = new Vector3(carToFollow.transform.position.x, transform.position.y, carToFollow.transform.position.z) + offset;

        transform.position = Vector3.Lerp(transform.position, target, 0.025f * Time.timeScale);
    }

    
}
