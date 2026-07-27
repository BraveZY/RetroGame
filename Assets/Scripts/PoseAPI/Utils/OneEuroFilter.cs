using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// One Euro Filter implementation for stabilizing noisy signals.
    /// Reference: http://www.lifl.fr/~casiez/1euro/
    /// </summary>
    public class OneEuroFilter
    {
        private float minCutoff;
        private float beta;
        private float dCutoff;
        
        private float lastValue;
        private float lastDerivative;
        private float lastTimestamp;
        private bool hasLastValue;

        public OneEuroFilter(float minCutoff = 1.0f, float beta = 0.0f, float dCutoff = 1.0f)
        {
            this.minCutoff = minCutoff;
            this.beta = beta;
            this.dCutoff = dCutoff;
            this.hasLastValue = false;
        }

        public float Filter(float value, float timestamp)
        {
            if (!hasLastValue)
            {
                lastValue = value;
                lastDerivative = 0;
                lastTimestamp = timestamp;
                hasLastValue = true;
                return value;
            }

            float dt = timestamp - lastTimestamp;
            if (dt <= 0) return lastValue;

            // Calculate derivative
            float derivative = (value - lastValue) / dt;
            float alphaD = Alpha(dt, dCutoff);
            float smoothedDerivative = LowPassFilter(derivative, lastDerivative, alphaD);

            // Calculate cutoff frequency based on derivative
            float cutoff = minCutoff + beta * Mathf.Abs(smoothedDerivative);
            float alpha = Alpha(dt, cutoff);
            float smoothedValue = LowPassFilter(value, lastValue, alpha);

            lastValue = smoothedValue;
            lastDerivative = smoothedDerivative;
            lastTimestamp = timestamp;

            return smoothedValue;
        }

        private float Alpha(float dt, float cutoff)
        {
            float tau = 1.0f / (2.0f * Mathf.PI * cutoff);
            return 1.0f / (1.0f + tau / dt);
        }

        private float LowPassFilter(float value, float lastValue, float alpha)
        {
            return alpha * value + (1.0f - alpha) * lastValue;
        }

        public void Reset()
        {
            hasLastValue = false;
        }
    }

    /// <summary>
    /// Vector2 version of One Euro Filter
    /// </summary>
    public class OneEuroFilterVector2
    {
        private OneEuroFilter filterX;
        private OneEuroFilter filterY;

        public OneEuroFilterVector2(float minCutoff = 1.0f, float beta = 0.0f, float dCutoff = 1.0f)
        {
            filterX = new OneEuroFilter(minCutoff, beta, dCutoff);
            filterY = new OneEuroFilter(minCutoff, beta, dCutoff);
        }

        public Vector2 Filter(Vector2 value, float timestamp)
        {
            return new Vector2(
                filterX.Filter(value.x, timestamp),
                filterY.Filter(value.y, timestamp)
            );
        }

        public void Reset()
        {
            filterX.Reset();
            filterY.Reset();
        }
    }
}
