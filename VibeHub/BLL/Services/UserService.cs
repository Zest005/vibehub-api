using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using Core.DTO;
using Core.Models;
using DAL.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;
using System.Text;

namespace BLL.Services;

public class UserService : IUserService
{
    private readonly IFilterUtility _filterUtility;
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordManagerUtility _passwordManagerUtility;

    public UserService(IFilterUtility filterUtility, ILogger<UserService> logger, IUserRepository userRepository, IPasswordManagerUtility passwordManagerUtility)
    {
        _filterUtility = filterUtility;
        _logger = logger;
        _userRepository = userRepository;
        _passwordManagerUtility = passwordManagerUtility;
    }

    public async Task Add(User user)
    {
        if (await _userRepository.Exists(user.Id))
            throw new InvalidOperationException("A user with the same ID already exists.");

        user = await _filterUtility.Filter(user);
        
        await _userRepository.Add(user);
    }

    public async Task Delete(Guid id)
    {
        var user = await _userRepository.GetById(id);
        
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        await _userRepository.Delete(id);
    }

    public async Task<User> GetById(Guid id)
    {
        var user = await _userRepository.GetById(id);
        
        if (user == null)
            throw new KeyNotFoundException("User not found.");

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
            throw new KeyNotFoundException("User not found.");

        user = await _filterUtility.Filter(user);

        existingUser.Email = user.Email;
        existingUser.Nickname = user.Nickname;
        existingUser.Password = user.Password;
        existingUser.Room = user.Room;
        existingUser.Token = user.Token;
        existingUser.Salt = user.Salt;

        await _userRepository.Update(existingUser);
    }

    public async Task UpdateDto(Guid id, UserDto userDto)
    {
        string? fileData = null;

        var targetUser = await _userRepository.GetById(id);

        if (targetUser == null)
            throw new KeyNotFoundException("User not found.");

        if (userDto.Avatar != null)
        {
            using var memoryStream = new MemoryStream();
            await userDto.Avatar.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var image = await Image.LoadAsync(memoryStream);
            image.Mutate(x => x.Resize(24, 24));

            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, new PngEncoder());

            fileData = Convert.ToBase64String(outputStream.ToArray());
        }

        userDto = await _filterUtility.Filter(userDto);

        targetUser.Nickname = userDto.Nickname ?? targetUser.Nickname;
        targetUser.Email = userDto.Email ?? targetUser.Email;
        targetUser.Password = userDto.Password ?? targetUser.Password;
        targetUser.Avatar = fileData ?? targetUser.Avatar;

        await _userRepository.Update(targetUser);
    }

    public async Task<User> Authenticate(string email, string password)
    {
        var user = await _userRepository.GetByEmail(email);
        if (user == null || !_passwordManagerUtility.VerifyPasswordHash(password, user.Password, user.Salt))
            return null;

        return user;
    }

    public async Task Logout(User user)
    {
        user.Token = null;
        
        await _userRepository.Update(user);
    }

    public async Task<User?> GetByEmail(string email)
    {
        return await _userRepository.GetByEmail(email);
    }

    public async Task<User?> GetByNickname(string nickname)
    {
        return await _userRepository.GetByNickname(nickname);
    }
}