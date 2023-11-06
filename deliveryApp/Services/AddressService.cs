using deliveryApp.Models;
using deliveryApp.Models.DTOs;
using deliveryApp.Models.GAR;
using deliveryApp.Services.Interfaces;

namespace deliveryApp.Services
{
    public class AddressService : IAddressService
    {
        private readonly GarDbContext _context;

        public AddressService(GarDbContext context)
        {
            _context = context;
        }
        public Task<List<SearchAddressModel>> GetChain(Guid objectId)
        {
            throw new NotImplementedException();
        }

        public Task<List<SearchAddressModel>> GetChildren(Guid parentObjectId, string query)
        {
            throw new NotImplementedException();
        }
    }
}
