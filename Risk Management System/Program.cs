
using System;

namespace Fault_Tolerant
{
    public delegate double Distributions(double x);
    public class Blocking
    {
       public static int faultRate = 0;
        int time0;
        public bool Isfault { set; get; }
        Distributions _fault;
        Distributions _corrective;
        static Random R = new Random();
        public double Rfault(double x) => _fault(x);
        public double Rcorrective(double x) => _corrective(x);
        public Blocking(Distributions fault, Distributions corrective)
        {
            _fault = fault;
            _corrective = corrective;
            time0 = 0;
            Isfault = false;
        }

        public bool Step(int time)
        {
            bool b;
            if (Isfault)
            {
                b = R.NextDouble() < _corrective(time - time0);
                if (b)
                {
                    Isfault = false;
                    time0 = time;
                }

            }
            else
            {
                b = R.NextDouble() < _fault(time - time0);
                if (b)
                {
                    faultRate++;
                    Isfault = true;
                    time0 = time;
                }
            }
            return Isfault;
        }
    }
    class Program
    {


        static void Main(string[] args)
        {
            Blocking A = new Blocking((x) => Weibull(x, 1000, 1.5), (x) => Weibull(x, 100, 1.5));
            Blocking B = new Blocking((x) => Exponential(x, 10000), (x) => Weibull(x, 20, 1.5));
            Blocking C = new Blocking((x) => Normal(x, 200, 1000), (x) => Normal(x, 2, 6));
            Blocking D = new Blocking((x) => Weibull(x, 1000, 1.5), (x) => Exponential(x, 10));
            Blocking E = new Blocking((x) => Weibull(x, 1000, 3), (x) => Weibull(x, 20, 1.5));
            Blocking F = new Blocking((x) => Weibull(x, 5000, 1.5), (x) => Weibull(x, 100, 1.5));
            Blocking G = new Blocking((x) => Exponential(x, 100000), (x) => Weibull(x, 10, 1.5));
            Blocking H = new Blocking((x) => Normal(x, 50, 5000), (x) => Normal(x, 2, 10));
            Random R = new Random();
            Console.WriteLine("\n\n{0,-6}|{1,-12}|{2,-12}|{3,-12}|{4,-12}|{5,-12}", "Type", "Accesable", "fault Count","MTTF","Up Time","Down Time");
            // ReliabilityBlockDiagram
            int num_fault = 0;
            int Uptime = 0;
            int Downtime = 0;
            bool IsFault = false;
            for (int i = 0; i < 1000; i += 1)
            {
                if (!IsFault)
                {
                    double Rf = 1 - (1 - A.Rfault(i) * B.Rfault(i) * C.Rfault(i) * D.Rfault(i) * G.Rfault(i) * H.Rfault(i)) * (1 - (1 - E.Rfault(i)) * (1 - F.Rfault(i)));
                    Uptime++;
                    if (R.NextDouble() > Rf)
                    { IsFault = true;
                        num_fault++;
                    }
                }
                else
                {
                    double Rc = 1 - (1 - A.Rcorrective(i) * B.Rcorrective(i) * C.Rcorrective(i) * D.Rcorrective(i) * G.Rcorrective(i) * H.Rcorrective(i)) * (1 - (1 - E.Rcorrective(i)) * (1 - F.Rcorrective(i)));
                    Console.WriteLine($"Rc:{Rc}");
                    Downtime++;
                    if (R.NextDouble() > Rc)
                        IsFault = false;
                }
            }
            double MTTF = (1.0 * Downtime) / (Downtime + Uptime);
            double Accss = (1.0 * Uptime) / (Downtime + Uptime);
            Console.WriteLine("|{0,-6}|{1,12:0.##}|{2,12:0.##}|{3,12:0.##}|{4,12:0.##}|{5,12:0.##}", "RBD", Accss, num_fault, MTTF, Uptime, Downtime);

           
            //FaultTree
            Uptime = 0;
            Downtime = 0;
            
            for (int i = 0; i < 1000; i += 1)
            {
                if (A.Step(i) && B.Step(i) && C.Step(i) && D.Step(i) && (E.Step(i) || F.Step(i)) && G.Step(i) && H.Step(i))
                    Downtime++;
                else
                    Uptime++;

            }
             MTTF = (1.0 * Downtime) / (Downtime + Uptime);
             Accss = (1.0 * Uptime) / (Downtime + Uptime);
            Console.WriteLine("|{0,-6}|{1,12:0.##}|{2,12:0.##}|{3,12:0.##}|{4,12:0.##}|{5,12:0.##}\n\n", "FT", Accss, Blocking.faultRate, MTTF, Uptime, Downtime);


        }
        static double Weibull(double x, double shape, double scale)
        {
            if (x < 0)
                return 0;
            double e = Math.Exp(-1 * Math.Pow(x / scale, shape));
            return e * Math.Pow(x / scale, shape - 1) * (shape / scale);
        }
        static double Exponential(double x, double mean)
        {
            if (x < 0)
                return 0;
            double rate = 1 / mean;
            return rate * Math.Exp(-1 * rate * x);

        }
        static double Normal(double x, double stddev, double mean)
        {

            double variance = stddev * stddev;
            return Math.Exp((-1 * (x - mean) * (x - mean)) / (2 * variance)) / Math.Sqrt(2 * Math.PI * variance);

        }
    }
}

