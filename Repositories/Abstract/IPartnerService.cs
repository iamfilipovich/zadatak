using Wiener.Models;

namespace Wiener.Repositories.Abstract
{
    public interface IPartnerService
    {
        Task AddPartner(Partner partner);
        Task<IEnumerable<Partner>> GetAllPartnersAsync();
        Task<Partner> GetPartnerByIdAsync(int id);
        Task<bool> DeletePartnerAsync(string partnerNumber);
        Task<bool> UpdatePartnerAsync(Partner partner);
        Task<bool> PartnerExistsAsync(string partnerNumber);
        Task MarkPartnerAsOldAsync(int partnerId);
        Task<bool> ExternalCodeExistsAsync(string externalCode);
    }
}