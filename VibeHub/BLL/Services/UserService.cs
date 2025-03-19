using System.ComponentModel.DataAnnotations;
using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using Core.DTO;
using Core.Errors;
using Core.Models;
using DAL.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;


namespace BLL.Services;

public class UserService : IUserService
{
    private readonly IFilterUtility _filterUtility;
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordManagerUtility _passwordManagerUtility;

    public UserService(IFilterUtility filterUtility, ILogger<UserService> logger, IUserRepository userRepository,
        IPasswordManagerUtility passwordManagerUtility)
    {
        _filterUtility = filterUtility;
        _logger = logger;
        _userRepository = userRepository;
        _passwordManagerUtility = passwordManagerUtility;
    }

    public async Task<EntityResult<User>> Add(User user)
    {
        try
        {
            if (await _userRepository.Exists(user.Id))
                return ErrorCatalog.UserAlreadyExists;

            user = await _filterUtility.Filter(user);

            await _userRepository.Add(user);

            return new EntityResult<User>
            {
                Entity = user
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while logging out user.");
            return new EntityResult<User>("Unknown error", true);
        }
    }

    public async Task<EntityResult<User>> Delete(Guid id)
    {
        try
        {
            var user = await _userRepository.GetById(id);

            if (user == null)
            {
                _logger.LogError("User not found");
                return ErrorCatalog.UserNotFound;
            }

            return new EntityResult<User>();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while logging out user.");
            return new EntityResult<User>("Unknown error", true);
        }
    }

    public async Task<EntityResult<User>> GetById(Guid id)
    {
        try
        {
            EntityResult<User> entityResult = new()
            {
                Entity = await _userRepository.GetById(id)
            };

            if (entityResult.Entity == null)
            {
                _logger.LogError("User not found");
                return ErrorCatalog.UserNotFound;
            }

            return entityResult;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while logging out user.");
            return new EntityResult<User>("Unknown error", true);
        }
    }

    public async Task<EntityResult<IEnumerable<User>>> GetList()
    {
        try
        {
            EntityResult<IEnumerable<User>> entityResult = new()
            {
                Entity = await _userRepository.GetList()
            };

            return entityResult;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while logging out user.");
            return new EntityResult<IEnumerable<User>>("Unknown error", true);
        }
    }

    public async Task<EntityResult<User>> Update(Guid id, User user)
    {
        try
        {
            EntityResult<User> result = new()
            {
                Entity = await _userRepository.GetById(id)
            };

            if (result.Entity == null)
            {
                _logger.LogError("User not found");
                return ErrorCatalog.UserNotFound;
            }

            user = await _filterUtility.Filter(user);

            result.Entity.Email = user.Email;
            result.Entity.Nickname = user.Nickname;
            result.Entity.Password = user.Password;
            result.Entity.Room = user.Room;
            result.Entity.SessionId = user.SessionId;
            result.Entity.Salt = user.Salt;

            return result;
        }
        catch (ValidationException exception)
        {
            return new EntityResult<User>(exception.Message, true);
        }

        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while updating user.");
            return new EntityResult<User>("Unknown error", true);
        }
    }

    public async Task<EntityResult<User>> UpdateDto(Guid id, UserDto userDto)
    {
        try
        {
            string? fileData = null;

            var targetUser = await _userRepository.GetById(id);

            if (targetUser == null)
                return ErrorCatalog.UserNotFound;

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

            return new EntityResult<User>();
        }
        catch (ValidationException exception)
        {
            return new EntityResult<User>(exception.Message, true);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while updating user.");
            return new EntityResult<User>("Unknown error", true);
        }
    }

    public async Task<EntityResult<User>> Authenticate(string email, string password)
    {
        EntityResult<User> result = new()
        {
            Entity = await _userRepository.GetByEmail(email)
        };

        if (result.Entity == null ||
            !_passwordManagerUtility.VerifyPasswordHash(password, result.Entity.Password, result.Entity.Salt))
            return ErrorCatalog.Unauthorized;

        return result;
    }

    public async Task<EntityResult<User>> Logout(User user)
    {
        try
        {
            user.SessionId = null;

            await _userRepository.Update(user);

            return new EntityResult<User>();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while logging out user.");
            return new EntityResult<User>("Unknown error", true);
        }
    }

    public async Task<EntityResult<User>?> GetByEmail(string email)
    {
        try
        {
            EntityResult<User> result = new()
            {
                Entity = await _userRepository.GetByEmail(email)
            };

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while logging out user.");
            return new EntityResult<User>("Unknown error", true);
        }
    }

    public async Task<EntityResult<User>?> GetByNickname(string nickname)
    {
        try
        {
            EntityResult<User> result = new()
            {
                Entity = await _userRepository.GetByNickname(nickname)
            };

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while logging out user.");
            return new EntityResult<User>("Unknown error", true);
        }
    }
}