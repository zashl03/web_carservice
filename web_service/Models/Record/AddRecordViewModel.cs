using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;  // для SelectListItem

namespace web_service.Models.Record
{
    public class AddRecordViewModel
    {
        [Required(ErrorMessage = "Выберите автомобиль")]
        [Display(Name = "Автомобиль")]
        public Guid SelectedCarId { get; set; }

        // Список машин для формы (радио-кнопки или select)
        public List<SelectListItem> Cars { get; set; } = new();

        [Required(ErrorMessage = "Поле 'Дата' обязательно для заполнения")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Дата и время записи")]
        [DateAfterYesterday(ErrorMessage = "Дата должна быть позже вчерашнего дня")]
        public DateTime BookingDate { get; set; }

        [StringLength(500, ErrorMessage = "Комментарий не может быть длиннее 500 символов")]
        [Display(Name = "Комментарий")]
        public string? Comment { get; set; }
    }
}
