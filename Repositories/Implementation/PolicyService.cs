using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Wiener.Models;
using Wiener.Repositories.Abstract;

namespace Wiener.Repositories.Implementation
{
    public class PolicyService : IPolicyService
    {
        private readonly IDbConnection _db;

        public PolicyService (IDbConnection db) 
        {
            _db = db;
        }

        public async Task<bool> AddPolicy(Policy policy)
        {
            if (_db.State == ConnectionState.Closed)
            {
                if (_db is SqlConnection sqlConnection)
                {
                    await sqlConnection.OpenAsync(); 
                }
                else
                {
                    throw new InvalidOperationException("The database connection does not support asynchronous operations.");
                }
            }

            using (var transaction = _db.BeginTransaction())
            {
                try
                {
                    var partnerExists = await _db.ExecuteScalarAsync<int>(
                        "SELECT COUNT(1) FROM Partners WHERE Id = @PartnerId",
                        new { PartnerId = policy.PartnerId }, transaction);

                    if (partnerExists == 0)
                    {
                        throw new Exception($"Partner with Id {policy.PartnerId} does not exist.");
                    }

                    var result = await _db.ExecuteAsync(
                        "INSERT INTO Policies (PartnerId, PolicyNumber, PolicyAmount) " +
                        "VALUES (@PartnerId, @PolicyNumber, @PolicyAmount)",
                        new
                        {
                            policy.PartnerId,
                            policy.PolicyNumber,
                            policy.PolicyAmount
                        }, transaction);

                    transaction.Commit();
                    return result > 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    throw;
                }
            }
        }
        public async Task UpdatePartnerStatus(int partnerId)
        {
            var partner = await _db.QuerySingleOrDefaultAsync<Partner>(
                "SELECT * FROM Partners WHERE Id = @PartnerId",
                new { PartnerId = partnerId }
            );

            if (partner != null)
            {
                var policies = await _db.QueryAsync<Policy>(
                    "SELECT * FROM Policies WHERE PartnerId = @PartnerId",
                    new { PartnerId = partner.Id }
                );

                int policyCount = policies.Count();

                decimal totalPolicyAmount = policies.Sum(p => p.PolicyAmount);

                bool needsStar = policyCount > 5 || totalPolicyAmount > 5000;

                if (needsStar && !partner.FirstName.StartsWith("* "))
                {
                    partner.FirstName = "* " + partner.FirstName;
                }
                else if (!needsStar && partner.FirstName.StartsWith("* "))
                {
                    partner.FirstName = partner.FirstName.Substring(2); 
                }

                await _db.ExecuteAsync(
                    "UPDATE Partners SET FirstName = @FirstName WHERE Id = @Id",
                    new { FirstName = partner.FirstName, Id = partner.Id }
                );
            }
        }

    }
}
