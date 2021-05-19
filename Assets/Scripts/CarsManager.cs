using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarsManager : MonoBehaviour
{
    public static CarsManager Instance = null;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }


    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform genParent;

    private int[] layers = new int[] { 5, 3, 1 };

    [SerializeField] private int populationSize = 50;
    [SerializeField] [Range(0.0001f, 1f)] private float MutationChance = 0.01f;
    [SerializeField] [Range(0f, 1f)] private float MutationStrength = 0.5f;

    [SerializeField] [Range(0.1f, 20f)] private float timeScale = 1f;

    public List<NeuralNetwork> Networks { get; private set; }
    public List<CarController> Cars { get; private set; }

    [SerializeField] private RectTransform ann_visual;
    public RectTransform Ann_visual { get { return ann_visual; } }
    [SerializeField] private Image neuronsImage;
    public Image NeuronsImage { get { return neuronsImage; } }
    [SerializeField] private Image weightsImage;
    public Image WeightsImage { get { return weightsImage; } }


    private void Start()
    {
        populationSize = populationSize / 2 * 2;    //Makes the population size even (eg. 9 / 2 * 2 = 8) (Needs to be even for evolution)

        InitNetworks();
        CreateCars();
    }


    private void Update()
    {
        Time.timeScale = timeScale;

        if (Input.GetKeyDown(KeyCode.R))
        {
            CreateCars();
        }

        GetHighestFitnessCar().Network.Visualize();
    }

    public CarController GetHighestFitnessCar(float threshold = 0f)
    {
        float highestFitness = float.MinValue;
        int highestFitnessIndex = -1;

        for (int i = 0; i < Networks.Count; i++)
        {
            if (Networks[i].Fitness >= highestFitness + threshold)
            {
                highestFitness = Networks[i].Fitness;
                highestFitnessIndex = i;
            }
        }

        return CarsManager.Instance.Cars[highestFitnessIndex];
    }


    public System.Collections.IEnumerator NextGeneration()
    {
        for (int i = 0; i < genParent.childCount; i++)
        {
            if (genParent.GetChild(i).GetComponent<CarController>().Collided == false)
            {
                yield break;
            }
        }

        yield return new WaitForSeconds(2.0f);

        CreateCars();
    }


    public void InitNetworks()
    {
        Networks = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Load("Assets/Data/start.txt");
            //net.Load("Assets/Data/trained.txt"); 
            Networks.Add(net);
        }
    }

    public void CreateCars()
    {
        if (Cars != null)
        {
            for (int i = 0; i < Cars.Count; i++)
            {
                GameObject.Destroy(Cars[i].gameObject);
            }

            MutateNetworks();
        }

        Cars = new List<CarController>();
        for (int i = 0; i < populationSize; i++)
        {
            CarController car = Instantiate(prefab, new Vector3(0.0f, 0.4f, 0.0f), Quaternion.identity, genParent).GetComponent<CarController>();
            car.Network = Networks[i];
            Cars.Add(car);
        }
    }

    public void MutateNetworks()
    {
        for (int i = 0; i < populationSize; i++)
            Cars[i].SetFitness();
        Networks.Sort();

        Networks[populationSize - 1].Save("Assets/Data/trained.txt");

        for (int i = 0; i < populationSize / 2; i++)
        {
            Networks[i] = Networks[i + populationSize / 2].Copy(new NeuralNetwork(layers));
            Networks[i].Mutate(MutationChance, MutationStrength);
        }
    }

}
