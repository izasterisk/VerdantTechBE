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
        if (await _userBankAccountsRepository.ValidateImportedBankAccount(userId, dto.AccountNumber, cancellationToken))
        {
            throw new DuplicateNameException("Tài khoản ngân hàng đã tồn tại.");
        }
        
        var userBankAccount = _mapper.Map<UserBankAccount>(dto);
        userBankAccount.UserId = userId;
        var createdAccount = await _userBankAccountsRepository.CreateUserBankAccountWithTransactionAsync(userBankAccount, cancellationToken);
        return _mapper.Map<UserBankAccountResponseDTO>(createdAccount);
    }

    public async Task<bool> SoftDeleteUserBankAccountAsync(ulong accountId, CancellationToken cancellationToken = default)
    {
        var existingAccount = await _userBankAccountsRepository.GetUserBankAccountByIdAsync(accountId, cancellationToken);
        return await _userBankAccountsRepository.SoftDeleteUserBankAccountWithTransactionAsync(existingAccount, cancellationToken);
    }

    public async Task<List<UserBankAccountResponseDTO>> GetAllUserBankAccountsByUserIdAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        var accounts = await _userBankAccountsRepository.GetAllUserBankAccountsByUserIdAsync(
            userId,
            cancellationToken);

        return _mapper.Map<List<UserBankAccountResponseDTO>>(accounts);
    }
}
