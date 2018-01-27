using System;
using System.Collections.Generic;
using System.Text;

namespace TotalTroll
{
    class Bucket
    {
        public DateTime lastCheck;
        private double allowance;
        private double rate;
        private double time;

        public Bucket(double rate, double time)
        {
            this.lastCheck = DateTime.Now;
            this.allowance = rate;
            this.rate = rate;
            this.time = time;
        }

        public bool CanSpend(int count)
        {
            this.allowance += (DateTime.Now - this.lastCheck).TotalMilliseconds / 1000D * (this.rate / this.time);
            this.lastCheck = DateTime.Now;
            if (this.allowance > this.rate)
            {
                this.allowance = this.rate;
            }
            if (this.allowance < count)
            {
                return false;
            }
            this.allowance -= count;
            return true;
        }

        public double WaitTime()
        {
            this.allowance += (DateTime.Now - this.lastCheck).TotalMilliseconds / 1000D * (this.rate / this.time);
            this.lastCheck = DateTime.Now;
            if (this.allowance > this.rate)
            {
                this.allowance = this.rate;
            }
            if (this.allowance > 1D)
            {
                return 0D;
            }
            return (1D - allowance) * (this.time / this.rate);
        }

        public double Allowance()
        {
            return allowance;
        }
    }
}
