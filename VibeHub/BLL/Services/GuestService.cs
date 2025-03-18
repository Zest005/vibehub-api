using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using Core.Models;
using DAL.Abstractions.Interfaces;


namespace BLL.Services;

public class GuestService : IGuestService
{
    private readonly IGuestRepository _guestRepository;
    private readonly IGeneratorUtility _generator;

    public GuestService(IGeneratorUtility generator, IGuestRepository guestRepository)
    {
        _generator = generator;
        _guestRepository = guestRepository;
    }

    public async Task<Guest> Create()
    {
        return await _guestRepository.Create(new Guest
        {
            Id = Guid.NewGuid(),
            Name = "Guest" + _generator.GenerateString(10),
            Room = null
        });
    }

    public async Task Delete(Guid id)
    {
        var guest = await _guestRepository.GetById(id);

        if (guest == null)
            throw new ArgumentException("Guest not found");

        await _guestRepository.Delete(guest);
    }
}