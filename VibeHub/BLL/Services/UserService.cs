using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using Core.Models;
using DAL.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class UserService : IUserService
{
    private readonly IFilterUtility _filterUtility;
    private readonly ILogger<UserService> _logger;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;

    public UserService(IFilterUtility filterUtility, ILogger<UserService> logger, IUserRepository userRepository,
        IRoomRepository roomRepository)
    {
        _filterUtility = filterUtility;
        _logger = logger;
        _userRepository = userRepository;
        _roomRepository = roomRepository;
    }

    public async Task Add(User user)
    {
        if (await _userRepository.Exists(user.Id))
            throw new InvalidOperationException("A user with the same ID already exists.");

        await _userRepository.Add(user);
    }

    public async Task Delete(Guid id)
    {
        var user = await _userRepository.GetById(id);
        if (user == null) throw new KeyNotFoundException("User not found.");

        await _userRepository.Delete(id);
    }

    public async Task<User> GetById(Guid id)
    {
        var user = await _userRepository.GetById(id);
        if (user == null) throw new KeyNotFoundException("User not found.");

        return user;
    }

    public async Task<IEnumerable<User>> GetList()
    {
        return await _userRepository.GetList();
    }

    public async Task Update(Guid id, User user)
    {
        var existingUser = await _userRepository.GetById(id);
        if (existingUser == null) throw new KeyNotFoundException("User not found.");

        existingUser.Name = user.Name;
        existingUser.Email = user.Email;
        existingUser.Nickname = user.Nickname;
        existingUser.Password = user.Password;
        existingUser.Room = user.Room;

        await _userRepository.Update(existingUser);
    }

    public async Task<User> Authenticate(string email, string password)
    {
        var user = await _userRepository.GetByEmail(email);
        if (user == null || user.Password != password)
            return null;

        return user;
    }

    public async Task Logout(User user)
    {
        user.Token = null;
        await _userRepository.Update(user);
    }
}