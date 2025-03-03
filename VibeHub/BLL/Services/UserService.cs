using BLL.Abstractions.Interfaces;
using Core.Models;
using DAL.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IFilterUtility _filterUtility;
    private readonly ILogger<UserService> _logger;

    public UserService(IFilterUtility filterUtility, ILogger<UserService> logger, IUserRepository userRepository)
    {
        _filterUtility = filterUtility;
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task Add(User user)
    {
        if (await _userRepository.Exists(user.Id))
        {
            throw new InvalidOperationException("A user with the same ID already exists.");
        }

        await _userRepository.Add(user);
    }

    public async Task Delete(Guid id)
    {
        var user = await _userRepository.GetById(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        await _userRepository.Delete(id);
    }

    public async Task<User> GetById(Guid id)
    {
        var user = await _userRepository.GetById(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return user;
    }

    public async Task<IEnumerable<User>> GetList()
    {
        return await _userRepository.GetList();
    }

    public async Task Update(Guid id, User user)
    {
        var existingUser = await _userRepository.GetById(id);
        if (existingUser == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        existingUser.Name = user.Name;
        existingUser.Email = user.Email;
        existingUser.Nickname = user.Nickname;
        existingUser.Password = user.Password;
        existingUser.Room = user.Room;

        await _userRepository.Update(existingUser);
    }
}