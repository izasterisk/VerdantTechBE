using AutoMapper;
using BLL.DTO.VendorBankAccount;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class VendorBankAccountsService : IVendorBankAccountsService
{
    private readonly IMapper _mapper;
    private readonly IVendorBankAccountsRepository _vendorBankAccountsRepository;
    private readonly IUserRepository _userRepository;

    public VendorBankAccountsService(
        IMapper mapper,
        IVendorBankAccountsRepository vendorBankAccountsRepository,
        IUserRepository userRepository)
    {
        _mapper = mapper;
        _vendorBankAccountsRepository = vendorBankAccountsRepository;
        _userRepository = userRepository;
    }

    public async Task<VendorBankAccountResponseDTO> CreateVendorBankAccountAsync(ulong userId, VendorBankAccountCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");

        var vendor = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (vendor == null || vendor.Role != UserRole.Vendor)
        {
            throw new KeyNotFoundException($"Người dùng với ID {userId} không tồn tại hoặc không được phép dùng chức năng này.");
        }

        var vendorBankAccount = _mapper.Map<VendorBankAccount>(dto);
        vendorBankAccount.VendorId = userId;
        var createdAccount = await _vendorBankAccountsRepository.CreateVendorBankAccountWithTransactionAsync(vendorBankAccount, cancellationToken);
        return _mapper.Map<VendorBankAccountResponseDTO>(createdAccount);
    }

    public async Task<VendorBankAccountResponseDTO> UpdateVendorBankAccountAsync(ulong accountId, VendorBankAccountUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");

        var existingAccount = await _vendorBankAccountsRepository.GetVendorBankAccountByIdAsync(accountId, cancellationToken);

        _mapper.Map(dto, existingAccount);
        var updatedAccount = await _vendorBankAccountsRepository.UpdateVendorBankAccountWithTransactionAsync(
            existingAccount,
            cancellationToken);

        return _mapper.Map<VendorBankAccountResponseDTO>(updatedAccount);
    }

    public async Task<List<VendorBankAccountResponseDTO>> GetAllVendorBankAccountsByVendorIdAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var vendor = await _userRepository.GetUserByIdAsync(vendorId, cancellationToken);
        if (vendor == null || vendor.Role != UserRole.Vendor)
        {
            throw new KeyNotFoundException($"Người dùng với ID {vendorId} không tồn tại hoặc không được phép dùng chức năng này.");
        }

        var accounts = await _vendorBankAccountsRepository.GetAllVendorBankAccountsByVendorIdAsync(
            vendorId,
            cancellationToken);

        return _mapper.Map<List<VendorBankAccountResponseDTO>>(accounts);
    }
}