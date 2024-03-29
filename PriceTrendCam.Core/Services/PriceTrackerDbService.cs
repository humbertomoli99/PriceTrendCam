﻿using PriceTrendCam.Core.Models;
using SQLite;

namespace PriceTrendCam.Core.Services;
public class PriceTrackerDbService
{
    private SQLiteAsyncConnection _database;
    public PriceTrackerDbService(string DbPath)
    {
        _database = new SQLiteAsyncConnection(DbPath);
        _ = CreateTablesAsync();
    }

    public async Task CreateTablesAsync()
    {
        await _database.CreateTableAsync<Store>().ConfigureAwait(false);
        await _database.CreateTableAsync<StoreUrl>().ConfigureAwait(false);
        await _database.CreateTableAsync<Selector>().ConfigureAwait(false);
        await _database.CreateTableAsync<ProductInfo>().ConfigureAwait(false);
        await _database.CreateTableAsync<ProductPhoto>().ConfigureAwait(false);
        await _database.CreateTableAsync<Notification>().ConfigureAwait(false);
        await _database.CreateTableAsync<History>().ConfigureAwait(false);
    }

    public async Task<List<T>> GetAllAsync<T>() where T : class, new()
    {
        return await _database.Table<T>().ToListAsync().ConfigureAwait(false);
    }

    public async Task<T> GetAsync<T>(int id) where T : class, new()
    {
        return await _database.FindAsync<T>(id).ConfigureAwait(false);
    }

    public async Task InsertAsync<T>(T item) where T : class, new()
    {
        await _database.InsertAsync(item).ConfigureAwait(false);
    }

    public async Task UpdateAsync<T>(T item) where T : class, new()
    {
        await _database.UpdateAsync(item).ConfigureAwait(false);
    }

    public async Task DeleteAsync<T>(T item) where T : class, new()
    {
        await _database.DeleteAsync(item).ConfigureAwait(false);
    }

}
