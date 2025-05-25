// Models/Admin/WarehousesViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using web_service.Data.Entities;

namespace web_service.Models.Admin
{
    public class WarehousesViewModel
    {
        public IEnumerable<SelectListItem>? WarehouseOptions { get; set; }
        public IEnumerable<SelectListItem>? StorekeeperOptions { get; set; }
        public Guid? SelectedWarehouseId { get; set; }
        [Required]
        [Display(Name = "Адрес склада")]
        public string NewAddress { get; set; }

        [Required]
        [Display(Name = "Кладовщик")]
        public string NewStorekeeperId { get; set; }
        public IEnumerable<PartEntity> Parts { get; set; } = new List<PartEntity>();
    }
}
