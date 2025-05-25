using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Entities;

public class WarehouseService : IWarehouseService
{
    private readonly ApplicationDbContext _db;
    public WarehouseService(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<WarehouseEntity>> GetAllWarehousesAsync()
    {
        return await _db.Warehouses
                        .Include(w => w.Storekeeper)
                        .ToListAsync();
    }

    public async Task<WarehouseEntity> GetWarehouseAsync(Guid id)
    {
        return await _db.Warehouses
                        .Include(w => w.Storekeeper)
                        .FirstOrDefaultAsync(w => w.Id == id);
    }
    public async Task<IEnumerable<PartEntity>> GetPartsByWarehouseAsync(Guid warehouseId)
    {
        return await _db.Parts
                        .Where(p => p.WarehouseId == warehouseId)
                        .ToListAsync();
    }
}