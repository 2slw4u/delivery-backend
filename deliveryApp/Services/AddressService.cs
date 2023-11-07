using deliveryApp.Models;
using deliveryApp.Models.DTOs;
using deliveryApp.Models.Enums;
using deliveryApp.Models.Exceptions;
using deliveryApp.Models.GAR;
using deliveryApp.Services.Interfaces;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using System.ComponentModel;
using System.Runtime.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace deliveryApp.Services
{
    public class AddressService : IAddressService
    {
        private readonly GarDbContext _context;
        private readonly ILogger<UserService> _logger;


        public AddressService(GarDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<SearchAddressModel>> GetChain(Guid objectGuid)
        {
            await ValidateAddressGuid(objectGuid);
            var objectId = await GetIdFromGuid(objectGuid);
            var result = new List<SearchAddressModel>();
            while (objectId != 0)
            {
                var objectEntity = await _context.AsAdmHierarchies.Where(x => x.Objectid == objectId).FirstOrDefaultAsync();
                objectId = (objectEntity == null) ? 0 : objectEntity.Parentobjid;
                result.Add(await ReformEntityIntoSearchAddressModel(objectEntity));
            }
            result.Reverse();
            _logger.LogInformation($"Chain of an object with {objectGuid} guid has been given out");
            return result;
        }

        public async Task<List<SearchAddressModel>> GetChildren(long parentObjectId, string? query)
        {
            var parentObjectGuid = await GetGuidFromId(parentObjectId);
            await ValidateAddressGuid(parentObjectGuid);
            var children = await _context.AsAdmHierarchies.Where(x => x.Parentobjid == parentObjectId).ToListAsync();
            var result = new List<SearchAddressModel>();
            foreach (var child in children)
            {
                var reformedChild = await ReformEntityIntoSearchAddressModel(child);
                if (query != null)
                {
                    //в один if не объединено, иначе будет ArgumentNullException
                    if (!reformedChild.Text.Contains(query))
                    {
                        continue;
                    }
                }
                result.Add(reformedChild);
            }
            _logger.LogInformation($"Children of {parentObjectGuid} object have been given out");
            return result;
        }
        private async Task<SearchAddressModel> ReformEntityIntoSearchAddressModel(AsAdmHierarchy entity)
        {
            var reformedEntity = new SearchAddressModel();
            var entityGuid = await GetGuidFromId(entity.Objectid);
            string text = "";
            if (await FindObjectLocation(entityGuid) == ObjectLocations.Houses)
            {
                var childInHouses = await _context.AsHouses.Where(x => x.Objectid == entity.Objectid).FirstOrDefaultAsync();
                text = childInHouses.Housenum;
                reformedEntity.ObjectLevel = GarAddressLevel.Building;
            }
            else
            {
                var childInAddresses = await _context.AsAddrObjs.Where(x => x.Objectid == entity.Objectid).FirstOrDefaultAsync();
                text = childInAddresses.Typename + " " + childInAddresses.Name;
                reformedEntity.ObjectLevel = (GarAddressLevel)Enum.Parse(typeof(GarAddressLevel), childInAddresses.Level);
            }
            reformedEntity.ObjectId = (long)entity.Objectid;
            reformedEntity.ObjectGuid = entityGuid;
            reformedEntity.Text = text;
            reformedEntity.ObjectLevelText = TranslateObjectLevel(reformedEntity.ObjectLevel);
            return reformedEntity;
        }
        private async Task<Guid> GetGuidFromId(long? objectId)
        {
            var objectEntityInHouses = await _context.AsHouses.Where(x => x.Objectid == objectId).FirstOrDefaultAsync();
            if (objectEntityInHouses == null)
            {
                var objectEntityInAddresses = await _context.AsAddrObjs.Where(x => x.Objectid == objectId).FirstOrDefaultAsync();
                if (objectEntityInAddresses == null)
                {
                    return Guid.Empty;
                }
                return objectEntityInAddresses.Objectguid;
            }
            return objectEntityInHouses.Objectguid;
            
        }
        private async Task<long?> GetIdFromGuid(Guid objectGuid)
        {
            var objectEntityInHouses = await _context.AsHouses.Where(x => x.Objectguid == objectGuid).FirstOrDefaultAsync();
            if (objectEntityInHouses == null)
            {
                var objectEntityInAddresses = await _context.AsAddrObjs.Where(x => x.Objectguid == objectGuid).FirstOrDefaultAsync();
                if (objectEntityInAddresses == null)
                {
                    return 0;
                }
                return objectEntityInAddresses.Objectid;
            }
            return objectEntityInHouses.Objectid;
        }
        private static String? TranslateObjectLevel(GarAddressLevel objectLevel)
        {
            String? objectLevelText = objectLevel switch
            {
                GarAddressLevel.Region => "Субъект РФ",
                GarAddressLevel.AdministrativeArea => "Административная область",
                GarAddressLevel.MunicipalArea => "Муниципальная область",
                GarAddressLevel.RuralUrbanSettlement => "Поселок городского типа",
                GarAddressLevel.City => "Город",
                GarAddressLevel.Locality => "Населенный пункт",
                GarAddressLevel.ElementOfPlanningStructure => "Элемент структуры планировки",
                GarAddressLevel.ElementOfRoadNetwork => "Элемент дорожной сети",
                GarAddressLevel.Land => "Земной участок",
                GarAddressLevel.Building => "Здание (сооружение)",
                GarAddressLevel.Room => "Комната",
                GarAddressLevel.RoomInRooms => "Комната в комнатах",
                GarAddressLevel.AutonomousRegionLevel => "Автономный региональный уровень",
                GarAddressLevel.IntracityLevel => "Внутригородской уровень",
                GarAddressLevel.AdditionalTerritoriesLevel => "Дополнительные территориальные уровни",
                GarAddressLevel.LevelOfObjectsInAdditionalTerritories => "Уровни объектов в дополнительных территориях",
                GarAddressLevel.CarPlace => "Стоянка для машины",
            };
            return objectLevelText;

        }
        public async Task ValidateAddressGuid(Guid? objectGuid)
        {
            if (objectGuid == Guid.Empty)
            {
                _logger.LogError("An empty Guid has been tried to be validated");
                throw new BadRequest("The Guid is empty");
            }
            var objectLocation = await FindObjectLocation(objectGuid);
            if (objectLocation == ObjectLocations.Houses)
            {
                await ValidateHouse(objectGuid);
            }
            if (objectLocation == ObjectLocations.Addresses)
            {
                await ValidateAddress(objectGuid);
            }
            if (objectLocation == null)
            {
                _logger.LogError($"{objectGuid} address has not been found in database");
                throw new BadRequest($"There is no address with {objectGuid} Guid");
            }
            _logger.LogInformation($"{objectGuid} object has been validated");
        }

        private async Task ValidateHouse(Guid? objectGuid)
        {
            var objectEntity = await _context.AsHouses.Where(x => x.Objectguid == objectGuid).FirstOrDefaultAsync();
            if (objectEntity == null)
            {
                _logger.LogError($"{objectGuid} building has not been found in database");
                throw new NotFound($"There is no building with {objectGuid} Guid");
            }
            if (objectEntity.Isactive == 0)
            {
                _logger.LogError($"{objectGuid} building is not active");
                throw new Conflict($"{objectGuid} building is not active");
            }
            if (objectEntity.Isactual == 0)
            {
                _logger.LogError($"{objectGuid} building is not actual");
                throw new Conflict($"{objectGuid} building is not actual");
            }
            _logger.LogInformation($"{objectGuid} has been validated confirmed to be a house");
        }
        private async Task ValidateAddress(Guid? objectGuid)
        {
            var objectEntity = await _context.AsAddrObjs.Where(x => x.Objectguid == objectGuid).FirstOrDefaultAsync();
            if (objectEntity == null)
            {
                _logger.LogError($"{objectGuid} address has not been found in database");
                throw new NotFound($"There is no address with {objectGuid} Guid");
            }
            if (objectEntity.Isactive == 0)
            {
                _logger.LogError($"{objectGuid} address is not active");
                throw new Conflict($"{objectGuid} address is not active");
            }
            if (objectEntity.Isactual == 0)
            {
                _logger.LogError($"{objectGuid} address is not actual");
                throw new Conflict($"{objectGuid} address is not actual");
            }
            _logger.LogInformation($"{objectGuid} address has been confirmed to be something other than a house");
        }
        private async Task<ObjectLocations?> FindObjectLocation(Guid? objectGuid)
        {
            if (await _context.AsHouses.Where(x => x.Objectguid == objectGuid).FirstOrDefaultAsync() != null)
            {
                _logger.LogInformation($"Object with {objectGuid} Guid has been confirmed to be a house");
                return ObjectLocations.Houses;
            }
            if (await _context.AsAddrObjs.Where(x => x.Objectguid == objectGuid).FirstOrDefaultAsync() != null)
            {
                _logger.LogInformation($"Object with {objectGuid} Guid has been confirmed to be something other than a house");
                return ObjectLocations.Addresses;
            }
            _logger.LogInformation($"Object with {objectGuid} Guid has not been found anywhere");
            return null;
        }
    }
}
