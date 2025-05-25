using System;
using System.ComponentModel.DataAnnotations;

namespace web_service.Models.Record
{
    /// <summary>
    /// Модель для отображения данных записи на обслуживание.
    /// Используется для передачи данных клиенту (только для чтения).
    /// </summary>
    public class RecordViewModel
    {
        /// <summary>Уникальный идентификатор записи</summary>
        public Guid Id { get; set; }

        /// <summary>Информация об автомобиле (марка, модель, VIN)</summary>
        [Display(Name = "Автомобиль")]
        public string CarDisplay { get; set; } = null!;

        /// <summary>Дата и время записи на обслуживание</summary>
        [Display(Name = "Дата записи")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime BookingDate { get; set; }

        /// <summary>Комментарий клиента к записи</summary>
        [Display(Name = "Комментарий")]
        public string? Comment { get; set; }
    }
}
