using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float offsetDistance = 1.0f;

    private void LateUpdate()
    {
        CarController carToFollow = CarsManager.Instance.GetHighestFitnessCar(0.1f);

        Vector3 offset = carToFollow.GetComponent<Rigidbody>().velocity * offsetDistance;
        Vector3 target = new Vector3(carToFollow.transform.position.x, transform.position.y, carToFollow.transform.position.z) + offset;

        transform.position = Vector3.Lerp(transform.position, target, 0.025f * Time.timeScale);
    }

}
