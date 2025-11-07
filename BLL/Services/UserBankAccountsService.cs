using System.Data;
using AutoMapper;
using BLL.DTO.UserBankAccount;
using BLL.Helpers.VendorBankAccounts;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class UserBankAccountsService : IUserBankAccountsService
{
    private readonly IMapper _mapper;
    private readonly IUserBankAccountsRepository _userBankAccountsRepository;
    private readonly IUserRepository _userRepository;

    public UserBankAccountsService(
        IMapper mapper,
        IUserBankAccountsRepository userBankAccountsRepository,
        IUserRepository userRepository)
    {
        _mapper = mapper;
        _userBankAccountsRepository = userBankAccountsRepository;
        _userRepository = userRepository;
    }

    public async Task<UserBankAccountResponseDTO> CreateUserBankAccountAsync(ulong userId, UserBankAccountCreateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} rỗng.");
        VendorBankAccountsHelper.ValidateBankCode(dto.BankCode);
        await _userRepository.GetUserByIdAsync(userId, cancellationToken);

        var userBankAccount = _mapper.Map<UserBankAccount>(dto);
        userBankAccount.UserId = userId;
        var createdAccount = await _userBankAccountsRepository.CreateUserBankAccountWithTransactionAsync(userBankAccount, cancellationToken);
        return _mapper.Map<UserBankAccountResponseDTO>(createdAccount);
    }

    public async Task<UserBankAccountResponseDTO> UpdateUserBankAccountAsync(ulong accountId, UserBankAccountUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} rỗng.");
        if(dto.BankCode != null)
            VendorBankAccountsHelper.ValidateBankCode(dto.BankCode);
        
        var existingAccount = await _userBankAccountsRepository.GetUserBankAccountByIdAsync(accountId, cancellationToken);
        _mapper.Map(dto, existingAccount);
        if (dto.AccountNumber != null || dto.AccountHolder != null)
        {
            if (await _userBankAccountsRepository.ValidateImportedBankAccount(existingAccount.UserId, 
                    existingAccount.AccountNumber, existingAccount.AccountHolder, cancellationToken) == true)
                throw new DuplicateNameException("Người dùng đã có sẵn tài khoản ngân hàng như này.");
        }
        var updatedAccount = await _userBankAccountsRepository.UpdateUserBankAccountWithTransactionAsync(
            existingAccount, cancellationToken);
        return _mapper.Map<UserBankAccountResponseDTO>(updatedAccount);
    }

    public async Task<bool> DeleteUserBankAccountAsync(ulong accountId, CancellationToken cancellationToken = default)
    {
        var existingAccount = await _userBankAccountsRepository.GetUserBankAccountByIdAsync(accountId, cancellationToken);
        return await _userBankAccountsRepository.DeleteUserBankAccountWithTransactionAsync(existingAccount, cancellationToken);
    }

    public async Task<List<UserBankAccountResponseDTO>> GetAllUserBankAccountsByUserIdAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        var accounts = await _userBankAccountsRepository.GetAllUserBankAccountsByUserIdAsync(
            userId,
            cancellationToken);

        return _mapper.Map<List<UserBankAccountResponseDTO>>(accounts);
    }
}
