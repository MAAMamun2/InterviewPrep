using InterviewPrep.Domain.Entities;

namespace InterviewPrep.Application.Interfaces;

public interface IPurchaseRepository
{
    IQueryable<Purchase> Query();
    Task AddAsync(Purchase purchase, CancellationToken ct = default);
}
