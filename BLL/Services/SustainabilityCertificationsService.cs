using AutoMapper;
using BLL.DTO;
using BLL.DTO.SustainabilityCertifications;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class SustainabilityCertificationsService : ISustainabilityCertificationsService
{
    private readonly IMapper _mapper;
    private readonly ISustainabilityCertificationsRepository _sustainabilityCertificationsRepository;
    
    public SustainabilityCertificationsService(IMapper mapper, ISustainabilityCertificationsRepository sustainabilityCertificationsRepository)
    {
        _mapper = mapper;
        _sustainabilityCertificationsRepository = sustainabilityCertificationsRepository;
    }

    public async Task<SustainabilityCertificationsReadOnlyDTO> CreateSustainabilityCertificationAsync(SustainabilityCertificationsCreateDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        var codeExists = await _sustainabilityCertificationsRepository.CheckCodeExistsAsync(dto.Code);
        if (codeExists)
        {
            throw new Exception($"Email {dto.Code} already exists.");
        }
        
        SustainabilityCertification sustainabilityCertification = _mapper.Map<SustainabilityCertification>(dto);
        var createdSustainabilityCertification = await _sustainabilityCertificationsRepository.CreateSustainabilityCertificationWithTransactionAsync(sustainabilityCertification);
        return _mapper.Map<SustainabilityCertificationsReadOnlyDTO>(createdSustainabilityCertification);
    }
    
    public async Task<SustainabilityCertificationsReadOnlyDTO?> GetSustainabilityCertificationByIdAsync(ulong id)
    {
        var sustainabilityCertification = await _sustainabilityCertificationsRepository.GetSustainabilityCertificationByIdAsync(id);
        return sustainabilityCertification == null ? null : _mapper.Map<SustainabilityCertificationsReadOnlyDTO>(sustainabilityCertification);
    }
    
    public async Task<SustainabilityCertificationsReadOnlyDTO> UpdateSustainabilityCertificationAsync(ulong id, SustainabilityCertificationsUpdateDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        if(!string.IsNullOrWhiteSpace(dto.Code))
        {
            var codeExists = await _sustainabilityCertificationsRepository.CheckCodeExistsAsync(dto.Code);
            if (codeExists)
            {
                throw new Exception($"Email {dto.Code} already exists.");
            }
        }
        
        var existingSustainabilityCertification = await _sustainabilityCertificationsRepository.GetSustainabilityCertificationByIdAsync(id);
        if (existingSustainabilityCertification == null)
        {
            throw new Exception($"Sustainability certification with ID {id} not found or inactive.");
        }

        _mapper.Map(dto, existingSustainabilityCertification);
        var updatedSustainabilityCertification = await _sustainabilityCertificationsRepository.UpdateSustainabilityCertificationWithTransactionAsync(existingSustainabilityCertification);
        return _mapper.Map<SustainabilityCertificationsReadOnlyDTO>(updatedSustainabilityCertification);
    }

    public async Task<PagedResponse<SustainabilityCertificationsReadOnlyDTO>> GetAllSustainabilityCertificationsAsync(int page, int pageSize, String? category = null)
    {
        var (sustainabilityCertifications, totalCount) = await _sustainabilityCertificationsRepository.GetAllSustainabilityCertificationsAsync(page, pageSize, category);
        var sustainabilityCertificationsDtos = _mapper.Map<List<SustainabilityCertificationsReadOnlyDTO>>(sustainabilityCertifications);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        
        return new PagedResponse<SustainabilityCertificationsReadOnlyDTO>
        {
            Data = sustainabilityCertificationsDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalCount,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
    
    public async Task<List<SustainabilityCertificationsCategoryReadOnlyDTO>> GetAllCategoriesAsync()
    {
        var categories = await _sustainabilityCertificationsRepository.GetAllCategoriesAsync();
        var result = categories
            .Select(c => new SustainabilityCertificationsCategoryReadOnlyDTO
            {
                Name = c.ToString()
            })
            .ToList();
        return result;
    }
}