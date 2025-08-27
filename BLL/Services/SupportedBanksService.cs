using AutoMapper;
using BLL.DTO;
using BLL.DTO.SupportedBanks;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class SupportedBanksService : ISupportedBanksService
{
    private readonly IMapper _mapper;
    private readonly ISupportedBanksRepository _supportedBanksRepository;
    
    public SupportedBanksService(IMapper mapper, ISupportedBanksRepository supportedBanksRepository)
    {
        _mapper = mapper;
        _supportedBanksRepository = supportedBanksRepository;
    }
    
    public async Task<SupportedBanksReadOnlyDTO> CreateSupportedBankAsync(SupportedBanksCreateDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        
        var codeExists = await _supportedBanksRepository.CheckBankCodeExistsAsync(dto.BankCode);
        if (codeExists)
        {
            throw new Exception($"Bank code {dto.BankCode} already exists.");
        }
        SupportedBank supportedBank = _mapper.Map<SupportedBank>(dto);
        var createdSupportedBank = await _supportedBanksRepository.CreateSupportedBankWithTransactionAsync(supportedBank);
        return _mapper.Map<SupportedBanksReadOnlyDTO>(createdSupportedBank);
    }
    
    public async Task<SupportedBanksReadOnlyDTO> UpdateSupportedBankAsync(ulong id, SupportedBanksUpdateDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        
        var existingSupportedBank = await _supportedBanksRepository.GetSupportedBankByIdAsync(id);
        if (existingSupportedBank == null)
        {
            throw new Exception($"Supported bank with ID {id} not found.");
        }
        if(!string.IsNullOrWhiteSpace(dto.BankCode))
        {
            var codeExists = await _supportedBanksRepository.CheckBankCodeExistsAsync(dto.BankCode);
            if (codeExists)
            {
                throw new Exception($"Bank code {dto.BankCode} already exists.");
            }
        }
        
        _mapper.Map(dto, existingSupportedBank);
        var updatedSupportedBank = await _supportedBanksRepository.UpdateSupportedBankWithTransactionAsync(existingSupportedBank);
        return _mapper.Map<SupportedBanksReadOnlyDTO>(updatedSupportedBank);
    }
    
    public async Task<SupportedBanksReadOnlyDTO?> GetSupportedBankByIdAsync(ulong id)
    {
        var supportedBank = await _supportedBanksRepository.GetSupportedBankByIdAsync(id);
        return supportedBank == null ? null : _mapper.Map<SupportedBanksReadOnlyDTO>(supportedBank);
    }
    
    public async Task<SupportedBanksReadOnlyDTO?> GetSupportedBankByBankCodeAsync(String code)
    {
        var supportedBank = await _supportedBanksRepository.GetSupportedBankByBankCodeAsync(code);
        return supportedBank == null ? null : _mapper.Map<SupportedBanksReadOnlyDTO>(supportedBank);
    }
    
    public async Task<PagedResponse<SupportedBanksReadOnlyDTO>> GetAllSupportedBanksAsync(int page, int pageSize)
    {
        var (supportedBanks, totalCount) = await _supportedBanksRepository.GetAllSupportedBanksAsync(page, pageSize);
        var supportedBanksDtos = _mapper.Map<List<SupportedBanksReadOnlyDTO>>(supportedBanks);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        
        return new PagedResponse<SupportedBanksReadOnlyDTO>
        {
            Data = supportedBanksDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalCount,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}