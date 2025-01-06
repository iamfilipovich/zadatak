using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wiener.Models
{
    public class Policy
    {
        public int Id { get; set; }

        [Required]
        public int PartnerId { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 10)]
        public string PolicyNumber { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "PolicyAmount must be a positive value.")]
        public decimal PolicyAmount { get; set; }
        
    }
}
