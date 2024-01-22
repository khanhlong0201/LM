using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace LM.API.Infrastructure;

public interface IBMDbContext
{
    Task Connect(); // kết nối tới db 
    Task DisConnect(); // ngắt kết nối tới db
    Task BeginTranAsync();
    Task CommitTranAsync();
    Task RollbackAsync();
    Task<IEnumerable<T>> GetDataAsync<T>(string commandText, Func<IDataReader, T> dataFunc, IEnumerable<SqlParameter>? sqlParameters = null, CommandType commandType = CommandType.StoredProcedure);
    Task<T> GetDataByIdAsync<T>(string commandText, Func<IDataReader, T> dataFunc, IEnumerable<SqlParameter>? sqlParameters = null, CommandType commandType = CommandType.StoredProcedure);
    Task<DataSet> GetDataSetAsync(string commandText, IEnumerable<SqlParameter>? sqlParameters = null, CommandType commandType = CommandType.StoredProcedure);
    Task<DataTable> AddOrUpdateAsync(string commandText, IEnumerable<SqlParameter> sqlParameters, CommandType commandType = CommandType.StoredProcedure);
    Task<object?> ExcecFuntionAsync(string commandText, IEnumerable<SqlParameter>? sqlParameters = null);
    Task<int> ExecuteScalarAsync(string commandText, IEnumerable<SqlParameter>? sqlParameters = null);
    Task<int> DeleteDataAsync(string commandText, IEnumerable<SqlParameter>? sqlParameters = null);
    Task<object?> ExecuteScalarObjectAsync(string commandText, IEnumerable<SqlParameter>? sqlParameters = null);
}
public class LMDbContext : IBMDbContext
{
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;
    private SqlConnection? sqlConnection { get; set; }
    private SqlCommand? sqlCommand { get; set; }
    SqlTransaction? sqlTransaction { get; set; }
    public LMDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("BMConnection");
        sqlConnection = new SqlConnection(_connectionString);
    }

    /// <summary>
    /// mở connection sql
    /// hainguyen created
    /// </summary>
    /// <returns></returns>
    public async Task Connect()
    {
        if (sqlConnection == null) sqlConnection = new SqlConnection(_connectionString);
        if (sqlConnection.State != ConnectionState.Open) await sqlConnection.OpenAsync();
    }

    /// <summary>
    /// đóng connection sql
    /// hainguyen created
    /// </summary>
    /// <returns></returns>
    public async Task DisConnect()
    {
        if (sqlConnection is not null && sqlConnection.State != ConnectionState.Closed)
        {
            await sqlConnection.CloseAsync();
            //await sqlConnection.DisposeAsync(); // giải phóng tài nguyên
        }
    }

    /// <summary>
    /// begin tran
    /// </summary>
    /// <returns></returns>
    public async Task BeginTranAsync()
    {
        if(sqlConnection is not null && sqlConnection.State == ConnectionState.Open)
        {
            sqlTransaction = (SqlTransaction?)await sqlConnection.BeginTransactionAsync();
        }    
    }

    /// <summary>
    /// commit tran
    /// </summary>
    /// <returns></returns>
    public async Task CommitTranAsync()
    {
        if(sqlTransaction is not null) await sqlTransaction.CommitAsync();
    }

    /// <summary>
    /// commit tran
    /// </summary>
    /// <returns></returns>
    public async Task RollbackAsync()
    {
        if (sqlTransaction is not null) await sqlTransaction.RollbackAsync();
    }

    /// <summary>
    /// get data async with command text is: sql query string, store proc
    /// hainguyen created
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="dataFunc"></param>
    /// <param name="sqlParameters"></param>
    /// <param name="commandType"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<IEnumerable<T>> GetDataAsync<T>(string commandText, Func<IDataReader, T> dataFunc, IEnumerable<SqlParameter>? sqlParameters = null, CommandType commandType = CommandType.StoredProcedure)
    {
        sqlCommand = new SqlCommand();
        sqlCommand.Connection = sqlConnection;
        sqlCommand.CommandTimeout = 500;
        sqlCommand.CommandText = commandText;
        sqlCommand.CommandType = commandType;
        if (sqlParameters != null && sqlParameters.Any()) sqlCommand.Parameters.AddRange(sqlParameters.ToArray());
        List<T> listData = new List<T>();
        using (IDataReader reader = await sqlCommand.ExecuteReaderAsync())
        {
            while (reader.Read())
            {
                T record = dataFunc(reader); // đọc record
                listData.Add(record);
            }
            reader.Close();
            await sqlCommand.DisposeAsync(); // giải phóng tài nguyên
        }
        return listData;
    }

    /// <summary>
    /// Get data by id async with command text is: sql query string, store proc
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="dataFunc"></param>
    /// <param name="sqlParameters"></param>
    /// <param name="commandType"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<T> GetDataByIdAsync<T>(string commandText, Func<IDataReader, T> dataFunc, IEnumerable<SqlParameter>? sqlParameters = null, CommandType commandType = CommandType.StoredProcedure)
    {
        sqlCommand = new SqlCommand();
        sqlCommand.Connection = sqlConnection;
        sqlCommand.CommandTimeout = 500;
        sqlCommand.CommandText = commandText;
        sqlCommand.CommandType = commandType;
        if (sqlParameters != null && sqlParameters.Any()) sqlCommand.Parameters.AddRange(sqlParameters.ToArray());
        using (IDataReader reader = await sqlCommand.ExecuteReaderAsync())
        {
            T record = Activator.CreateInstance<T>();
            while (reader.Read())
            {
                record = dataFunc(reader); // đọc record
            }
            reader.Close();
            await sqlCommand.DisposeAsync(); // giải phóng tài nguyên
            return record;
        }
    }

    /// <summary>
    /// Get multiple table in query string/store proc
    /// hainguyen created
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="sqlParameters"></param>
    /// <returns></returns>
    public async Task<DataSet> GetDataSetAsync(string commandText, IEnumerable<SqlParameter>? sqlParameters = null, CommandType commandType = CommandType.StoredProcedure)
    {

        sqlCommand = new SqlCommand();
        sqlCommand.Connection = sqlConnection;
        sqlCommand.CommandTimeout = 500;
        sqlCommand.CommandText = commandText;
        sqlCommand.CommandType = commandType;
        if (sqlParameters != null && sqlParameters.Any()) sqlCommand.Parameters.AddRange(sqlParameters.ToArray());
        DataSet ds = new DataSet();
        using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
        {
            sqlDataAdapter.SelectCommand = sqlCommand;
            sqlDataAdapter.Fill(ds);
            await sqlCommand.DisposeAsync(); // giải phóng tài nguyên
            return ds;
        }

    }

    /// <summary>
    /// Call store Add Or Update data
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="sqlParameters"></param>
    /// <param name="commandType"></param>
    /// <returns></returns>
    public async Task<DataTable> AddOrUpdateAsync(string commandText, IEnumerable<SqlParameter> sqlParameters, CommandType commandType = CommandType.StoredProcedure)
    {
        sqlCommand = new SqlCommand();
        sqlCommand.Connection = sqlConnection;
        sqlCommand.CommandTimeout = 500;
        sqlCommand.CommandText = commandText;
        sqlCommand.CommandType = commandType;
        sqlCommand.Transaction = sqlTransaction;
        if (sqlParameters != null && sqlParameters.Any()) sqlCommand.Parameters.AddRange(sqlParameters.ToArray());
        DataTable dt = new DataTable();
        if (commandType == CommandType.StoredProcedure)
        {
            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
            {
                sqlDataAdapter.SelectCommand = sqlCommand;
                sqlDataAdapter.Fill(dt);
                await sqlCommand.DisposeAsync(); // giải phóng tài nguyên
                return dt;
            }
        } 
        else
        {
            DataColumn column = new DataColumn("StatusCode", typeof(string));
            dt.Columns.Add(column);
            column = new DataColumn("ErrorMessage", typeof(string));
            dt.Columns.Add(column);
            // Thêm dòng mới
            DataRow row = dt.NewRow();
            // Thực hiện truy vấn UPDATE
            int rowsAffected = sqlCommand.ExecuteNonQuery();
            if(rowsAffected > 0)
            {
                row["StatusCode"] = 0;
                row["ErrorMessage"] = "Success";
                dt.Rows.Add(row);
            } 
            else
            {
                row["StatusCode"] = -1;
                row["ErrorMessage"] = "Error";
                dt.Rows.Add(row);
            }    
            return dt;
        }    
        
    }

    /// <summary>
    /// call funtion 
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="sqlParameters"></param>
    /// <returns></returns>
    public async Task<object?> ExcecFuntionAsync(string commandText, IEnumerable<SqlParameter>? sqlParameters = null)
    {
        sqlCommand = new SqlCommand();
        sqlCommand.Connection = sqlConnection;
        sqlCommand.CommandTimeout = 500;
        sqlCommand.CommandText = commandText;
        sqlCommand.CommandType = CommandType.StoredProcedure;
        if (sqlParameters != null && sqlParameters.Any()) sqlCommand.Parameters.AddRange(sqlParameters.ToArray());
        SqlParameter returnValue = sqlCommand.Parameters.Add("@RETURN_VALUE", SqlDbType.VarChar);
        returnValue.Direction = ParameterDirection.ReturnValue;
        await sqlCommand.ExecuteNonQueryAsync();
        return returnValue.Value;
    }

    /// <summary>
    /// call ra một cột
    /// </summary>
    /// <param name="commandText"></param>
    /// <returns></returns>
    public async Task<int> ExecuteScalarAsync(string commandText, IEnumerable<SqlParameter>? sqlParameters = null)
    {
        sqlCommand = new SqlCommand();
        sqlCommand.Connection = sqlConnection;
        sqlCommand.CommandTimeout = 500;
        sqlCommand.CommandText = commandText;
        sqlCommand.CommandType = CommandType.Text;
        sqlCommand.Transaction = sqlTransaction;
        if (sqlParameters != null && sqlParameters.Any()) sqlCommand.Parameters.AddRange(sqlParameters.ToArray());
        int scopeIdentity = Convert.ToInt32(await sqlCommand.ExecuteScalarAsync());
        return scopeIdentity;
    }

    /// <summary>
    /// xóa dữ liệu
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="sqlParameters"></param>
    /// <returns>Trả về số lượng dòng được delete</returns>
    public async Task<int> DeleteDataAsync(string commandText, IEnumerable<SqlParameter>? sqlParameters = null)
    {
        sqlCommand = new SqlCommand();
        sqlCommand.Connection = sqlConnection;
        sqlCommand.CommandTimeout = 500;
        sqlCommand.CommandText = commandText;
        sqlCommand.CommandType = CommandType.Text;
        sqlCommand.Transaction = sqlTransaction;
        if (sqlParameters != null && sqlParameters.Any()) sqlCommand.Parameters.AddRange(sqlParameters.ToArray());
        int rowsAffected = await sqlCommand.ExecuteNonQueryAsync();
        return rowsAffected;
    }

    /// <summary>
    /// trả tra 1 object => muốn trả gì trả
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="sqlParameters"></param>
    /// <returns></returns>
    public async Task<object?> ExecuteScalarObjectAsync(string commandText, IEnumerable<SqlParameter>? sqlParameters = null)
    {
        sqlCommand = new SqlCommand();
        sqlCommand.Connection = sqlConnection;
        sqlCommand.CommandTimeout = 500;
        sqlCommand.CommandText = commandText;
        sqlCommand.CommandType = CommandType.Text;
        sqlCommand.Transaction = sqlTransaction;
        if (sqlParameters != null && sqlParameters.Any()) sqlCommand.Parameters.AddRange(sqlParameters.ToArray());
        return await sqlCommand.ExecuteScalarAsync();
    }
}