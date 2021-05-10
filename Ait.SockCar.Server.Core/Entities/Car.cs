using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Ait.SockCar.Server.Core.Entities
{
    public class Car
    {
        public string NrPlate { get; private set; }
        public DateTime LastCall { get; private set; }
        public double LastSpeed { get; private set; }
        public double TotalDistance { get; private set; }

        public Car(string nrPlate)
        {
            NrPlate = nrPlate;
            LastCall = DateTime.Now;
            LastSpeed = 0.0;
            TotalDistance = 0.0;
        }
        public void ChangeSpeed(double newSpeed)
        {
            DateTime newCall = DateTime.Now;
            double totalSeconds = (newCall - LastCall).TotalSeconds;
            double speedPerSec = 1.0 * LastSpeed / 3600;
            double distanceSinceLastCall = totalSeconds * speedPerSec;
            TotalDistance += distanceSinceLastCall * 3600 / 1000;
            LastCall = newCall;
            LastSpeed = newSpeed;
        }
        public double GetTotalDistance()
        {
            DateTime newCall = DateTime.Now;
            double totalSeconds = (newCall - LastCall).TotalSeconds;
            double speedPerSec = 1.0 * LastSpeed / 3600;
            double distanceSinceLastCall = totalSeconds * speedPerSec;
            TotalDistance += distanceSinceLastCall * 3600 / 1000;
            LastCall = newCall;
            return TotalDistance;
        }

    }
}
