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

    private int[] layers = new int[] { 5, 4, 2 };

    [SerializeField] private int populationSize = 50;
    [SerializeField] [Range(0.0001f, 1f)] private float MutationChance = 0.01f;
    [SerializeField] [Range(0f, 1f)] private float MutationStrength = 0.5f;

    [SerializeField] [Range(0.1f, 50.0f)] private float timeScale = 1.0f;

    public List<NeuralNetwork> Networks { get; private set; }
    public List<CarController> Cars { get; private set; }

    [SerializeField] private RectTransform ann_visual;
    public RectTransform Ann_visual { get { return ann_visual; } }
    [SerializeField] private Image neuronsImage;
    public Image NeuronsImage { get { return neuronsImage; } }
    [SerializeField] private Image weightsImage;
    public Image WeightsImage { get { return weightsImage; } }

    [SerializeField] private Text stats;

    private float absoluteBestFitness = 0f;


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

        NeuralNetwork bestNetwork = GetHighestFitnessCar(0.1f, true).Network;
        bestNetwork.Visualize();
        UpdateStatistics(bestNetwork.Generation, bestNetwork.Fitness);
    }

    public CarController GetHighestFitnessCar(float threshold = 0f, bool ignoreDead = false)
    {
        float highestFitness = float.MinValue;
        int highestFitnessIndex = 0;

        for (int i = 0; i < Networks.Count; i++)
        {
            if (Networks[i].Fitness >= highestFitness + threshold)
            {
                if (ignoreDead && CarsManager.Instance.Cars[i].Collided)    //If this car has collided and collided cars should be ignored -> continue
                    continue;

                highestFitness = Networks[i].Fitness;
                highestFitnessIndex = i;
            }
        }

        return CarsManager.Instance.Cars[highestFitnessIndex];
    }

    private void UpdateStatistics(int gen, float highestFitness)
    {
        float avgFitness = 0;
        for (int i = 0; i < Networks.Count; i++)
            avgFitness += Networks[i].Fitness / Networks.Count;

        if (highestFitness > absoluteBestFitness)
            absoluteBestFitness = highestFitness;

        stats.text = $"Generation: \t{gen}\n" +
                     $"Highest Fitness: \t{highestFitness}\n" +
                     $"\n" +
                     $"Absolute Highest Fitness: \t{absoluteBestFitness}";
    }


    public System.Collections.IEnumerator NextGeneration(bool ignoreAlive = false)
    {
        if (ignoreAlive == false)
        {
            for (int i = 0; i < genParent.childCount; i++)
            {
                if (genParent.GetChild(i).GetComponent<CarController>().Collided == false)
                {
                    yield break;
                }
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
            car.Network.Generation++;
            Cars.Add(car);
        }
    }

    public void MutateNetworks()
    {
        for (int i = 0; i < populationSize; i++)
            Cars[i].SetFitness();
        Networks.Sort();

        Networks[populationSize - 1].Save("Assets/Data/trained.txt");   //Saves the best network to file

        for (int i = 0; i < populationSize / 2; i++)    //Mutates the worst half of the generation
        {
            Networks[i] = Networks[i + populationSize / 2].Copy(new NeuralNetwork(layers));
            Networks[i].Mutate(MutationChance, MutationStrength);
        }
    }

}
