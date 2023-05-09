using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PriceTrendCam.Contracts.ViewModels;
using PriceTrendCam.Models;

namespace PriceTrendCam.ViewModels;
public partial class ControlPagerViewModel : ControlPagerModel
{
    public void Pagination(int totalItemsCount, int defaultRowsPerPage = 10)
    {
        TotalItemsCount = totalItemsCount;
        SelectedRowsPerPageOption = defaultRowsPerPage;
    }

    [RelayCommand(CanExecute = nameof(CanMoveToFirstPage))]
    private async Task MoveToFirstPage()
    {
        CurrentPageIndex = 0;
        await UpdatePageCommands();
    }

    [RelayCommand(CanExecute = nameof(CanMoveToPreviousPage))]
    private async Task MoveToPreviousPage()
    {
        CurrentPageIndex--;
        await UpdatePageCommands();
    }

    [RelayCommand(CanExecute = nameof(CanMoveToNextPage))]
    private async Task MoveToNextPage()
    {
        CurrentPageIndex++;
        await UpdatePageCommands();
    }

    [RelayCommand(CanExecute = nameof(CanMoveToLastPage))]
    private async Task MoveToLastPage()
    {
        CurrentPageIndex = TotalPagesCount - 1;
        await UpdatePageCommands();
    }


    private bool CanMoveToFirstPage() => CurrentPageIndex > 0;
    private bool CanMoveToPreviousPage() => CurrentPageIndex > 0;
    private bool CanMoveToNextPage() => CurrentPageIndex < TotalPagesCount - 1;
    private bool CanMoveToLastPage() => CurrentPageIndex != TotalPagesCount - 1;

    private async Task UpdatePageCommands()
    {
        MoveToPreviousPageCommand.NotifyCanExecuteChanged();
        MoveToNextPageCommand.NotifyCanExecuteChanged();
        MoveToLastPageCommand.NotifyCanExecuteChanged();
        MoveToFirstPageCommand.NotifyCanExecuteChanged();

        //var isAscending = (previousSelectedSortDirection == "Ascending");
        //await GetOrderedList(OrderBy, isAscending, CurrentPageIndex, SelectedRowsPerPageOption);
    }

    public async Task OnSelectedItemChanged()
    {
        //if (ProductsList == null) return;
        //var isAscending = (previousSelectedSortDirection == "Ascending");
        //await GetOrderedList(OrderBy, isAscending, CurrentPageIndex, SelectedRowsPerPageOption);
        //await UpdatePageCommands();
    }
}
