using PriceTrendCam.Core.Models;

namespace PriceTrendCam.Core.Contracts.Services;

// Remove this class once your pages/features are using your data.
public interface ISampleDataService<T>
{
    Task<IEnumerable<T>> GetContentGridDataAsync(int count = 10, int startIndex = 0);

    Task<IEnumerable<T>> GetGridDataAsync(int count = 10, int startIndex = 0);

    Task<IEnumerable<T>> GetListDetailsDataAsync(int count = 10, int startIndex = 0);
    Task<int> GetMaxPageCountAsync(int count = 10);

    Task<int> GetMaxRecordCountAsync();
}