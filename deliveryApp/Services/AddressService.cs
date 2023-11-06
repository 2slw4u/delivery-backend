using deliveryApp.Models;
using deliveryApp.Models.DTOs;
using deliveryApp.Models.Enums;
using deliveryApp.Models.Exceptions;
using deliveryApp.Models.GAR;
using deliveryApp.Services.Interfaces;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace deliveryApp.Services
{
    public class AddressService : IAddressService
    {
        private readonly GarDbContext _context;

        public AddressService(GarDbContext context)
        {
            _context = context;
        }
        public async Task<List<SearchAddressModel>> GetChain(Guid objectGuid)
        {

            throw new NotImplementedException();
        }

        public async Task<List<SearchAddressModel>> GetChildren(long? parentObjectId, string query)
        {
            var parentObjectGuid = await GetGuidFromId(parentObjectId);
            await ValidateGuid(parentObjectGuid);
            var children = await _context.AsAdmHierarchies.Where(x => x.Parentobjid == parentObjectId).ToListAsync();
            var result = new List<SearchAddressModel>();
            foreach (var child in children)
            {
                var reformedChild = await ReformEntity(child);
                if (!reformedChild.Text.Contains(query) && query!="")
                {
                    continue;
                }
                result.Add(reformedChild);
            }
            return result;
        }
        private async Task<SearchAddressModel> ReformEntity(AsAdmHierarchy entity)
        {
            var reformedEntity = new SearchAddressModel();
            var entityGuid = await GetGuidFromId(entity.Objectid);
            string text = "";
            if (await FindObjectLocation(entityGuid) == ObjectLocations.Houses)
            {
                var childInHouses = await _context.AsHouses.Where(x => x.Objectguid == entityGuid).FirstOrDefaultAsync();
                text = childInHouses.Housenum;
                reformedEntity.ObjectLevel = GarAddressLevel.Building;
            }
            else
            {
                var childInAddresses = await _context.AsAddrObjs.Where(x => x.Objectguid == entityGuid).FirstOrDefaultAsync();
                text = childInAddresses.Typename + childInAddresses.Name;
                reformedEntity.ObjectLevel = (GarAddressLevel)Enum.Parse(typeof(GarAddressLevel), childInAddresses.Level);
            }
            reformedEntity.ObjectId = entity.Id;
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
        private async Task ValidateGuid(Guid objectGuid)
        {
            if (objectGuid == Guid.Empty)
            {
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
            throw new BadRequest("Threre is no such object");
        }

        private async Task ValidateHouse(Guid objectGuid)
        {
            var objectEntity = await _context.AsHouses.Where(x => x.Objectguid == objectGuid).FirstOrDefaultAsync();
            if (objectEntity == null)
            {
                throw new NotFound("There is no such building");
            }
            if (objectEntity.Isactive == 0)
            {
                throw new Conflict("Selected building is inactive");
            }
            if (objectEntity.Isactual == 0)
            {
                throw new Conflict("Selected building is not actual");
            }
        }
        private async Task ValidateAddress(Guid objectGuid)
        {
            var objectEntity = await _context.AsAddrObjs.Where(x => x.Objectguid == objectGuid).FirstOrDefaultAsync();
            if (objectEntity == null)
            {
                throw new NotFound("There is no such address");
            }
            if (objectEntity.Isactive == 0)
            {
                throw new Conflict("Selected address is inactive");
            }
            if (objectEntity.Isactual == 0)
            {
                throw new Conflict("Selected address is not actual");
            }
        }
        private async Task<ObjectLocations?> FindObjectLocation(Guid objectGuid)
        {
            if (await _context.AsHouses.Where(x => x.Objectguid == objectGuid).FirstOrDefaultAsync() != null)
            {
                return ObjectLocations.Houses;
            }
            if (await _context.AsAddrObjs.Where(x => x.Objectguid == objectGuid).FirstOrDefaultAsync() != null)
            {
                return ObjectLocations.Addresses;
            }
            return null;
        }
    }
}
