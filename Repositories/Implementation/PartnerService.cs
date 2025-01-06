using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Wiener.Models;
using Wiener.Repositories.Abstract;

namespace Wiener.Repositories.Implementation
{
    public class PartnerService : IPartnerService
    {
        private readonly IDbConnection _db;

        public PartnerService(IDbConnection db)
        {
            _db = db;
        }
        public async Task AddPartner(Partner partner)
        {
            var partnerExists = await _db.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(1) FROM Partners WHERE PartnerNumber = @PartnerNumber",
                new { PartnerNumber = partner.PartnerNumber });

            if (partnerExists > 0)
            {
                throw new ArgumentException("Partner with the same PartnerNumber already exists.");
            }

            var externalCodeExists = await _db.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(1) FROM Partners WHERE ExternalCode = @ExternalCode",
                new { ExternalCode = partner.ExternalCode });

            if (externalCodeExists > 0)
            {
                throw new ArgumentException("Partner with the same ExternalCode already exists.");
            }

            partner.IsNew = true;
            var result = await _db.ExecuteAsync(
                "INSERT INTO Partners (FirstName, LastName, Address, PartnerNumber, CroatianPIN, PartnerTypeId, CreatedAtUtc, CreateByUser, IsForeign, ExternalCode, Gender, IsNew) " +
                "VALUES (@FirstName, @LastName, @Address, @PartnerNumber, @CroatianPIN, @PartnerTypeId, @CreatedAtUtc, @CreateByUser, @IsForeign, @ExternalCode, @Gender, @IsNew)",
                new
                {
                    FirstName = partner.FirstName,
                    LastName = partner.LastName,
                    Address = partner.Address,
                    PartnerNumber = partner.PartnerNumber,
                    CroatianPIN = partner.CroatianPIN,
                    PartnerTypeId = partner.PartnerTypeId,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreateByUser = partner.CreateByUser,
                    IsForeign = partner.IsForeign,
                    ExternalCode = partner.ExternalCode,
                    Gender = partner.Gender,
                    IsNew = partner.IsNew
                });

            if (result == 0)
            {
                throw new Exception("Failed to create partner.");
            }
        }

        public async Task<IEnumerable<Partner>> GetAllPartnersAsync()
        {
            var query = "SELECT * FROM Partners ORDER BY CreatedAtUtc DESC";
            return await _db.QueryAsync<Partner>(query);
        }
        public async Task<Partner> GetPartnerByIdAsync(int id)
        {
            var partner = await _db.QuerySingleOrDefaultAsync<Partner>(
            "SELECT * FROM Partners WHERE Id = @Id",
            new { Id = id });

            if (partner == null)
            {
                throw new Exception($"Partner with Id {id} not found.");
            }

            return partner;
        }
        public async Task<bool> DeletePartnerAsync(string partnerNumber)
        {
            try
            {
                var partner = await _db.QueryFirstOrDefaultAsync<Partner>(
                    "SELECT * FROM Partners WHERE PartnerNumber = @PartnerNumber",
                    new { PartnerNumber = partnerNumber });

                if (partner == null)
                {
                    return false;
                }

                await _db.ExecuteAsync(
                    "DELETE FROM Policies WHERE PartnerId = @PartnerId",
                    new { PartnerId = partner.Id });

                var result = await _db.ExecuteAsync(
                    "DELETE FROM Partners WHERE PartnerNumber = @PartnerNumber",
                    new { PartnerNumber = partnerNumber });

                return result > 0; 
            }
            catch (Exception ex)
            {
                throw new Exception("Došlo je do greške prilikom brisanja partnera i povezanih polisa.", ex);
            }
        }

        public async Task<bool> UpdatePartnerAsync(Partner partner)
        {
            try
            {
                var partnerExists = await _db.QueryFirstOrDefaultAsync<int>(
                @"SELECT COUNT(1) 
              FROM Partners 
              WHERE PartnerNumber = @PartnerNumber AND Id != @Id", 
                new { PartnerNumber = partner.PartnerNumber, Id = partner.Id });

                if (partnerExists > 0)
                {
                    throw new ArgumentException("Partner with the same PartnerNumber already exists.");
                }

                var externalCodeExists = await _db.QueryFirstOrDefaultAsync<int>(
            @"SELECT COUNT(1) 
              FROM Partners 
              WHERE ExternalCode = @ExternalCode AND Id != @Id",
            new { ExternalCode = partner.ExternalCode, Id = partner.Id });

                if (externalCodeExists > 0)
                {
                    throw new ArgumentException("Partner with the same ExternalCode already exists.");
                }

                var query = @"
                UPDATE Partners 
                SET FirstName = @FirstName,
                LastName = @LastName,
                PartnerNumber = @PartnerNumber,
                Gender = @Gender,
                Address = @Address,
                CroatianPIN = @CroatianPIN,
                PartnerTypeId = @PartnerTypeId,
                IsForeign = @IsForeign,
                ExternalCode = @ExternalCode,
                CreateByUser = @CreateByUser
                WHERE Id = @Id"; 

                var result = await _db.ExecuteAsync(query, new
                {
                    partner.FirstName,
                    partner.LastName,
                    partner.PartnerNumber,
                    partner.Gender,
                    partner.Address,
                    partner.CroatianPIN,
                    partner.PartnerTypeId,
                    partner.IsForeign,  
                    partner.ExternalCode,
                    partner.CreateByUser,
                    partner.Id
                });

                return result > 0; 
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while updating the partner.", ex);
            }
        }
        public async Task<bool> PartnerExistsAsync(string partnerNumber)
        {
            var existingPartner = await _db.QueryFirstOrDefaultAsync<int>(
                "SELECT 1 FROM Partners WHERE PartnerNumber = @PartnerNumber",
                new { PartnerNumber = partnerNumber });

            return existingPartner > 0;
        }
        public async Task MarkPartnerAsOldAsync(int partnerId)
        {
            try
            {
                string query = "UPDATE Partners SET IsNew = 0";
                var rowsAffected = await _db.ExecuteAsync(query, new { Id = partnerId });

                if (rowsAffected == 0)
                {
                    throw new Exception($"Partner with Id {partnerId} not found or already marked as old.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while marking the partner as old.", ex);
            }
        }
        public async Task<bool> ExternalCodeExistsAsync(string externalCode)
        {
            var count = await _db.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(1) FROM Partners WHERE ExternalCode = @ExternalCode",
                new { ExternalCode = externalCode });

            return count > 0;
        }
    }
}
