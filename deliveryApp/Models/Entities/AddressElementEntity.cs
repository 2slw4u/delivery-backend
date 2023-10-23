namespace deliveryApp.Models.Entities
{
    public class AddressElementEntity
    {
        public Guid Id { get; set; }
        public int ObjectId { get; set; }
        public Guid ObjectGuid { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string Level { get; set; }
        public Boolean IsActive { get; set; }
        public List<HierarchyEntity> Hierarchies { get; set; }
    }
}
