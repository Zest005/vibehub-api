using Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Abstractions.Interfaces
{
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetList();
        Task<Room> GetById(Guid id);
        Task<Room> GetByCode(string code);
        Task Add(Room room);
        Task Update(Room room);
        Task Delete(Guid id);
        Task<bool> Exists(Guid id);
    }
}
