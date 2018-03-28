using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingConcepts
{
    public class MovingAverageFilter
    {
        private Queue<double> samples = new Queue<double>();
        private int windowSize = 5;
        private double sampleAccumulator;
        public double Average { get; private set; }

        public MovingAverageFilter()
        {
            this.sampleAccumulator = 0;
            this.Average = 0;
            
        }

        /// <summary>
        /// Computes a new windowed average each time a new sample arrives
        /// </summary>
        /// <param name="newSample"></param>
        public void ComputeAverage(double newSample)
        {
            samples.Enqueue(newSample);
            if (samples.Count > this.windowSize)
            {
                sampleAccumulator -= samples.First();
                samples.Dequeue();
            }

            sampleAccumulator += newSample;

            Average = 1 * (sampleAccumulator / samples.Count) + 0 * newSample;

        }
    }
}
