using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wiener.Models
{
    public class Partner
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(25, MinimumLength = 2)]
        public string FirstName { get; set; }//

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(25, MinimumLength = 2)]
        public string LastName { get; set; }//

        public string? Address { get; set; }//

        [Required(ErrorMessage = "Partner Number is required.")]
        [RegularExpression(@"^\d{20}$", ErrorMessage = "Partner Number must be exactly 20 digits.")]
        public string PartnerNumber { get; set; }

        [MaxLength(11)]
        public string? CroatianPIN { get; set; }

        [Required]
        [Range(1, 2)]
        public int PartnerTypeId { get; set; }

        public DateTime? CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "This field is required.")]
        [EmailAddress]
        public string CreateByUser { get; set; }//

        [Required]
        public bool IsForeign { get; set; }

        [StringLength(20, MinimumLength = 10)]
        public string ExternalCode { get; set; }//

        [Required]
        public string Gender { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        public List<Policy> Policies { get; set; } = new List<Policy>();
        public bool IsNew { get; set; } = false;
    }
}

