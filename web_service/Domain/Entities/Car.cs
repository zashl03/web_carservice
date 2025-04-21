using System;
using System.ComponentModel.DataAnnotations;

namespace web_service.Domain.Entities
{
    public class Car
    {
        // Для EF Core
        protected Car() { }

        public Car(string brand, string model, string vin, string ownerId)
        {
            if (string.IsNullOrWhiteSpace(vin))
                throw new ArgumentException("VIN обязателен");

            Brand = brand;
            Model = model;
            VIN = vin;
            OwnerId = ownerId;
        }

        public Guid Id { get; private set; }
        public string Brand { get; private set; }
        public string Model { get; private set; }
        public string VIN { get; private set; }
        public string OwnerId { get; private set; }
    }
}