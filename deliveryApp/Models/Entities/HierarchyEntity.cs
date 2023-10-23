namespace deliveryApp.Models.Entities
{
    public class HierarchyEntity
    {
        public Guid Id { get; set; }
        public int ObjectId { get; set; }
        public int ParentObjId { get; set; }
        public Boolean IsActive { get; set; }
    }
}
