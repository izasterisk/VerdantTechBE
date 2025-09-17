using BLL.Interfaces;
using AutoMapper;
using BLL.DTO;
using BLL.DTO.User;
using BLL.Helpers.Auth;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    
    public UserService(IMapper mapper, IUserRepository userRepository)
    {
        _mapper = mapper;
        _userRepository = userRepository;
    }

    public async Task<UserReadOnlyDTO> CreateUserAsync(UserCreateDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        
        var emailExists = await _userRepository.CheckEmailExistsAsync(dto.Email);
        if (emailExists)
        {
            throw new Exception($"Email {dto.Email} already exists.");
        }
        if(dto.Role == null)
        {
            dto.Role = "customer";
        }
        User user = _mapper.Map<User>(dto);
        user.PasswordHash = AuthUtils.HashPassword(dto.Password);
        user.IsVerified = false;
        var createdUser = await _userRepository.CreateUserWithTransactionAsync(user);
        return _mapper.Map<UserReadOnlyDTO>(createdUser);
    }

    public async Task<UserReadOnlyDTO?> GetUserByIdAsync(ulong userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        return user == null ? null : _mapper.Map<UserReadOnlyDTO>(user);
    }

    public async Task<PagedResponse<UserReadOnlyDTO>> GetAllUsersAsync(int page, int pageSize, String? role = null)
    {
        var (users, totalCount) = await _userRepository.GetAllUsersAsync(page, pageSize, role);
        var userDtos = _mapper.Map<List<UserReadOnlyDTO>>(users);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        
        return new PagedResponse<UserReadOnlyDTO>
        {
            Data = userDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalCount,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<UserReadOnlyDTO> UpdateUserAsync(ulong userId, UserUpdateDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        if (existingUser == null)
        {
            throw new Exception($"User with ID {userId} not found.");
        }
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (!Enum.TryParse<UserStatus>(dto.Status, true, out var newStatus))
                throw new ArgumentException($"Invalid status '{dto.Status}'. Valid statuses are: {string.Join(", ", Enum.GetNames<UserStatus>())}");

            if (existingUser.Status == UserStatus.Deleted)
                throw new InvalidOperationException("This account has already been deleted.");

            if (existingUser.Status != newStatus)
            {
                existingUser.Status = newStatus;
                if (newStatus == UserStatus.Deleted)
                    existingUser.DeletedAt = DateTime.UtcNow;
            }
        }
        _mapper.Map(dto, existingUser);
        var updatedUser = await _userRepository.UpdateUserWithTransactionAsync(existingUser);
        return _mapper.Map<UserReadOnlyDTO>(updatedUser);
    }
}