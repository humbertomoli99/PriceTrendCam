using PriceTrendCam.Core.Models;

namespace PriceTrendCam.Core.Contracts.Services;

// Remove this class once your pages/features are using your data.
public interface ISampleDataService<T>
{
    Task<IEnumerable<T>> GetContentGridDataAsync();

    Task<IEnumerable<T>> GetGridDataAsync();

    Task<IEnumerable<T>> GetListDetailsDataAsync();
}