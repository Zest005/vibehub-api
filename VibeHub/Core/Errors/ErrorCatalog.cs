using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.Errors;

public static class ErrorCatalog
{
    public static readonly EntityResult<User> UserNotFound = new("The user was not found.", true);
    public static readonly EntityResult<User> Unauthorized = new("Unauthorized", true);
    public static readonly EntityResult<User> UserAlreadyExists = new("User is already exists.", true);
    public static readonly EntityResult<User> NickNameAlreadyExists = new("Nickname already registered", true);
    public static readonly EntityResult<User> EmailAlreadyExists = new("Email already registered", true);
    
    public static readonly EntityResult<Room> RoomNotFound = new("The room was not found.", true);
    public static readonly EntityResult<Room> UserForRoomNotFound = new("The user was not found.", true);
    public static readonly EntityResult<Room> NotUserOwner = new("You are not the owner of this room.", true);
    public static readonly EntityResult<Room> UserNotInRoom = new("You are not in room", true);
    public static readonly EntityResult<Room> MusicUpdateDenied = new("You cannot update music", true);
    public static readonly EntityResult<Room> RoomIsFull = new("The room is full.", true);
    public static readonly EntityResult<Room> SelfKick = new("You can't kick yourself", true);
    public static readonly EntityResult<Room> AlreadyInRoom = new("You are already in room", true);
    public static readonly EntityResult<Room> RoomIsPrivate = new("The room is private.", true);
    public static readonly EntityResult<Room> NotInRoom = new("You are not in room", true);
    
    public static readonly EntityResult<IEnumerable<MessageHistory>> MessagesForRoomNotFound = new("The messages was not found.", true);
    public static readonly EntityResult<MessageHistory> RoomOrUserNotFound = new("User or Room not found", true);
    public static readonly EntityResult<MessageHistory> MessageIsEmpty = new("Message is empty", true);
    
    public static readonly EntityResult<Music> MusicNotFound = new("The music was not found.", true);
    public static readonly EntityResult<FileStreamResult> MusicFileNotFound = new("Music not found", true);

    public static readonly EntityResult<Guid> SessionIdNotFound = new("Session not found", true);
    public static readonly EntityResult<Guid> SessionIdInvalid = new("Session is invalid", true);
}