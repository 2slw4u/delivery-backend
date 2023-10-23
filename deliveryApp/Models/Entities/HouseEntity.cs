namespace deliveryApp.Models.Entities
{
    public class HouseEntity
    {
        public Guid id { get; set; }
        public Guid Objectguid { get; set; }
        public int Housenum { get; set; }
        public int Addnum1 { get; set; }
        public int Addnum2 { get; set; }
        public int Addtype1 { get; set; }
        public int Addtype2 { get; set; }
        public Boolean Isactive { get; set; }
    }
}
