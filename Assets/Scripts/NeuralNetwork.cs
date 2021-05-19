using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class NeuralNetwork : System.IComparable<NeuralNetwork>
{
    private int[] layers;
    private float[][] neurons;
    private float[][] biases;
    private float[][][] weights;

    public float Fitness { get; set; } = 0.0f;

    private const float e = 2.71828f;



    public NeuralNetwork(int[] layers)
    {
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }

        InitNeurons();
        InitBiases();
        InitWeights();
    }


    private void InitNeurons()
    {
        List<float[]> neuronsList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            neuronsList.Add(new float[layers[i]]);
        }
        neurons = neuronsList.ToArray();
    }

    private void InitBiases()
    {
        List<float[]> biasList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            float[] bias = new float[layers[i]];
            for (int j = 0; j < layers[i]; j++)
            {
                bias[j] = Random.Range(-0.5f, 0.5f);
            }
            biasList.Add(bias);
        }
        biases = biasList.ToArray();
    }

    private void InitWeights()
    {
        List<float[][]> weightsList = new List<float[][]>();
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>();
            int neuronsInPreviousLayer = layers[i - 1];
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer];
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    neuronWeights[k] = Random.Range(-0.5f, 0.5f);
                }
                layerWeightsList.Add(neuronWeights);
            }
            weightsList.Add(layerWeightsList.ToArray());
        }
        weights = weightsList.ToArray();
    }



    public float Activate(float x)
    {
        return (float)System.Math.Tanh(x);
    }


    public float[] FeedForward(float[] inputs)
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }
        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0f;
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }
                neurons[i][j] = Activate(value + biases[i][j]);
            }
        }
        return neurons[neurons.Length - 1];
    }


    public void Mutate(float chance, float val)
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                biases[i][j] = (Random.Range(0f, 1.0f) <= chance) ? biases[i][j] += Random.Range(-val, val) : biases[i][j];
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = (Random.Range(0.0f, 1.0f) <= chance) ? weights[i][j][k] += Random.Range(-val, val) : weights[i][j][k];
                }
            }
        }
    }


    public int CompareTo(NeuralNetwork other)
    {
        if (other == null) return 1;

        if (Fitness > other.Fitness)
            return 1;
        else if (Fitness < other.Fitness)
            return -1;
        else
            return 0;
    }

    public NeuralNetwork Copy(NeuralNetwork nn)
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                nn.biases[i][j] = biases[i][j];
            }
        }
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    nn.weights[i][j][k] = weights[i][j][k];
                }
            }
        }
        return nn;
    }


    public void Load(string path)
    {
        TextReader tr = new StreamReader(path);
        int NumberOfLines = (int)new FileInfo(path).Length;
        string[] ListLines = new string[NumberOfLines];
        int index = 1;
        for (int i = 1; i < NumberOfLines; i++)
        {
            ListLines[i] = tr.ReadLine();
        }
        tr.Close();
        if (new FileInfo(path).Length > 0)
        {
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    biases[i][j] = float.Parse(ListLines[index]);
                    index++;
                }
            }
            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        weights[i][j][k] = float.Parse(ListLines[index]);
                        index++;
                    }
                }
            }
        }
    }

    public void Save(string path)//this is used for saving the biases and weights within the network to a file.
    {
        File.Create(path).Close();
        StreamWriter writer = new StreamWriter(path, true);

        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                writer.WriteLine(biases[i][j]);
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    writer.WriteLine(weights[i][j][k]);
                }
            }
        }
        writer.Close();
    }


    public void Visualize()
    {
        RectTransform ann_visual = CarsManager.Instance.Ann_visual;

        Image neuronsImage = CarsManager.Instance.NeuronsImage;
        Image weightsImage = CarsManager.Instance.WeightsImage;

        Vector2[][] neuronPositions = new Vector2[neurons.Length][];

        if (ann_visual.childCount == 0)
        {
            for (int n1 = 0; n1 < neurons.Length; n1++)
            {
                neuronPositions[n1] = new Vector2[neurons[n1].Length];
                for (int n2 = 0; n2 < neurons[n1].Length; n2++)
                {
                    Vector2 position = new Vector2(
                        ann_visual.rect.xMin + ann_visual.rect.width * (n1 + 1) / (neurons.Length + 1),
                        ann_visual.rect.yMax - ann_visual.rect.height * (n2 + 1) / (neurons[n1].Length + 1))
                        + new Vector2(ann_visual.position.x, ann_visual.position.y);

                    neuronPositions[n1][n2] = position;

                    GameObject.Instantiate(neuronsImage, position, Quaternion.identity, ann_visual);
                }
            }
            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        Vector2 fromNeuron = neuronPositions[i + 1][j];
                        Vector2 toNeuron = neuronPositions[i][k];

                        Vector2 position = fromNeuron / 2.0f + toNeuron / 2.0f;

                        Transform t = GameObject.Instantiate(weightsImage, position, Quaternion.identity, ann_visual).GetComponent<RectTransform>();

                        Vector2 fromToNeuron = toNeuron - fromNeuron;
                        float rotation = Mathf.Atan2(fromToNeuron.y, fromToNeuron.x);

                        float rt_width = fromNeuron.magnitude;
                        float rt_height = 5;
                        t.localScale = new Vector2(
                            rt_width / weightsImage.rectTransform.rect.width,
                            rt_height / weightsImage.rectTransform.rect.height);

                        t.rotation = Quaternion.Euler(0f, 0f, rotation);
                    }
                }
            }
        }

        int c = 0;
        for (int n1 = 0; n1 < neurons.Length; n1++)
        {
            for (int n2 = 0; n2 < neurons[n1].Length; n2++, c++)
            {
                ann_visual.GetChild(c).GetComponent<Image>().color = ValueToColor(neurons[n1][n2]);
            }
        }
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++, c++)
                {
                    ann_visual.GetChild(c).GetComponent<Image>().color = ValueToColor(weights[i][j][k]);
                }
            }
        }
    }

    //Black: (0,0,0)  Green: (0,1,0)  Red: (1,0,0)
    private Color ValueToColor(float value)
    {
        if (value >= 0f) //Green [0,1]
        {
            return new Color(0f, value, 0f);
        }
        else //Red [-1,0[
        {
            return new Color(-value, 0f, 0f);
        }
    }

}
