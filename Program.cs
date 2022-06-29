using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;

namespace NeuralNetIDS
{
    class Program
    {
        private enum Mode{
            Train, Test, Quit, Null
        }

        private enum Classifier
        {
            Misuse, Anomaly, Quit, Null
        }

        static string BANNER = "|============================================|\n" +
                               "|===============Neural Net IDS===============|\n" +
                               "|============================================|";
        static string MODE_MESSAGE = "|Select Mode to Use: Train(TR) or Test(TS)\n|";
        static string MODE_TRAIN = "|Training mode selected...";
        static string MODE_TEST = "|Testing mode selected...";
        static string CLASS_MESSAGE = "|Select Classifier to Use: Anomaly (A) or Misuse (M)\n|";
        static string CLASS_ANOM = "|Anomaly classifier selected...";
        static string CLASS_MISU = "|Misuse classifier selected...";
        static string TRAINING = "|Generating training set...";
        static string TRAIN_INIT = "|Neural Net initialized...\n|Would you like to view output statistics? (Y or N)\n|";
        static string TESTING = "|Generating testing set...";
        static string FIN = "|OPERATION COMPLETE!";

        static int[] aInits = { 15, 8, 2 };
        static int[] mInits = { 15, 18, 6 };

        static double[] aVals = { 0.18, 0.8 };

        static double[] mVals = { 0.5, 0.9 };

        static Mode m = Mode.Null;
        static Classifier c = Classifier.Null;

        static List<Packet> GenerateTraining()
        {
            List<Packet> train = new List<Packet>();

            CreatePackets(train, Properties.Resources.Normal, 500, "Normal");
            CreatePackets(train, Properties.Resources.RootKit, 50, "RootKit");
            CreatePackets(train, Properties.Resources.BackDoor, 100, "BackDoor");
            CreatePackets(train, Properties.Resources.Smurf, 100, "Smurf");
            CreatePackets(train, Properties.Resources.GuessPassword, 100, "GuessPassword");
            CreatePackets(train, Properties.Resources.TearDrop, 100, "TearDrop");

            return train.OrderBy(a => Guid.NewGuid()).ToList<Packet>();
        }

        static List<Packet> GenerateTesting()
        {
            List<Packet> test = new List<Packet>();

            CreatePackets(test, Properties.Resources.Normal, 100, "Normal");
            CreatePackets(test, Properties.Resources.RootKit, 10, "RootKit");
            CreatePackets(test, Properties.Resources.BackDoor, 40, "BackDoor");
            CreatePackets(test, Properties.Resources.Smurf, 40, "Smurf");
            CreatePackets(test, Properties.Resources.GuessPassword, 40, "GuessPassword");
            CreatePackets(test, Properties.Resources.TearDrop, 40, "TearDrop");

            return test.OrderBy(a => Guid.NewGuid()).ToList<Packet>();
        }

        static void CreatePackets(List<Packet> storage, string input, int count, string type )
        {
            using (StringReader reader = new StringReader(input))
            {
                int stepper = 0;
                string line;
                while (stepper < count)
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        storage.Add(new Packet(line.Split(", "), type));
                        stepper++;
                    }
                    else
                    {
                        CreatePackets(storage, input, count - stepper, type);
                        break;
                    }
                }
                reader.Close();
            }
        }

        static bool Prompt()
        {
            string selector = Console.ReadLine();
            if(selector == "YES" || selector == "yes" || selector == "Y" || selector == "y")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static Mode ParseMode(string mode)
        {
            if (mode == "Train" || mode == "TR" || mode == "Tr" || mode == "tR" || mode == "tr" || mode == "-tr")
            {
                Console.WriteLine(MODE_TRAIN);
                return Mode.Train;
            }
            else if (mode == "Test" || mode == "TS" || mode == "Ts" || mode == "tS" || mode == "ts" || mode == "-ts")
            {
                Console.WriteLine(MODE_TEST);
                return Mode.Test;
            }
            else if (mode == "Q" || mode == "q" || mode == "quit")
            {
                return Mode.Quit;
            }

            return Mode.Null;
        }

        static Classifier ParseClassifier(string classify)
        {
            if (classify == "A" || classify == "a" || classify == "-a" || classify == "Anomaly")
            {
                Console.WriteLine(CLASS_ANOM);
                return Classifier.Anomaly;
            }
            else if (classify == "M" || classify == "m" || classify == "-m" || classify == "Misuse")
            {
                Console.WriteLine(CLASS_MISU);
                return Classifier.Misuse;
            }
            else if (classify == "Q" || classify == "q" || classify == "quit")
            {
                return Classifier.Quit;
            }
            return Classifier.Null;
        }

        static bool InputChecker()
        {
            while (m == Mode.Null)
            {
                Console.Write(MODE_MESSAGE);
                string mode = Console.ReadLine();
                m = ParseMode(mode);
            }

            if(m == Mode.Quit)
            {
                return false;
            }

            while (c == Classifier.Null)
            {
                Console.Write(CLASS_MESSAGE);
                string classify = Console.ReadLine();
                c = ParseClassifier(classify);
            }

            if(c == Classifier.Quit)
            {
                return false;
            }

            return true;

        }

        static void Main(string[] args)
        {
            Console.WriteLine(BANNER);
            bool cont;

            switch (args.Length)
            {
                case 0:
                    cont = InputChecker();
                    break;
                case 1:
                    m = ParseMode(args[0]);
                    c = ParseClassifier(args[0]);

                    cont = InputChecker();
                    break;
                case 2:
                    m = ParseMode(args[0]);
                    c = ParseClassifier (args[1]);

                    cont = InputChecker();
                    break;
                default:
                    Console.WriteLine("|Invalid Input, Exiting...");
                    return;
            }

            if (!cont)
            {
                return;
            }

            Network anomaly = new Network(aInits[0], aInits[1], aInits[2]);
            Network misuse = new Network(mInits[0], mInits[1], mInits[2]);

            while(c != Classifier.Quit || m != Mode.Quit)
            {
                if(m == Mode.Train)
                {
                    if(c == Classifier.Anomaly)
                    {
                        Console.WriteLine(TRAINING);
                        List<Packet> train = GenerateTraining();
                        Console.WriteLine(FIN);
                        Console.Write(TRAIN_INIT);
                        if (Prompt())
                        {
                            anomaly.Train(train, aVals, false, true);
                        }
                        else
                        {
                            anomaly.Train(train, aVals, false, false);
                        }
                    }
                    else
                    {
                        Console.WriteLine(TRAINING);
                        List<Packet> train = GenerateTraining();
                        Console.WriteLine(FIN);
                        Console.Write(TRAIN_INIT);
                        if (Prompt())
                        {
                            misuse.Train(train, mVals, true, true);
                        }
                        else
                        {
                            misuse.Train(train, mVals, true, false);
                        }
                    }
                } 
                else
                {
                    if(c == Classifier.Anomaly)
                    {
                        Console.WriteLine(TESTING);
                        List<Packet> test = GenerateTesting();
                        //List<Packet> test = GenerateTraining();
                        Console.WriteLine(FIN);
                        anomaly.Test(test, false);
                    }
                    else
                    {
                        Console.WriteLine(TESTING);
                        List<Packet> test = GenerateTesting();
                        Console.WriteLine(FIN);
                        misuse.Test(test, true);

                    }

                }

                Console.Write("|Run Again? (Y or N)\n|");
                if(!Prompt())
                {
                    m = Mode.Null;
                    c = Classifier.Null;

                    InputChecker();
                }
            }
        }
    }
}
