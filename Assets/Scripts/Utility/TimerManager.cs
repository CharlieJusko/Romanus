using UnityEngine;


namespace Utility
{
    public static class TimerManager
    {
        public static void Increment(float maxTime, ref float timer, float amount)
        {
            if(timer <= maxTime)
                timer += amount;
            else
                timer = maxTime;
        }

        public static void Decrement(float minTime, ref float timer, float amount)
        {
            if(timer >= minTime)
                timer -= amount;
            else
                timer = minTime;
        }

        public static void Reset(ref float timer, float value=0f)
        {
            timer = value;
        }
    }
}
