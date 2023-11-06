using deliveryApp.Models.DTOs;

namespace deliveryApp.Services.Interfaces
{
    public interface IAddressService
    {
        Task<List<SearchAddressModel>> GetChildren(Guid parentObjectId, string query);
        Task<List<SearchAddressModel>> GetChain(Guid objectId);
    }
}
