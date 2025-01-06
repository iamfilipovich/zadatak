using Wiener.Models;

namespace Wiener.Repositories.Abstract
{
    public interface IPolicyService
    {
        Task<bool> AddPolicy(Policy policy);
        Task UpdatePartnerStatus(int partnerId);
    }
}