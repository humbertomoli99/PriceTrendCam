﻿using System.Diagnostics;
using PriceTrendCam.Core.Models;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;

namespace PriceTrendCam.Core.Services;
/// <summary>
/// Class that contains methods for accessing and manipulating the database.
/// </summary>
public class PriceTrackerDbService
{
    private SQLiteAsyncConnection _database;

    /// <summary>
    /// Initializes a new instance of the <see cref="PriceTrackerDbService"/> class.
    /// </summary>
    /// <param name="DbPath">The path of the database file.</param>
    public PriceTrackerDbService(string DbPath)
    {
        _database = new SQLiteAsyncConnection(DbPath);
        _ = CreateTablesAsync();
    }

    /// <summary>
    /// Creates the necessary tables for the database.
    /// </summary>
    /// <returns>A task representing the asynchronous creation of the tables.</returns>
    public async Task CreateTablesAsync()
    {
        await _database.CreateTableAsync<Store>().ConfigureAwait(false);
        await _database.CreateTableAsync<StoreUrl>().ConfigureAwait(false);
        await _database.CreateTableAsync<Selector>().ConfigureAwait(false);
        await _database.CreateTableAsync<ProductInfo>().ConfigureAwait(false);
        await _database.CreateTableAsync<ProductPhoto>().ConfigureAwait(false);
        await _database.CreateTableAsync<Notification>().ConfigureAwait(false);
        await _database.CreateTableAsync<History>().ConfigureAwait(false);
        await _database.CreateTableAsync<RegexMethod>().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all records of the specified type from the database.
    /// </summary>
    /// <typeparam name="T">The type of records to retrieve.</typeparam>
    /// <returns>A list of all records of the specified type in the database.</returns>
    public async Task<List<T>> GetAllAsync<T>() where T : class, new()
    {
        return await _database.Table<T>().ToListAsync().ConfigureAwait(false);
    }
    public async Task<List<T>> GetAllWithChildrenAsync<T>() where T : class, new()
    {
        try
        {
            return await _database.GetAllWithChildrenAsync<T>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al obtener los datos de la tabla {typeof(T).Name}: {ex.Message}");
            return null;
        }
    }
    public async Task<T> GetWithChildrenAsync<T>(int id) where T : class, new()
    {
        return await _database.FindWithChildrenAsync<T>(id).ConfigureAwait(false);
    }
    /// <summary>
    /// Gets a single record of the specified type from the database, based on its ID.
    /// </summary>
    /// <typeparam name="T">The type of the record to retrieve.</typeparam>
    /// <param name="id">The ID of the record to retrieve.</param>
    /// <returns>The record with the specified ID, or `null` if no such record exists.</returns>
    public async Task<T> GetAsync<T>(int id) where T : class, new()
    {
        return await _database.FindAsync<T>(id).ConfigureAwait(false);
    }

    /// <summary>
    /// Inserts a collection of records into the database.
    /// </summary>
    /// <typeparam name="T">The type of the records to insert.</typeparam>
    /// <param name="items">The collection of records to insert.</param>
    /// <returns>A task representing the asynchronous insertion of the records.</returns>
    public async Task InsertAllAsync<T>(IEnumerable<T> items) where T : class, new()
    {
        await _database.InsertAllAsync(items).ConfigureAwait(false);
    }
    public async Task InsertAllWithChildrenAsync<T>(IEnumerable<T> items) where T : class, new()
    {
        await _database.InsertAllWithChildrenAsync(items).ConfigureAwait(false);
    }
    public async Task InsertWithChildrenAsync<T>(T item, bool recursive) where T : class, new()
    {
        await _database.InsertWithChildrenAsync(item, recursive: false).ConfigureAwait(false);
    }
    /// <summary>
    /// Inserts a single item into the database.
    /// </summary>
    /// <typeparam name="T">The type of the item to be inserted.</typeparam>
    /// <param name="item">The item to be inserted.</param>
    /// <returns>A task that represents the asynchronous insert operation. The task result contains the number of rows inserted, which should be 1 for a successful insert.</returns>
    public async Task<int> InsertAsync<T>(T item) where T : class, new()
    {
        return await _database.InsertAsync(item).ConfigureAwait(false);
    }
    /// <summary>
    /// Updates a single item in the database.
    /// </summary>
    /// <typeparam name="T">The type of the item to be updated.</typeparam>
    /// <param name="item">The item to be updated.</param>
    /// <returns>A task that represents the asynchronous update operation. The task result indicates whether the update was successful or not.</returns>
    public async Task<bool> UpdateAsync<T>(T item) where T : class, new()
    {
        var result = await _database.UpdateAsync(item).ConfigureAwait(false);
        return result > 0;
    }
    public async Task UpdateWithChildrenAsync<T>(T item) where T : class, new()
    {
        await _database.UpdateWithChildrenAsync(item);
    }
    /// <summary>
    /// Deletes a single item from the database.
    /// </summary>
    /// <typeparam name="T">The type of the item to be deleted.</typeparam>
    /// <param name="id">The id of the item to be deleted.</param>
    /// <returns>A task that represents the asynchronous delete operation. The task result indicates whether the delete was successful or not.</returns>
    public async Task<bool> DeleteAsync<T>(int id) where T : class, new()
    {
        var result = await _database.DeleteAsync<T>(id).ConfigureAwait(false);
        return result > 0;
    }
    /// <summary>
    /// Closes the connection to the database.
    /// </summary>
    public async Task CloseAsync()
    {
        await _database.CloseAsync().ConfigureAwait(false);
    }
}