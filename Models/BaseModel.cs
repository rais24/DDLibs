namespace Utils.Models
{
    public class BaseModel
    {
        virtual public int Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
