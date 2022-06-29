using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetIDS
{
    [Serializable]
    class Layer
    {
        private int nInputs;
        private int nOutputs;
        private double[] inputs;
        private double[] biases;
        private double[] outputs;
        private double[,] weights;

        public Layer( int numIn, int numOut, Random ram)
        {
            inputs = new double[numIn];
            biases = new double[numIn];
            weights = new double[numIn, numOut];

            for( int i = 0; i < numIn; i++)
            {
                biases[i] = ram.NextDouble();

                for( int j = 0; j < numOut; j++)
                {
                    weights[i, j] = ram.NextDouble();
                }
            }

            nInputs = numIn;
            nOutputs = numOut;
        }

        public double[] Fire(double[] input)
        {
            input.CopyTo(inputs, 0);
            outputs = new double[nOutputs];

            for (int i = 0; i < nInputs; i++)
            {
                for (int j = 0; j < nOutputs; j++)
                {
                    outputs[j] += inputs[i];
                }
            }

            return outputs;
        }

        public double[] Calculate(double[] inner)
        {
            inner.CopyTo(inputs, 0);
            outputs = new double[nOutputs];

            for ( int i = 0; i < nInputs; i++)
            {
                for( int j = 0; j < nOutputs; j++)
                {
                    outputs[j] += inputs[i] * weights[i, j] + biases[i];
                }
            }

            for( int i = 0; i < nOutputs; i++)
            {
                outputs[i] = Sigmoid(outputs[i]);
            }

            return outputs;
        }

        public void CalculateGradients(double[] error, double[] res)
        {
            Array.Clear(res, 0, res.Length);

            for( int i = 0; i < nInputs; i++)
            {
                for( int j = 0; j < nOutputs; j++)
                {
                    res[i] += DerivativeSigmoid(outputs[j]) * error[j] * weights[i, j];
                }
            }
        }

        public void CalculateWeightDeltas( double[] gradient, double[,] deltas, double lrate )
        {
            for ( int i = 0; i < nInputs; i++)
            {
                for( int j = 0; j < nOutputs; j++)
                {
                    deltas[i, j] = lrate * gradient[j] * inputs[i];
                }
            }
        }

        public void CalculateBiasDeltas( double[] gradient, double[] deltas, double lrate)
        {
            for( int i = 0; i < nInputs; i++)
            {
                deltas[i] = lrate * gradient[i];
            }
        }

        public void Update(double[,] weightDeltas, double[,] oldWeightDeltas, double[] biasDeltas, double[] oldBiasDeltas, double momentum)
        {
            for( int i = 0; i < nInputs; i++)
            {
                biases[i] += biasDeltas[i] + oldBiasDeltas[i] * momentum;
                oldBiasDeltas[i] = biasDeltas[i];

                for ( int j = 0; j < nOutputs; j++)
                {
                    weights[i, j] += weightDeltas[i, j] + oldWeightDeltas[i, j] * momentum;
                    oldWeightDeltas[i, j] = weightDeltas[i, j];
                }
            }
        }

        private double Sigmoid( double sum )
        {
            return 1 / (1 + Math.Exp(-sum));
        }

        private double DerivativeSigmoid( double sum)
        {
            return sum * 1 - sum;
        }


        public override string ToString()
        {
            string resStr = $"Number of nodes in layer: {nInputs}\n";
            resStr += $"Number of Outputs: {nOutputs}\n\n";

            return resStr;
        }
    }
}
