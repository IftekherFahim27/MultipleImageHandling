using System.ComponentModel.DataAnnotations;

namespace MultipleImageHandling.Models
{
    public class ItemDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = "";
        [Required]
        public int Unit { get; set; }
        [Required]
        public int Quantity { get; set; }


        public List<IFormFile>? ImageFiles { get; set; }
    }
}
