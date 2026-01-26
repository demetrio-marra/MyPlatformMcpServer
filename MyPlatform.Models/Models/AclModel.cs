namespace MyPlatformModels.Models
{
    public class AclModel
    {
        public IEnumerable<AclModelDetail> AclDetails { get; set; } = new List<AclModelDetail>();
    }

    public class AclModelDetail
    {
        public int? Company { get; set; }
        public int? Family { get; set; }
        public int? Product { get; set; }
    }
}
