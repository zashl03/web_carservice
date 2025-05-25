using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using web_service.Data.Entities;

public interface IWarehouseService
{
    Task<IEnumerable<WarehouseEntity>> GetAllWarehousesAsync();
    Task<WarehouseEntity> GetWarehouseAsync(Guid id);

    Task<IEnumerable<PartEntity>> GetPartsByWarehouseAsync(Guid warehouseId);
}