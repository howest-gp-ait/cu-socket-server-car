using System;
using System.Collections.Generic;
using System.Text;
using Ait.SockCar.Server.Core.Entities;

namespace Ait.SockCar.Server.Core.Services
{
    public class CarService
    {
        public List<Car> Cars { get; private set; }
        public CarService()
        {
            Cars = new List<Car>();
        }
        public void AddCar(Car car)
        {
            Cars.Add(car);
        }
        public void DeleteCar(Car car)
        {
            Cars.Remove(car);
        }
        public Car FindCar(string nrPlate)
        {
            foreach(Car car in Cars)
            {
                if(car.NrPlate.ToUpper() == nrPlate)
                {
                    return car;
                }
            }
            return null;
        }
    }
}
