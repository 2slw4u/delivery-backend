using deliveryApp.Models.DTOs;
using deliveryApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace deliveryApp.Controllers
{
    [ApiController]
    [Route("api/address")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;
        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }
        [HttpGet]
        [Route("search")]
        public async Task<List<SearchAddressModel>> GetChildren(long parentObjectId, string? query)
        {
            return await _addressService.GetChildren(parentObjectId, query);
        }
        [HttpGet]
        [Route("chain")]
        public async Task<List<SearchAddressModel>> GetChain(Guid objectGuid)
        {
            return await _addressService.GetChain(objectGuid);
        }
        [HttpGet]
        [Route("validate")]
        public async Task Validateguid(Guid objectGuid)
        {
            await _addressService.ValidateAddressGuid(objectGuid);
        }
    }
}
