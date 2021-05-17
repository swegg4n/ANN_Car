using System.Collections.Generic;
using UnityEngine;

public class CarsManager : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    private int[] layers = new int[] { 8, 6, 3, 2 };

    [SerializeField] private int populationSize = 50;
    [SerializeField] [Range(0.0001f, 1f)] private float MutationChance = 0.01f;
    [SerializeField] [Range(0f, 1f)] private float MutationStrength = 0.5f;

    [SerializeField] [Range(0.1f, 10f)] private float timeScale = 1f;

    private List<NeuralNetwork> networks;
    private List<CarController> cars;



    private void Start()
    {
        Time.timeScale = timeScale;

        populationSize = populationSize / 2 * 2;    //Makes the population size even (eg. 9 / 2 * 2 = 8)

        InitNetworks();
        CreateCars();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            CreateCars();
        }
    }


    public void InitNetworks()
    {
        networks = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Load("Assets/Data/start.txt");
            networks.Add(net);
        }
    }

    public void CreateCars()
    {
        if (cars != null)
        {
            for (int i = 0; i < cars.Count; i++)
            {
                GameObject.Destroy(cars[i].gameObject);
            }

            MutateNetworks();
        }

        cars = new List<CarController>();
        for (int i = 0; i < populationSize; i++)
        {
            CarController car = Instantiate(prefab, new Vector3(0.0f, 0.4f, 0.0f), Quaternion.identity).GetComponent<CarController>();
            car.Network = networks[i];
            cars.Add(car);
        }
    }

    public void MutateNetworks()
    {
        for (int i = 0; i < populationSize; i++)
            cars[i].SetFitness();
        networks.Sort();

        networks[populationSize - 1].Save("Assets/Data/trained.txt");

        for (int i = 0; i < populationSize / 2; i++)
        {
            networks[i] = networks[i + populationSize / 2].Copy(new NeuralNetwork(layers));
            networks[i].Mutate((int)(1 / MutationChance), MutationStrength);
        }
    }

}
