using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetIDS
{
    [Serializable]
    class Network
    {
        private string banner = "\n================Neural Net IDS================\n\n";
        private Layer input;
        private Layer hidden;
        private Layer output;
        private int numIn;
        private int numHid;
        private int numOut;

        public Network( int numInput, int numHidden, int numOutput)
        {
            Random ram = new Random();
            input = new Layer(numInput, numHidden, ram);
            hidden = new Layer(numHidden, numOutput, ram);
            output = new Layer(numOutput, numOutput, ram);
            numIn = numInput;
            numHid = numHidden;
            numOut = numOutput;
        }

        public void Train( List<Packet> packets, double[] vals, bool classifier, bool show)
        {
            double learnRate = vals[0];
            double momentum = vals[1];

            double[] outGrads = new double[numOut];
            double[,] outWD = new double[numOut, numOut];
            double[,] outOWD = new double[numOut, numOut];
            double[] outBD = new double[numOut];
            double[] outOBD = new double[numOut];
            double[] hidGrads = new double[numHid];
            double[,] hidWD = new double[numHid, numOut];
            double[,] hidOWD = new double[numHid, numOut];
            double[] hidBD = new double[numHid];
            double[] hidOBD = new double[numHid];

            int[] predictions;

            if (classifier)
            {
                predictions = new int[6];
            }
            else
            {
                predictions = new int[2];
            }

            Packet p;
            List<double> grades = new List<double>();
            for(int epoch = 0;  epoch < 5000; epoch++)
            {
                for (int index = 0; index < packets.Count - 1; index++)
                {
                    p = packets[index];
                    double[] inputs = p.GetInputs();
                    double[] desired;

                    if (!classifier)
                    {
                        desired = p.CheckAnomaly();
                    }
                    else
                    {
                        desired = p.CheckMisuse();
                    }

                    double[] res = output.Calculate(hidden.Calculate(input.Fire(inputs)));

                    for (int i = 0; i < numOut; i++)
                    {
                        outGrads[i] = (desired[i] - res[i]) * (res[i] * (1 - res[i]));
                        grades.Add(outGrads[i]);
                    }

                    int find = 0;
                    double highest = 0;
                    for (int i = 0; i < res.Length; i++)
                    {
                        if (res[i] > highest)
                        {
                            find = i;
                            highest = res[i];
                        }
                    }

                    predictions[find]++;

                    hidden.CalculateGradients(outGrads, hidGrads);

                    output.CalculateWeightDeltas(outGrads, outWD, learnRate);
                    output.CalculateBiasDeltas(outGrads, outBD, learnRate);

                    hidden.CalculateWeightDeltas(hidGrads, hidWD, learnRate);
                    hidden.CalculateBiasDeltas(hidGrads, hidBD, learnRate);

                    hidden.Update(hidWD, hidOWD, hidBD, hidOBD, momentum);
                    output.Update(outWD, outOWD, outBD, outOBD, momentum);
                }
                int check = 0;
                foreach(double curr in grades)
                {
                    if(-0.05 < curr && curr < 0.05)
                    {
                        check++;
                    }
                }
                if(packets.Count() == check)
                {
                    Console.WriteLine($"|Done! Finished at epoch {epoch}");
                    break;
                }

                grades.Clear();

                if (show)
                {
                    if (classifier)
                    {
                        Console.WriteLine($"|Normal: {predictions[0]}, TearDrop: {predictions[1]}, " +
                            $"BackDoor: {predictions[2]}, Smurf: {predictions[3]}, " +
                            $"RootKit: {predictions[4]}, GuessPassword: {predictions[5]}");
                    }
                    else
                    {
                        Console.WriteLine($"|Normal: {predictions[0]}, Abnormal: {predictions[1]}");
                    }

                    Array.Clear(predictions, 0, predictions.Length);
                }
                packets = packets.OrderBy(a => Guid.NewGuid()).ToList<Packet>();
            }
        }

        public void Test(List<Packet> tester, bool classifier)
        {
            int[] predictions = new int[6];
            int[] actuals = new int[6];

            int predInd;
            int actInd;
            int correct = 0;

            foreach(Packet pack in tester)
            {
                predInd = Predict(output.Calculate(hidden.Calculate(input.Fire(pack.GetInputs()))));
                predictions[predInd]++;

                if (classifier)
                {
                    actInd = Predict(pack.CheckMisuse());
                }
                else
                {
                    actInd = Predict(pack.CheckAnomaly());
                }

                actuals[actInd]++;

                if(predInd == actInd)
                {
                    correct++;
                }
            }

            CalculateStats(predictions, actuals, correct, classifier);
        }

        private int Predict(double[] results)
        {
            int index = 0;
            double highest = 0;

            for( int i = 0; i < results.Length; i++)
            {
                if(results[i] > highest)
                {
                    index = i;
                    highest = results[i];
                }
            }

            return index;
        }

        private void CalculateStats(int[] predictions, int[] actuals, int correct, bool classifier)
        {
            int vals;
            string[] labels;

            if (classifier)
            {
                vals = 6;
                labels = new string[] { "Normal", "TearDrop", "BackDoor", "Smurf", "RootKit", "GuessPassword"};
            }
            else
            {
                vals = 2;
                labels = new string[] { "Normal", "Abnormal" };
            }

            for(int i = 0; i < vals; i++)
            {
                Console.WriteLine($"|Predicted {labels[i]}: {predictions[i]}, Actual {labels[i]}: {actuals[i]}");
            }
            Console.WriteLine($"|Correct: {correct}");
        }
    }
}
