using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_service.Data.Entities
{
    public class CarEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required, StringLength(17, MinimumLength = 17)]
        public string VIN { get; set; } = null!;

        [Required, StringLength(9, MinimumLength = 7)]
        public string LicencePlate { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Brand { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Model { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Mileage { get; set; } = null!;

        [Required, MaxLength(4)]
        public int? Year { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Color { get; set; } = null!;

        [Required]
        public string ClientProfileId { get; set; } = null!;

        [ForeignKey(nameof(ClientProfileId))]
        public virtual ClientProfile Client { get; set; } = null!;

    }
}
