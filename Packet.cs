using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetIDS
{
    class Packet
    {
        public double protocol; // Protocol used for the connection
        public double service;  // Service requested during the connection
        public double srcBytes; // Bytes sent from source to host
        public double destBytes;// Bytes sent from host to source
        public double land; // flag for if destination and source were the same
        public double wrongFragment; // Number of broken fragments sent in a package
        public double hot;  // Indicators for entering a directory, executing a file, general changing/accessing data
        public double failedLogins; // Count of failed login attempts over the connection
        public double loginStatus;  // Indicates if the source was logged in to the destination
        public double outCmds;  // Count of commands going from host to source
        public double count;    //Counter for how many recent connections have been made to the same host destination
        public double srvCount; // Counter for how many recent connections have been made to the same host port
        public double sameSrvRate; // Rate of connections to the same service + srvCount
        public double dstHostCount; // Counter for connections with same host IP address
        public double dstHostSameServ; // Rate of connections to the same service + dstHostCount

        List<double> inputs = new List<double>();
        double[] desAnom;
        double[] desMisu;

        public string classification;
        public Packet(string[] setup, string type)
        {
            // Grabs each feature from where it is in the string
            protocol = Convert.ToDouble(setup[1]);
            service = Convert.ToDouble(setup[2]);
            srcBytes = Convert.ToDouble(setup[4]);
            destBytes = Convert.ToDouble(setup[5]);
            land = Convert.ToDouble(setup[6]);
            wrongFragment = Convert.ToDouble(setup[7]);
            hot = Convert.ToDouble(setup[9]);
            failedLogins = Convert.ToDouble(setup[10]);
            loginStatus = Convert.ToDouble(setup[11]);
            outCmds = Convert.ToDouble(setup[19]);
            count = Convert.ToDouble(setup[22]);
            srvCount = Convert.ToDouble(setup[23]);
            sameSrvRate = Convert.ToDouble(setup[28]);
            dstHostCount = Convert.ToDouble(setup[31]);
            dstHostSameServ = Convert.ToDouble(33);

            inputs.Add(protocol);
            inputs.Add(service);
            inputs.Add(srcBytes);
            inputs.Add(destBytes);
            inputs.Add(land);
            inputs.Add(wrongFragment);
            inputs.Add(hot);
            inputs.Add(failedLogins);
            inputs.Add(loginStatus);
            inputs.Add(outCmds);
            inputs.Add(count);
            inputs.Add(srvCount);
            inputs.Add(sameSrvRate);
            inputs.Add(dstHostCount);
            inputs.Add(dstHostSameServ);

            classification = type;
            AssessOutputs();
        }

        public double[] GetInputs()
        {
            return inputs.ToArray();
        }

        public double[] CheckAnomaly()
        {
            return desAnom;
        }

        public double[] CheckMisuse()
        {
            return desMisu;
        }

        private void AssessOutputs()
        {
            switch (classification)
            {
                case "Normal":
                    desAnom = new double[2] { 1.0, 0.0};
                    desMisu = new double[6] { 1.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
                    break;
                case "TearDrop":
                    desAnom = new double[2] { 0.0, 1.0 };
                    desMisu = new double[6] { 0.0, 1.0, 0.0, 0.0, 0.0, 0.0 };
                    break;
                case "BackDoor":
                    desAnom = new double[2] { 0.0, 1.0 };
                    desMisu = new double[6] { 0.0, 0.0, 1.0, 0.0, 0.0, 0.0 };
                    break;
                case "Smurf":
                    desAnom = new double[2] { 0.0, 1.0 };
                    desMisu = new double[6] { 0.0, 0.0, 0.0, 1.0, 0.0, 0.0 };
                    break;
                case "RootKit":
                    desAnom = new double[2] { 0.0, 1.0 };
                    desMisu = new double[6] { 0.0, 0.0, 0.0, 0.0, 1.0, 0.0 };
                    break;
                case "GuessPassword":
                    desAnom = new double[2] { 0.0, 1.0 };
                    desMisu = new double[6] { 0.0, 0.0, 0.0, 0.0, 0.0, 1.0 };
                    break;
                default:
                    classification = "Normal";
                    desAnom = new double[2] { 0.0, 1.0 };
                    desMisu = new double[6] { 1.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
                    break;
            }
        }
    }
}
