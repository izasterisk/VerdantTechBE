using BLL.Interfaces;
using AutoMapper;
using BLL.DTO;
using BLL.DTO.User;
using BLL.Helpers.Auth;
using BLL.Helpers;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using BLL.Interfaces.Infrastructure;

namespace BLL.Services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IEmailSender _emailSender;
    
    public UserService(IMapper mapper, IUserRepository userRepository, IAddressRepository addressRepository, IEmailSender emailSender)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _addressRepository = addressRepository;
        _emailSender = emailSender;
    }

    public async Task<UserResponseDTO> CreateUserAsync(UserCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        
        var emailExists = await _userRepository.CheckEmailExistsAsync(dto.Email, cancellationToken);
        if (emailExists)
        {
            throw new Exception($"Email {dto.Email} already exists.");
        }
        User user = _mapper.Map<User>(dto);
        user.PasswordHash = AuthUtils.HashPassword(dto.Password);
        user.IsVerified = false;
        var createdUser = await _userRepository.CreateUserWithTransactionAsync(user, cancellationToken);
        return _mapper.Map<UserResponseDTO>(createdUser);
    }
    
    public async Task<UserResponseDTO> CreateStaffAsync(StaffCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        
        var emailExists = await _userRepository.CheckEmailExistsAsync(dto.Email, cancellationToken);
        if (emailExists)
        {
            throw new Exception($"Email {dto.Email} already exists.");
        }
        
        var generatedPassword = AuthUtils.GenerateNumericCode();
        User user = _mapper.Map<User>(dto);
        user.PasswordHash = AuthUtils.HashPassword(generatedPassword);
        try
        {
            await _emailSender.SendStaffAccountCreatedEmailAsync(dto.Email, dto.FullName, generatedPassword, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to send staff creation email to {dto.Email}: {ex.Message}");
        }
        var createdStaff = await _userRepository.CreateStaffWithTransactionAsync(user, cancellationToken);
        return _mapper.Map<UserResponseDTO>(createdStaff);
    }
    
    public async Task<UserResponseDTO> UpdateUserAsync(ulong userId, UserUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        
        var existingUser = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (existingUser == null)
        {
            throw new Exception($"User with ID {userId} not found.");
        }
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            var newStatus = Utils.ParseEnum<UserStatus>(dto.Status, "trạng thái người dùng");

            if (existingUser.Status == UserStatus.Deleted)
                throw new InvalidOperationException("Tài khoản này đã bị xóa.");

            if (existingUser.Status != newStatus)
            {
                existingUser.Status = newStatus;
                if (newStatus == UserStatus.Deleted)
                    existingUser.DeletedAt = DateTime.UtcNow;
            }
        }
        _mapper.Map(dto, existingUser);
        var updatedUser = await _userRepository.UpdateUserWithTransactionAsync(existingUser, cancellationToken);
        return _mapper.Map<UserResponseDTO>(updatedUser);
    }
    
    public async Task<UserResponseDTO> CreateUserAddressAsync(ulong userId, UserAddressCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        var existingUser = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (existingUser == null)
        {
            throw new Exception($"User with ID {userId} not found.");
        }
        Address address = _mapper.Map<Address>(dto);
        await _addressRepository.CreateUserAddressAsync(existingUser.Id, address, cancellationToken);
        var updatedUser = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        return _mapper.Map<UserResponseDTO>(updatedUser);
    }
    
    public async Task<UserResponseDTO> UpdateUserAddressByAddressIdAsync(ulong addressId, UserAddressUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        
        var existingAddress = await _addressRepository.GetAddressByIdAsync(addressId, cancellationToken);
        if (existingAddress == null)
        {
            throw new Exception($"Địa chỉ với ID {addressId} không tồn tại.");
        }
        
        var existingUserAddress = await _addressRepository.GetUserAddressByAddressIdAsync(existingAddress.Id, cancellationToken);
        if (existingUserAddress == null)
        {
            throw new Exception($"Địa chỉ với ID {addressId} không được liên kết đến địa chỉ nhà/công ty của tài khoản nào.");
        }
        
        _mapper.Map(dto, existingAddress);
        _mapper.Map(dto, existingUserAddress);
        var updatedUserAddress = await _addressRepository.UpdateUserAddressAsync(existingUserAddress, existingAddress, cancellationToken);
        
        var user = await _userRepository.GetUserByIdAsync(updatedUserAddress.UserId, cancellationToken);
        return _mapper.Map<UserResponseDTO>(user);
    }

    public async Task<UserResponseDTO?> GetUserByIdAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        return user == null ? null : _mapper.Map<UserResponseDTO>(user);
    }

    public async Task<PagedResponse<UserResponseDTO>> GetAllUsersAsync(int page, int pageSize, String? role = null, CancellationToken cancellationToken = default)
    {
        var (users, totalCount) = await _userRepository.GetAllUsersAsync(page, pageSize, role, cancellationToken);
        var userDtos = _mapper.Map<List<UserResponseDTO>>(users);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        
        return new PagedResponse<UserResponseDTO>
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
}