using System;
using System.ComponentModel.DataAnnotations;

namespace web_service.Domain.Entities
{
    public class Car
    {
        protected Car() { }                     // Конструктор для EF Core (не использовать напрямую)

        public Car(string brand, string model, string vin, string ownerId)  // Основной конструктор (бренд, модель, VIN, ID владельца)
        {
            if (string.IsNullOrWhiteSpace(vin))   // VIN обязателен для заполнения
                throw new ArgumentException("VIN обязателен");

            Brand = brand;
            Model = model;
            VIN = vin;
            OwnerId = ownerId;
        }

        public Guid Id { get; private set; }     // Уникальный идентификатор (генерируется БД)
        public string Brand { get; private set; } // Марка автомобиля (пример: "Tesla")
        public string Model { get; private set; } // Модель автомобиля (пример: "Model 3")
        public string VIN { get; private set; }   // VIN-код (17 символов, уникальный)
        public string OwnerId { get; private set; } // ID владельца (внешний ключ)
    }
}