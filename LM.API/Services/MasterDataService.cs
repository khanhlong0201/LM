using LM.API.Commons;
using LM.API.Infrastructure;
using LM.Models;
using LM.Models.Shared;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NPOI.POIFS.Crypt.Dsig;
using SixLabors.ImageSharp;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Security.Policy;

namespace LM.API.Services;
public interface IMasterDataService
{
    Task<IEnumerable<UserModel>> GetUsersAsync(int pUserId = -1);
    Task<ResponseModel> UpdateUsers(RequestModel pRequest);
    Task<IEnumerable<UserModel>> Login(LoginRequestModel pRequest);
    Task<ResponseModel> DeleteDataAsync(RequestModel pRequest);
    Task<IEnumerable<KindBookModel>> GetKindBooksAsync();
    Task<ResponseModel> UpdateKindBooks(RequestModel pRequest);
    Task<IEnumerable<PublisherModel>> GetPublishersAsync();
    Task<ResponseModel> UpdatePublishers(RequestModel pRequest);
    Task<IEnumerable<BookModel>> GetBooksAsync(SearchModel pSearchData);
    Task<ResponseModel> UpdateBooks(RequestModel pRequest);
    Task<ResponseModel> UpdateReaders(RequestModel pRequest);
    Task<IEnumerable<ReaderModel>> GetReadersAsync();
    Task<IEnumerable<CliBookModel>> GetBookClientsAsync(SearchModel pSearchData);

    //
    Task<IEnumerable<LocationModel>> GetLocationsAsync();
    Task<ResponseModel> UpdateLocationAsync(RequestModel pRequest);
    Task<IEnumerable<AuthorModel>> GetAuthorsAsync();
    Task<ResponseModel> UpdateAuthorAsync(RequestModel pRequest);
    Task<IEnumerable<StaffModel>> GetStaffsAsync(LoginRequestModel pRequest);
    Task<ResponseModel> UpdateBookSerial(RequestModel pRequest);
    Task<IEnumerable<BookSerialModel>> GetBookSerialsAsync();
}

public class MasterDataService : IMasterDataService
{
    private readonly IBMDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _HOST_API;
    public MasterDataService(IBMDbContext context, IDateTimeService dateTimeService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _dateTimeService = dateTimeService;
        _httpContextAccessor = httpContextAccessor;
        var requestContext = _httpContextAccessor!.HttpContext?.Request;
        string url = string.Empty;
        if (requestContext != null)
        {
            url = @$"{requestContext.Scheme}://{requestContext.Host.Value}";
        }
        _HOST_API = url;
    }

    #region Public Funtions
    /// <summary>
    /// Thêm mới/Cập nhật thông tin người dùng
    /// </summary>
    /// <param name="pRequest"></param>
    /// <returns></returns>
    public async Task<ResponseModel> UpdateUsers(RequestModel pRequest)
    {
        ResponseModel response = new ResponseModel();
        try
        {
            await _context.Connect();
            string queryString = "";
            UserModel oUser = JsonConvert.DeserializeObject<UserModel>(pRequest.Json + "")!;
            SqlParameter[] sqlParameters;
            async Task ExecQuery()
            {
                var data = await _context.AddOrUpdateAsync(queryString, sqlParameters, CommandType.Text);
                if (data != null && data.Rows.Count > 0)
                {
                    response.StatusCode = int.Parse(data.Rows[0]["StatusCode"]?.ToString() ?? "-1");
                    response.Message = data.Rows[0]["ErrorMessage"]?.ToString();
                }
            }
            switch (pRequest.Type)
            {
                case nameof(EnumType.Add):
                    sqlParameters = new SqlParameter[1];
                    sqlParameters[0] = new SqlParameter("@UserName", oUser.UserName);
                    // kiểm tra tên đăng nhập
                    if (await _context.ExecuteScalarAsync("select COUNT(*) from Users with(nolock) where UserName = @UserName", sqlParameters) > 0)
                    {
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        response.Message = "Tên đăng nhập đã tồn tại!";
                        break;
                    }
                    sqlParameters[0] = new SqlParameter("@Type", "Users");
                    queryString = @"INSERT INTO [dbo].[Users]([UserName],[Password],[LastPassword],[FullName],[PhoneNumber] ,[Email] ,[Address],[DateOfBirth],[IsAdmin],[DateCreate] ,[UserCreate],[Isdelete],[Type] )
                                                        values (@UserName , @Password , @LastPassword, @FullName, @PhoneNumber , @Email, @Address, @DateOfBirth, @IsAdmin, @DateTimeNow, @UserId, 0, 'Admin')";

                    string sPassword = EncryptHelper.Encrypt(oUser.Password + "");
                    sqlParameters = new SqlParameter[11];
                    sqlParameters[0] = new SqlParameter("@UserName", oUser.UserName);
                    sqlParameters[1] = new SqlParameter("@Password", sPassword);
                    sqlParameters[2] = new SqlParameter("@LastPassword", sPassword);
                    sqlParameters[3] = new SqlParameter("@FullName", oUser.FullName);
                    sqlParameters[4] = new SqlParameter("@PhoneNumber", oUser.PhoneNumber ?? (object)DBNull.Value);
                    sqlParameters[5] = new SqlParameter("@Email", oUser.Email ?? (object)DBNull.Value);
                    sqlParameters[6] = new SqlParameter("@Address", oUser.Address ?? (object)DBNull.Value);
                    sqlParameters[7] = new SqlParameter("@DateOfBirth", oUser.DateOfBirth ?? (object)DBNull.Value);
                    sqlParameters[8] = new SqlParameter("@IsAdmin", oUser.IsAdmin);
                    sqlParameters[9] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    sqlParameters[10] = new SqlParameter("@UserId", pRequest.UserId);
                    await ExecQuery();
                    break;
                case nameof(EnumType.Update):
                    queryString = @"UPDATE [dbo].[Users]
                                   SET [UserName] = @UserName
                                      ,[FullName] = @FullName
                                      ,[PhoneNumber] = @PhoneNumber
                                      ,[Email] = @Email
                                      ,[Address] = @Address
                                      ,[DateOfBirth] = @DateOfBirth
                                      ,[IsAdmin] = @IsAdmin
                                      ,[DateUpdate] = @DateTimeNow
                                      ,[UserUpdate] = @UserId
                                 WHERE [Id] = @Id";

                    sqlParameters = new SqlParameter[10];
                    sqlParameters[0] = new SqlParameter("@Id", oUser.Id);
                    sqlParameters[1] = new SqlParameter("@FullName", oUser.FullName ?? (object)DBNull.Value);
                    sqlParameters[2] = new SqlParameter("@PhoneNumber", oUser.PhoneNumber ?? (object)DBNull.Value);
                    sqlParameters[3] = new SqlParameter("@Email", oUser.Email ?? (object)DBNull.Value);
                    sqlParameters[4] = new SqlParameter("@Address", oUser.Address ?? (object)DBNull.Value);
                    sqlParameters[5] = new SqlParameter("@DateOfBirth", oUser.DateOfBirth ?? (object)DBNull.Value);
                    sqlParameters[6] = new SqlParameter("@IsAdmin", oUser.IsAdmin);
                    sqlParameters[7] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[8] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    sqlParameters[9] = new SqlParameter("@UserName", oUser.UserName ?? (object)DBNull.Value);
                    await ExecQuery();
                    break;
                case nameof(EnumType.@ChangePassWord):
                    queryString = @"Update [dbo].[Users]
                                    set Password = @PasswordNew, [DateUpdate] = @DateTimeNow, [UserUpdate] = @UserId
                                    where [Id] = @Id";
                    sqlParameters = new SqlParameter[4];
                    sqlParameters[0] = new SqlParameter("@Id", oUser.Id);
                    sqlParameters[1] = new SqlParameter("@PasswordNew", EncryptHelper.Encrypt(oUser.PasswordNew + ""));
                    sqlParameters[2] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    sqlParameters[3] = new SqlParameter("@UserId", pRequest.UserId);
                    await ExecQuery();
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Không xác định được phương thức!";
                    break;
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Message = ex.Message;
        }
        finally
        {
            await _context.DisConnect();
        }
        return response;
    }

    /// <summary>
    /// lấy danh sách nhân viên
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<UserModel>> GetUsersAsync(int pUserid = -1)
    {
        IEnumerable<UserModel> data;
        try
        {
            await _context.Connect();
            SqlParameter[] sqlParameters = new SqlParameter[1];
            sqlParameters[0] = new SqlParameter("@UserId", pUserid);
            data = await _context.GetDataAsync(@$"SELECT [Id]
                                                  ,[UserName]
                                                  ,[Password]
                                                  ,[LastPassword]
                                                  ,[FullName]
                                                  ,[PhoneNumber]
                                                  ,[Email]
                                                  ,[Address]
                                                  ,[DateOfBirth]
                                                  ,[IsAdmin]
                                                  ,[DateCreate]
                                                  ,[UserCreate]
                                                  ,[DateUpdate]
                                                  ,[UserUpdate]
                                              FROM [dbo].[Users] t0 where ISNULL(t0.IsDelete,0) = 0" // không lấy lên tk Support
                    , DataRecordToUserModel, sqlParameters, commandType: CommandType.Text);
        }
        catch (Exception) { throw; }
        finally
        {
            await _context.DisConnect();
        }
        return data;
    }


    /// <summary>
    /// Đăng nhập
    /// </summary>
    /// <param name="pBranchId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<UserModel>> Login(LoginRequestModel pRequest)
    {
        IEnumerable<UserModel> data;
        SqlParameter[] sqlParameters;
        string queryString = "";
        try
        {
            await _context.Connect();
            queryString = @"SELECT [Id]
                              ,[UserName]
                              ,[FullName]
                              ,[IsAdmin]
                              ,[Isdelete] as Isdeleted
                          FROM [dbo].[Users] t0 where ISNULL(t0.IsDelete,0) = 0
                          and t0.UserName = @UserName and t0.Password = @Password";
            //setParameter();
            sqlParameters = new SqlParameter[2];
            sqlParameters[0] = new SqlParameter("@UserName", pRequest.UserName);
            sqlParameters[1] = new SqlParameter("@Password", pRequest.Password);
            data = await _context.GetDataAsync(queryString, DataRecordToUserModelByLogin, sqlParameters, CommandType.Text);
        }
        catch (Exception) { throw; }
        finally
        {
            await _context.DisConnect();
        }
        return data;
    }

    /// <summary>
    /// xóa thông tin trong bảng
    /// </summary>
    /// <param name="pRequest"></param>
    /// <returns></returns>
    public async Task<ResponseModel> DeleteDataAsync(RequestModel pRequest)
    {
        ResponseModel response = new ResponseModel();
        ResponseModel responseCheck = new ResponseModel();
        try
        {
            await _context.Connect();
            SqlParameter[] sqlParameters;
            string queryString = "";
            switch (pRequest.Type)
            {
                case nameof(EnumTable.Users):
                    // kiểm tra điều kiện trước khi xóa
                    //
                    queryString = "[Id] in ( select value from STRING_SPLIT(@ListIds, ',') ) and [IsDelete] = 0";
                    sqlParameters = new SqlParameter[4];
                    sqlParameters[0] = new SqlParameter("@ReasonDelete", pRequest.JsonDetail ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@ListIds", pRequest.Json); // "1,2,3,4"
                    sqlParameters[2] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[3] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());

                    //responseCheck = await CheckKeyBindingBeforeDeleting(pRequest);
                    //if (responseCheck != null && responseCheck.StatusCode == -1)
                    //{
                    //    response.StatusCode = -1;
                    //    response.Message = responseCheck.Message;
                    //    return response;
                    //}
                    response = await deleteDataAsync(nameof(EnumTable.Users), queryString, sqlParameters);
                    break;
                case nameof(EnumTable.KindBooks):
                    queryString = "[KindBookId] in ( select value from STRING_SPLIT(@ListIds, ',') ) and [IsDelete] = 0";
                    sqlParameters = new SqlParameter[4];
                    sqlParameters[0] = new SqlParameter("@ReasonDelete", pRequest.JsonDetail ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@ListIds", pRequest.Json); // "1,2,3,4"
                    sqlParameters[2] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[3] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    response = await deleteDataAsync(nameof(EnumTable.KindBooks), queryString, sqlParameters);
                    break;
                case nameof(EnumTable.Publishers):
                    queryString = "[PublisherId] in ( select value from STRING_SPLIT(@ListIds, ',') ) and [IsDelete] = 0";
                    sqlParameters = new SqlParameter[4];
                    sqlParameters[0] = new SqlParameter("@ReasonDelete", pRequest.JsonDetail ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@ListIds", pRequest.Json); // "1,2,3,4"
                    sqlParameters[2] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[3] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    response = await deleteDataAsync(nameof(EnumTable.Publishers), queryString, sqlParameters);
                    break;
                case nameof(EnumTable.Books):
                    queryString = "[BookId] in ( select value from STRING_SPLIT(@ListIds, ',') ) and [IsDelete] = 0";
                    sqlParameters = new SqlParameter[4];
                    sqlParameters[0] = new SqlParameter("@ReasonDelete", pRequest.JsonDetail ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@ListIds", pRequest.Json); // "1,2,3,4"
                    sqlParameters[2] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[3] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    response = await deleteDataAsync(nameof(EnumTable.Books), queryString, sqlParameters);
                    break;
                case nameof(EnumTable.Readers):
                    queryString = "[ReaderId] in ( select value from STRING_SPLIT(@ListIds, ',') ) and [IsDelete] = 0";
                    sqlParameters = new SqlParameter[4];
                    sqlParameters[0] = new SqlParameter("@ReasonDelete", pRequest.JsonDetail ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@ListIds", pRequest.Json); // "1,2,3,4"
                    sqlParameters[2] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[3] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    response = await deleteDataAsync(nameof(EnumTable.Readers), queryString, sqlParameters);
                    break;
                case nameof(EnumTable.Batchs):
                    queryString = "[BatchId] in ( select value from STRING_SPLIT(@ListIds, ',') ) and [IsDelete] = 0";
                    sqlParameters = new SqlParameter[4];
                    sqlParameters[0] = new SqlParameter("@ReasonDelete", pRequest.JsonDetail ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@ListIds", pRequest.Json); // "1,2,3,4"
                    sqlParameters[2] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[3] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    response = await deleteDataAsync(nameof(EnumTable.Batchs), queryString, sqlParameters);
                    break;
                case nameof(EnumTable.Series):
                    queryString = "[SeriesId] in ( select value from STRING_SPLIT(@ListIds, ',') ) and [IsDelete] = 0";
                    sqlParameters = new SqlParameter[4];
                    sqlParameters[0] = new SqlParameter("@ReasonDelete", pRequest.JsonDetail ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@ListIds", pRequest.Json); // "1,2,3,4"
                    sqlParameters[2] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[3] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    response = await deleteDataAsync(nameof(EnumTable.Series), queryString, sqlParameters);
                    break;
                case nameof(EnumTable.BookSerials):
                    queryString = "[SeriesId] in ( select value from STRING_SPLIT(@ListIds, ',') ) and [IsDelete] = 0";
                    sqlParameters = new SqlParameter[4];
                    sqlParameters[0] = new SqlParameter("@ReasonDelete", pRequest.JsonDetail ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@ListIds", pRequest.Json); // "1,2,3,4"
                    sqlParameters[2] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[3] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());

                    responseCheck = await CheckKeyBindingBeforeDeleting(pRequest);
                    if (responseCheck != null && responseCheck.StatusCode == -1)
                    {
                        response.StatusCode = -1;
                        response.Message = responseCheck.Message;
                        return response;
                    }
                    response = await deleteDataAsync(nameof(EnumTable.BookSerials), queryString, sqlParameters);
                    break;
                case nameof(EnumTable.Authors):
                    queryString = "[SeriesId] in ( select value from STRING_SPLIT(@ListIds, ',') ) and [IsDelete] = 0";
                    sqlParameters = new SqlParameter[4];
                    sqlParameters[0] = new SqlParameter("@ReasonDelete", pRequest.JsonDetail ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@ListIds", pRequest.Json); // "1,2,3,4"
                    sqlParameters[2] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[3] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());

                    responseCheck = await CheckKeyBindingBeforeDeleting(pRequest);
                    if (responseCheck != null && responseCheck.StatusCode == -1)
                    {
                        response.StatusCode = -1;
                        response.Message = responseCheck.Message;
                        return response;
                    }
                    response = await deleteDataAsync(nameof(EnumTable.Authors), queryString, sqlParameters);
                    break;
                case nameof(EnumTable.@Locations):
                    queryString = "[SeriesId] in ( select value from STRING_SPLIT(@ListIds, ',') ) and [IsDelete] = 0";
                    sqlParameters = new SqlParameter[4];
                    sqlParameters[0] = new SqlParameter("@ReasonDelete", pRequest.JsonDetail ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@ListIds", pRequest.Json); // "1,2,3,4"
                    sqlParameters[2] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[3] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());

                    responseCheck = await CheckKeyBindingBeforeDeleting(pRequest);
                    if (responseCheck != null && responseCheck.StatusCode == -1)
                    {
                        response.StatusCode = -1;
                        response.Message = responseCheck.Message;
                        return response;
                    }
                    response = await deleteDataAsync(nameof(EnumTable.Authors), queryString, sqlParameters);
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Không xác định được phương thức!";
                    break;
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Message = ex.Message;
            await _context.RollbackAsync();
        }
        finally
        {
            await _context.DisConnect();
        }
        return response;
    }


    /// <summary>
    /// lấy danh sách loại sách
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<KindBookModel>> GetKindBooksAsync()
    {
        IEnumerable<KindBookModel> data;
        try
        {
            await _context.Connect();
            data = await _context.GetDataAsync(@$"SELECT t0.[KindBookId]
                                                  ,t0.[KindBookName]
                                                  ,t0.[Description]
                                                  ,t0.[DateCreate]
                                                  ,t0.[UserCreate]
                                                  ,t0.[DateUpdate]
                                                  ,t0.[UserUpdate]
                                                  ,t0.[LocationId]
                                                  ,iif(T1.[LocationName] is null, N'Chưa cập nhật', T1.[LocationName] ) as LocationName
                                              FROM [dbo].[KindBooks] t0 
                                           left join [Locations] as t1 on T0.LocationId = t1.Id
                                           where ISNULL(t0.IsDelete,0) = 0" // không lấy lên tk Support
                    , DataRecordToKindBookModel, commandType: CommandType.Text);
        }
        catch (Exception) { throw; }
        finally
        {
            await _context.DisConnect();
        }
        return data;
    }
    /// <summary>
    /// Thêm mới/Cập nhật thông tin loại sách
    /// </summary>
    /// <param name="pRequest"></param>
    /// <returns></returns>
    public async Task<ResponseModel> UpdateKindBooks(RequestModel pRequest)
    {
        ResponseModel response = new ResponseModel();
        try
        {
            await _context.Connect();
            string queryString = "";
            KindBookModel oKindBook = JsonConvert.DeserializeObject<KindBookModel>(pRequest.Json + "")!;
            SqlParameter[] sqlParameters;
            async Task ExecQuery()
            {
                var data = await _context.AddOrUpdateAsync(queryString, sqlParameters, CommandType.Text);
                if (data != null && data.Rows.Count > 0)
                {
                    response.StatusCode = int.Parse(data.Rows[0]["StatusCode"]?.ToString() ?? "-1");
                    response.Message = data.Rows[0]["ErrorMessage"]?.ToString();
                }
            }
            switch (pRequest.Type)
            {
                case nameof(EnumType.Add):
                    sqlParameters = new SqlParameter[1];
                    sqlParameters[0] = new SqlParameter("@KindBookName", oKindBook.KindBookName);
                    // kiểm tra tên đăng nhập
                    if (await _context.ExecuteScalarAsync("select COUNT(*) from KindBooks with(nolock) where KindBookName = @KindBookName", sqlParameters) > 0)
                    {
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        response.Message = "Tên đăng nhập đã tồn tại!";
                        break;
                    }
                    queryString = @"INSERT INTO [dbo].[KindBooks] ([KindBookName],[Description],[DateCreate],[UserCreate],[IsDelete], [LocationId])
                                                        values (@KindBookName , @Description , @DateTimeNow, @UserId, 0, @LocationId)";

                    sqlParameters = new SqlParameter[5];
                    sqlParameters[0] = new SqlParameter("@KindBookName", oKindBook.KindBookName ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@Description", oKindBook.Description ?? (object)DBNull.Value);
                    sqlParameters[2] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    sqlParameters[3] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[4] = new SqlParameter("@LocationId", oKindBook.LocationId);
                    await ExecQuery();
                    break;
                case nameof(EnumType.Update):
                    queryString = @"UPDATE [dbo].[KindBooks]
                               SET [KindBookName] =@KindBookName
                                  ,[Description] = @Description
                                  ,[DateUpdate] = @DateTimeNow
                                  ,[UserUpdate] = @UserId
                                  ,[LocationId] = @LocationId
                                 WHERE [KindBookId] = @KindBookId";

                    sqlParameters = new SqlParameter[6];
                    sqlParameters[0] = new SqlParameter("@KindBookId", oKindBook.KindBookId);
                    sqlParameters[1] = new SqlParameter("@KindBookName", oKindBook.KindBookName ?? (object)DBNull.Value);
                    sqlParameters[2] = new SqlParameter("@Description", oKindBook.Description ?? (object)DBNull.Value);
                    sqlParameters[3] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[4] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    sqlParameters[5] = new SqlParameter("@LocationId", oKindBook.LocationId);
                    await ExecQuery();
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Không xác định được phương thức!";
                    break;
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Message = ex.Message;
        }
        finally
        {
            await _context.DisConnect();
        }
        return response;
    }

    /// <summary>
    /// lấy danh sách nhà xuất bản
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<PublisherModel>> GetPublishersAsync()
    {
        IEnumerable<PublisherModel> data;
        try
        {
            await _context.Connect();
            data = await _context.GetDataAsync(@$"SELECT [PublisherId]
                                                  ,[PublisherName]
                                                  ,[Description]
                                                  ,[DateCreate]
                                                  ,[UserCreate]
                                                  ,[DateUpdate]
                                                  ,[UserUpdate]
                                              FROM [dbo].[Publishers] t0 where ISNULL(t0.IsDelete,0) = 0" 
                    , DataRecordToPublisherModel, commandType: CommandType.Text);
        }
        catch (Exception) { throw; }
        finally
        {
            await _context.DisConnect();
        }
        return data;
    }
    /// <summary>
    /// Thêm mới/Cập nhật thông tin nhà xuất bản
    /// </summary>
    /// <param name="pRequest"></param>
    /// <returns></returns>
    public async Task<ResponseModel> UpdatePublishers(RequestModel pRequest)
    {
        ResponseModel response = new ResponseModel();
        try
        {
            await _context.Connect();
            string queryString = "";
            PublisherModel oPublisher = JsonConvert.DeserializeObject<PublisherModel>(pRequest.Json + "")!;
            SqlParameter[] sqlParameters;
            async Task ExecQuery()
            {
                var data = await _context.AddOrUpdateAsync(queryString, sqlParameters, CommandType.Text);
                if (data != null && data.Rows.Count > 0)
                {
                    response.StatusCode = int.Parse(data.Rows[0]["StatusCode"]?.ToString() ?? "-1");
                    response.Message = data.Rows[0]["ErrorMessage"]?.ToString();
                }
            }
            switch (pRequest.Type)
            {
                case nameof(EnumType.Add):
                    sqlParameters = new SqlParameter[1];
                    sqlParameters[0] = new SqlParameter("@PublisherName", oPublisher.PublisherName);
                    // kiểm tra tên đăng nhập
                    if (await _context.ExecuteScalarAsync("select COUNT(*) from Publishers with(nolock) where PublisherName = @PublisherName", sqlParameters) > 0)
                    {
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        response.Message = "Tên đăng nhập đã tồn tại!";
                        break;
                    }
                    queryString = @"INSERT INTO [dbo].[Publishers] ([PublisherName],[Description],[DateCreate],[UserCreate],[IsDelete])
                                                        values (@PublisherName , @Description , @DateTimeNow, @UserId, 0)";

                    sqlParameters = new SqlParameter[4];
                    sqlParameters[0] = new SqlParameter("@PublisherName", oPublisher.PublisherName ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@Description", oPublisher.Description ?? (object)DBNull.Value);
                    sqlParameters[2] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    sqlParameters[3] = new SqlParameter("@UserId", pRequest.UserId);
                    await ExecQuery();
                    break;
                case nameof(EnumType.Update):
                    queryString = @"UPDATE [dbo].[Publishers]
                               SET [PublisherName] =@PublisherName
                                  ,[Description] = @Description
                                  ,[DateUpdate] = @DateTimeNow
                                  ,[UserUpdate] = @UserId
                                 WHERE [PublisherId] = @PublisherId";

                    sqlParameters = new SqlParameter[5];
                    sqlParameters[0] = new SqlParameter("@PublisherId", oPublisher.PublisherId);
                    sqlParameters[1] = new SqlParameter("@PublisherName", oPublisher.PublisherName ?? (object)DBNull.Value);
                    sqlParameters[2] = new SqlParameter("@Description", oPublisher.Description ?? (object)DBNull.Value);
                    sqlParameters[3] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[4] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    await ExecQuery();
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Không xác định được phương thức!";
                    break;
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Message = ex.Message;
        }
        finally
        {
            await _context.DisConnect();
        }
        return response;
    }

    /// <summary>
    /// lấy danh sách sách
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<BookModel>> GetBooksAsync(SearchModel pSearchData)
    {
        IEnumerable<BookModel> data;
        try
        {
            await _context.Connect();
            SqlParameter[] sqlParameters;
            sqlParameters = new SqlParameter[2];
            sqlParameters[0] = new SqlParameter("@KindBookId", pSearchData.KindBookId);
            sqlParameters[1] = new SqlParameter("@PublisherId", pSearchData.PublisherId);
            data = await _context.GetDataAsync(@$"select  t0.[BookId]
                        ,t0.[BookName]
                        ,t0.[Description]
	                    ,isnull((Select count(*) from BookSerials t00 where t00.IsActive = 1 and t0.BookId = t00.BookId),0) as 'Qty'
                        ,t0.[Language]
                        ,t0.[Size]
                        ,t0.[NumOfPage]
                        ,t0.[DateCreate]
                        ,t0.[UserCreate]
                        ,t0.[KindBookId]
                        ,t0.[PublisherId]
                        ,t0.[ImageUrl]
	                    ,t1.PublisherName
	                    ,t2.KindBookName
                        ,t0.PublisherId
	                    ,t0.KindBookId
                        ,t0.PublishingYear
						, concat(t0.BookName, N' - NXB: ',t0.PublishingYear) as 'Name'
                        ,t0.AuthorId, T3.AuthorName
                    FROM [dbo].[Books] t0 
                    inner join  Publishers t1 on t0.PublisherId = t1.PublisherId
                    inner join  KindBooks t2 on t0.KindBookid = t2.KindBookid
                    left join  Authors t3 on t0.AuthorId = t3.Id
                    where t0.isdelete = 0 
                      and (isnull(@KindBookId,0)=0 or t0.KindBookId = @KindBookId)
                    and (isnull(@PublisherId,0)=0 or t0.PublisherId = @PublisherId)"
                    , DataRecordToBookModel,sqlParameters, commandType: CommandType.Text);
        }
        catch (Exception) { throw; }
        finally
        {
            await _context.DisConnect();
        }
        return data;
    }

    /// <summary>
    /// Thêm mới/Cập nhật thông tin sách
    /// </summary>
    /// <param name="pRequest"></param>
    /// <returns></returns>
    public async Task<ResponseModel> UpdateBooks(RequestModel pRequest)
    {
        ResponseModel response = new ResponseModel();
        try
        {
            await _context.Connect();
            string queryString = "";
            bool isUpdated = false;
            BookModel oBook = JsonConvert.DeserializeObject<BookModel>(pRequest.Json + "")!;
            SqlParameter[] sqlParameters;
            async Task<bool> ExecQuery()
            {
                var data = await _context.AddOrUpdateAsync(queryString, sqlParameters, CommandType.Text);
                if (data != null && data.Rows.Count > 0)
                {
                    response.StatusCode = int.Parse(data.Rows[0]["StatusCode"]?.ToString() ?? "-1");
                    response.Message = data.Rows[0]["ErrorMessage"]?.ToString();
                    return response.StatusCode == 0;
                }
                return false;
            }
            switch (pRequest.Type)
            {
                case nameof(EnumType.Add):
                    sqlParameters = new SqlParameter[1];
                    sqlParameters[0] = new SqlParameter("@BookName", oBook.BookName);
                    // kiểm tra tên đăng nhập
                    //if (await _context.ExecuteScalarAsync("select COUNT(*) from Books with(nolock) where BookName = @BookName", sqlParameters) > 0)
                    //{
                    //    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    //    response.Message = "Tên đăng nhập đã tồn tại!";
                    //    break;
                    //}
                    queryString = @"INSERT INTO [dbo].[Books] ([BookName], [Description], [PublishingYear], [Language], [Size], [NumOfPage], [DateCreate], [UserCreate], [IsDelete]
                                                                , [KindBookId], [PublisherId], [ImageUrl], [AuthorId])
                                                            values (@BookName , @Description , @PublishingYear, @Language, @Size, @NumOfPage,  @DateTimeNow, @UserId, 0, @KindBookId, @PublisherId, @ImageUrl, @AuthorId )";
                    sqlParameters = new SqlParameter[12];
                    sqlParameters[0] = new SqlParameter("@BookName", oBook.BookName ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@Description", oBook.Description ?? (object)DBNull.Value);
                    sqlParameters[2] = new SqlParameter("@PublishingYear", oBook.PublishingYear);
                    sqlParameters[3] = new SqlParameter("@Language", oBook.Language ?? (object)DBNull.Value);
                    sqlParameters[4] = new SqlParameter("@Size", oBook.Size ?? (object)DBNull.Value);
                    sqlParameters[5] = new SqlParameter("@NumOfPage", oBook.NumOfPage);
                    sqlParameters[6] = new SqlParameter("@KindBookId", oBook.KindBookId);
                    sqlParameters[7] = new SqlParameter("@PublisherId", oBook.PublisherId);
                    sqlParameters[8] = new SqlParameter("@ImageUrl", oBook.ImageUrl ?? (object)DBNull.Value);
                    sqlParameters[9] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    sqlParameters[10] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[11] = new SqlParameter("@AuthorId", oBook.AuthorId);
                    isUpdated = await ExecQuery();
                    break;
                case nameof(EnumType.Update):
                    queryString = @"UPDATE [dbo].[Books]
                                       SET [BookName] = @BookName
                                           ,[Description] = @Description 
                                           ,[PublishingYear] = @PublishingYear
                                           ,[Language] = @Language
                                           ,[Size] = @Size
                                           ,[NumOfPage] = @NumOfPage
                                           ,[KindBookId] = @KindBookId
                                           ,[PublisherId] = @PublisherId
                                           ,[ImageUrl] = @ImageUrl
                                           ,[DateUpdate] = @DateTimeNow
                                           ,[UserUpdate] = @UserId
                                           ,[AuthorId] = @AuthorId
                                     WHERE [BookId] = @BookId";
                    sqlParameters = new SqlParameter[13];
                    sqlParameters[0] = new SqlParameter("@BookName", oBook.BookName);
                    sqlParameters[1] = new SqlParameter("@Description", oBook.Description ?? (object)DBNull.Value);
                    sqlParameters[2] = new SqlParameter("@PublishingYear", oBook.PublishingYear);
                    sqlParameters[3] = new SqlParameter("@Language", oBook.Language ?? (object)DBNull.Value);
                    sqlParameters[4] = new SqlParameter("@Size", oBook.Size ?? (object)DBNull.Value);
                    sqlParameters[5] = new SqlParameter("@NumOfPage", oBook.NumOfPage);
                    sqlParameters[6] = new SqlParameter("@KindBookId", oBook.KindBookId);
                    sqlParameters[7] = new SqlParameter("@PublisherId", oBook.PublisherId);
                    sqlParameters[8] = new SqlParameter("@ImageUrl", oBook.ImageUrl ?? (object)DBNull.Value);
                    sqlParameters[9] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    sqlParameters[10] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[11] = new SqlParameter("@BookId", oBook.BookId);
                    sqlParameters[12] = new SqlParameter("@AuthorId", oBook.AuthorId);
                    isUpdated = await ExecQuery();
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Không xác định được phương thức!";
                    break;
            }    
        }
        catch (Exception ex)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Message = ex.Message;
            await _context.RollbackAsync();
        }
        finally
        {
            await _context.DisConnect();
        }

        return response;
    }

    /// <summary>
    /// Thêm mới/Cập nhật thông tin đọc giả
    /// </summary>
    /// <param name="pRequest"></param>
    /// <returns></returns>
    public async Task<ResponseModel> UpdateReaders(RequestModel pRequest)
    {
        ResponseModel response = new ResponseModel();
        try
        {
            await _context.Connect();
            string queryString = "";
            bool isUpdated = false;
            ReaderModel oReader = JsonConvert.DeserializeObject<ReaderModel>(pRequest.Json + "")!;
            SqlParameter[] sqlParameters;
            async Task<bool> ExecQuery()
            {
                var data = await _context.AddOrUpdateAsync(queryString, sqlParameters, CommandType.Text);
                if (data != null && data.Rows.Count > 0)
                {
                    response.StatusCode = int.Parse(data.Rows[0]["StatusCode"]?.ToString() ?? "-1");
                    response.Message = data.Rows[0]["ErrorMessage"]?.ToString();
                    return response.StatusCode == 0;
                }
                return false;
            }
            switch (pRequest.Type)
            {
                case nameof(EnumType.Add):
                    sqlParameters = new SqlParameter[1];
                    sqlParameters[0] = new SqlParameter("@UserName", oReader.UserName);
                    // kiểm tra tên đăng nhập
                    if (await _context.ExecuteScalarAsync("select COUNT(*) from Users with(nolock) where UserName = @UserName", sqlParameters) > 0)
                    {
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        response.Message = "Tên đăng nhập đã tồn tại!";
                        break;
                    }
                    queryString = @"INSERT INTO [dbo].[Users]([UserName],[Password],[LastPassword],[FullName],[PhoneNumber] ,[Email] ,[Address],[DateOfBirth],[IsAdmin],[DateCreate] ,[UserCreate],[Isdelete],[Type] )
                                                        values (@UserName , @Password , @LastPassword, @FullName, @PhoneNumber , @Email, @Address, @DateOfBirth, @IsAdmin, @DateTimeNow, @UserId, 0, 'Client')";

                    string sPassword = EncryptHelper.Encrypt(oReader.Password + "");
                    sqlParameters = new SqlParameter[11];
                    sqlParameters[0] = new SqlParameter("@UserName", oReader.UserName);
                    sqlParameters[1] = new SqlParameter("@Password", sPassword);
                    sqlParameters[2] = new SqlParameter("@LastPassword", sPassword);
                    sqlParameters[3] = new SqlParameter("@FullName", oReader.FullName);
                    sqlParameters[4] = new SqlParameter("@PhoneNumber", oReader.PhoneNumber ?? (object)DBNull.Value);
                    sqlParameters[5] = new SqlParameter("@Email", oReader.Email ?? (object)DBNull.Value);
                    sqlParameters[6] = new SqlParameter("@Address", oReader.Address ?? (object)DBNull.Value);
                    sqlParameters[7] = new SqlParameter("@DateOfBirth", oReader.DateOfBirth ?? (object)DBNull.Value);
                    sqlParameters[8] = new SqlParameter("@IsAdmin", oReader.IsAdmin);
                    sqlParameters[9] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    sqlParameters[10] = new SqlParameter("@UserId", pRequest.UserId);
                    await _context.BeginTranAsync();
                    isUpdated = await ExecQuery();
                    if (isUpdated)
                    {
                        int iUserId = await _context.ExecuteScalarAsync("select isnull(max(Id), 0) from [dbo].[Users] with(nolock)");
                        queryString = @"INSERT INTO[dbo].[Readers] ([FullName],[DateOfBrirth],[Gender] ,[Level],[Job],[WorkPlace],[IdentityCard],[DateCreate],[UserCreate],[IsDelete],[UserId])
                                                                values (@FullName, @DateOfBrirth, @Gender ,@Level, @Job ,@WorkPlace, @IdentityCard, @DateTimeNow, @UserId, 0, @IUserId)";

                        sqlParameters = new SqlParameter[10];
                        sqlParameters[0] = new SqlParameter("@FullName", oReader.FullName);
                        sqlParameters[1] = new SqlParameter("@DateOfBrirth", oReader.DateOfBirth ?? (object)DBNull.Value);
                        sqlParameters[2] = new SqlParameter("@Gender", oReader.Gender ?? (object)DBNull.Value);
                        sqlParameters[3] = new SqlParameter("@Level", oReader.Level ?? (object)DBNull.Value);
                        sqlParameters[4] = new SqlParameter("@Job", oReader.Job ?? (object)DBNull.Value);
                        sqlParameters[5] = new SqlParameter("@WorkPlace", oReader.WorkPlace ?? (object)DBNull.Value);
                        sqlParameters[6] = new SqlParameter("@IdentityCard", oReader.IdentityCard ?? (object)DBNull.Value);
                        sqlParameters[7] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                        sqlParameters[8] = new SqlParameter("@UserId", pRequest.UserId);
                        sqlParameters[9] = new SqlParameter("@IUserId", iUserId);
                        isUpdated = await ExecQuery();
                        if (!isUpdated)
                        {
                            await _context.RollbackAsync();
                            return response;
                        }
                        await _context.CommitTranAsync();
                    }
                    break;
                case nameof(EnumType.Update):
                    queryString = @"UPDATE [dbo].[Readers]
                                   SET [FullName] = @FullName
                                      ,[DateOfBrirth] = @DateOfBrirth
                                      ,[Gender] = @Gender
                                      ,[Level] = @Level
                                      ,[Job] = @Job
                                      ,[WorkPlace] = @WorkPlace
                                      ,[IdentityCard] = @IdentityCard
                                      ,[DateUpdate] = @DateTimeNow
                                      ,[UserUpdate] = @UserId
                                 WHERE [ReaderId] = @ReaderId";
                    sqlParameters = new SqlParameter[10];
                    sqlParameters[0] = new SqlParameter("@FullName", oReader.FullName);
                    sqlParameters[1] = new SqlParameter("@DateOfBrirth", oReader.DateOfBirth ?? (object)DBNull.Value);
                    sqlParameters[2] = new SqlParameter("@Gender", oReader.Gender ?? (object)DBNull.Value);
                    sqlParameters[3] = new SqlParameter("@Level", oReader.Level ?? (object)DBNull.Value);
                    sqlParameters[4] = new SqlParameter("@Job", oReader.Job ?? (object)DBNull.Value);
                    sqlParameters[5] = new SqlParameter("@WorkPlace", oReader.WorkPlace ?? (object)DBNull.Value);
                    sqlParameters[6] = new SqlParameter("@IdentityCard", oReader.IdentityCard ?? (object)DBNull.Value);
                    sqlParameters[7] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    sqlParameters[8] = new SqlParameter("@UserId", pRequest.UserId);
                    sqlParameters[9] = new SqlParameter("@ReaderId", oReader.ReaderId);
                    await ExecQuery();
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Không xác định được phương thức!";
                    break;
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Message = ex.Message;
        }
        finally
        {
            await _context.DisConnect();
        }
        return response;
    }

    /// <summary>
    /// lấy danh sách đôc giả
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<ReaderModel>> GetReadersAsync()
    {
        IEnumerable<ReaderModel> data;
        try
        {
            await _context.Connect();
            data = await _context.GetDataAsync(@$"SELECT [ReaderId]
                                                  ,[FullName]
                                                  ,[DateOfBrirth]
                                                  ,[Gender]
                                                  ,[Level]
                                                  ,[Job]
                                                  ,[WorkPlace]
                                                  ,[IdentityCard]
                                                  ,[DateCreate]
                                                  ,[UserCreate]
                                                  ,[DateUpdate]
                                                  ,[UserId]
                                              FROM [dbo].[Readers] 
                                              where IsDelete = 0"
                    , DataRecordToReaderModel, commandType: CommandType.Text);
        }
        catch (Exception) { throw; }
        finally
        {
            await _context.DisConnect();
        }
        return data;
    }

    /// <summary>
    /// lấy danh sách ở client
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<CliBookModel>> GetBookClientsAsync(SearchModel pSearchData)
    {
        IEnumerable<CliBookModel> data;
        try
        {
            await _context.Connect();
            SqlParameter[] sqlParameters;
            sqlParameters = new SqlParameter[3];
            sqlParameters[0] = new SqlParameter("@KindBookId", pSearchData.KindBookId);
            sqlParameters[1] = new SqlParameter("@PublisherId", pSearchData.PublisherId);
            sqlParameters[2] = new SqlParameter("@PublishingYear", pSearchData.PublishingYear ?? (object)DBNull.Value);
            data = await _context.GetDataAsync(@$"select  t0.[BookId]
                        ,t0.[BookName]
                        ,t0.[Description]
	                    , isnull((Select top 1 t00.Price from Batchs t00 where t00.IsDelete = 0 order by t00.DateCreate desc),0) as 'Price'
	                    ,  isnull((Select SUM(t00.Qty) from Batchs t00 where t00.IsDelete = 0 and t0.BookId = t00.BookId group by t00.BatchId),0) as 'Qty'
                        ,t0.[Language]
                        ,t0.[Size]
                        ,t0.[NumOfPage]
                        ,t0.[DateCreate]
                        ,t0.[UserCreate]
                        ,t0.[KindBookId]
                        ,t0.[PublisherId]
                        ,t0.[ImageId]
	                    ,t1.PublisherName
	                    ,t2.KindBookName
                        ,t0.PublisherId
	                    ,t0.KindBookId
                        ,t0.PublishingYear
						, concat(t0.BookName, N' - NXB: ',t0.PublishingYear) as 'Name'
						,t4.[ImageDetailId]
						,t4.[FilePath]
                        ,t0.ImageId
                    FROM [dbo].[Books] t0 
                    inner join  Publishers t1 on t0.PublisherId = t1.PublisherId
                    inner join  KindBooks t2 on t0.KindBookid = t2.KindBookid
                    left join  Images t3 on t0.ImageId = t3.ImageId
					left join  ImageDetails t4 on t3.ImageId = t4.ImageId
                    where t0.isdelete = 0
                    and (isnull(@KindBookId,0)=0 or t0.KindBookId = @KindBookId)
                    and (isnull(@PublisherId,0)=0 or t0.PublisherId = @PublisherId)
					and (isnull(@PublishingYear,0)=0 or t0.PublishingYear = @PublishingYear)"
                    , DataRecordToBookClientModel, sqlParameters, commandType: CommandType.Text);
        }
        catch (Exception) { throw; }
        finally
        {
            await _context.DisConnect();
        }
        return data;
    }
    
    /// <summary>
    /// lấy danh sách vị trí
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<LocationModel>> GetLocationsAsync()
    {
        IEnumerable<LocationModel> data;
        try
        {
            await _context.Connect();
            string querry = @$"Select *
                                     from [dbo].[Locations] as T0 with(nolock)
                                    where T0.[IsDelete] = 0 order by T0.Id desc";
            Func<IDataRecord, LocationModel> readData = record =>
            {
                LocationModel model = new LocationModel();
                if (!Convert.IsDBNull(record["Id"])) model.Id = Convert.ToInt32(record["Id"]);
                if (!Convert.IsDBNull(record["LocationName"])) model.LocationName = Convert.ToString(record["LocationName"]);
                if (!Convert.IsDBNull(record["Description"])) model.Description = Convert.ToString(record["Description"]);
                if (!Convert.IsDBNull(record["DateCreate"])) model.DateCreate = Convert.ToDateTime(record["DateCreate"]);
                if (!Convert.IsDBNull(record["UserCreate"])) model.UserCreate = Convert.ToInt32(record["UserCreate"]);
                return model;
            };
            data = await _context.GetDataAsync(querry, readData, commandType: CommandType.Text);
        }
        catch (Exception) { throw; }
        finally
        {
            await _context.DisConnect();
        }
        return data;
    }

    /// <summary>
    /// thêm mới/ cập nhật thông tin vị trí sách
    /// </summary>
    /// <param name="pRequest"></param>
    /// <returns></returns>
    public async Task<ResponseModel> UpdateLocationAsync(RequestModel pRequest)
    {
        ResponseModel response = new ResponseModel();
        try
        {
            await _context.Connect();
            string queryString = "";
            LocationModel oItem = JsonConvert.DeserializeObject<LocationModel>(pRequest.Json + "")!;
            var lstFiles = JsonConvert.DeserializeObject<List<ImageModel>>(pRequest.JsonDetail + "");
            SqlParameter[] sqlParameters;
            sqlParameters = new SqlParameter[5];
            sqlParameters[0] = new SqlParameter("@LocationName", oItem.LocationName);
            sqlParameters[1] = new SqlParameter("@Description", oItem.Description ?? (object)DBNull.Value);
            sqlParameters[2] = new SqlParameter("@DateCreate", _dateTimeService.GetCurrentVietnamTime());
            sqlParameters[3] = new SqlParameter("@UserCreate", pRequest.UserId);
            sqlParameters[4] = new SqlParameter("@Id", oItem.Id);
            if(pRequest.Type == nameof(EnumType.Add))
            {
                queryString = @"Insert Into [dbo].[Locations]([LocationName],[Description],[DateCreate],[UserCreate])
                                        Values (@LocationName, @Description, @DateCreate, @UserCreate)";
            }  
            else
            {
                queryString = @"Update [dbo].[Locations]
                                           set [LocationName] = @LocationName, [Description] = @Description, [DateUpdate] = @DateCreate, [UserUpdate] = @UserCreate
                                         where [Id] = @Id";
            }

            var data = await _context.AddOrUpdateAsync(queryString, sqlParameters, CommandType.Text);
            if (data != null && data.Rows.Count > 0)
            {
                response.StatusCode = int.Parse(data.Rows[0]["StatusCode"]?.ToString() ?? "-1");
                response.Message = data.Rows[0]["ErrorMessage"]?.ToString();
            }

        }
        catch (Exception ex)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Message = ex.Message;
        }
        finally
        {
            await _context.DisConnect();
        }
        return response;
    }

    /// <summary>
    /// lấy danh sách tác gả
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<AuthorModel>> GetAuthorsAsync()
    {
        IEnumerable<AuthorModel> data;
        try
        {
            await _context.Connect();
            string querry = @$"Select *
                                     from [dbo].[Authors] as T0 with(nolock)
                                    where T0.[IsDelete] = 0 order by T0.Id desc";
            Func<IDataRecord, AuthorModel> readData = record =>
            {
                AuthorModel model = new AuthorModel();
                if (!Convert.IsDBNull(record["Id"])) model.Id = Convert.ToInt32(record["Id"]);
                if (!Convert.IsDBNull(record["AuthorName"])) model.AuthorName = Convert.ToString(record["AuthorName"]);
                if (!Convert.IsDBNull(record["Description"])) model.Description = Convert.ToString(record["Description"]);
                if (!Convert.IsDBNull(record["DateCreate"])) model.DateCreate = Convert.ToDateTime(record["DateCreate"]);
                if (!Convert.IsDBNull(record["UserCreate"])) model.UserCreate = Convert.ToInt32(record["UserCreate"]);
                return model;
            };
            data = await _context.GetDataAsync(querry, readData, commandType: CommandType.Text);
        }
        catch (Exception) { throw; }
        finally
        {
            await _context.DisConnect();
        }
        return data;
    }

    /// <summary>
    /// thêm mới/ cập nhật tác gả
    /// </summary>
    /// <param name="pRequest"></param>
    /// <returns></returns>
    public async Task<ResponseModel> UpdateAuthorAsync(RequestModel pRequest)
    {
        ResponseModel response = new ResponseModel();
        try
        {
            await _context.Connect();
            string queryString = "";
            AuthorModel oItem = JsonConvert.DeserializeObject<AuthorModel>(pRequest.Json + "")!;
            var lstFiles = JsonConvert.DeserializeObject<List<ImageModel>>(pRequest.JsonDetail + "");
            SqlParameter[] sqlParameters;
            sqlParameters = new SqlParameter[5];
            sqlParameters[0] = new SqlParameter("@AuthorName", oItem.AuthorName);
            sqlParameters[1] = new SqlParameter("@Description", oItem.Description ?? (object)DBNull.Value);
            sqlParameters[2] = new SqlParameter("@DateCreate", _dateTimeService.GetCurrentVietnamTime());
            sqlParameters[3] = new SqlParameter("@UserCreate", pRequest.UserId);
            sqlParameters[4] = new SqlParameter("@Id", oItem.Id);
            if (pRequest.Type == nameof(EnumType.Add))
            {
                queryString = @"Insert Into [dbo].[Authors]([AuthorName],[Description],[DateCreate],[UserCreate])
                                        Values (@AuthorName, @Description, @DateCreate, @UserCreate)";
            }
            else
            {
                queryString = @"Update [dbo].[Authors]
                                           set [AuthorName] = @AuthorName, [Description] = @Description, [DateUpdate] = @DateCreate, [UserUpdate] = @UserCreate
                                         where [Id] = @Id";
            }

            var data = await _context.AddOrUpdateAsync(queryString, sqlParameters, CommandType.Text);
            if (data != null && data.Rows.Count > 0)
            {
                response.StatusCode = int.Parse(data.Rows[0]["StatusCode"]?.ToString() ?? "-1");
                response.Message = data.Rows[0]["ErrorMessage"]?.ToString();
            }

        }
        catch (Exception ex)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Message = ex.Message;
        }
        finally
        {
            await _context.DisConnect();
        }
        return response;
    }

    /// <summary>
    /// lấy danh sách chi tiết của từng cuốn sách
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<BookSerialModel>> GetBookSerialsAsync()
    {
        IEnumerable<BookSerialModel> data;
        try
        {
            await _context.Connect();
            string querry = @$"select T0.*, T1.BookName 
                                 from BookSerials as T0
                                inner join Books as t1 on T0.BookId = T1.BookId
                                where T0.[IsDelete] = 0 order by T0.Id desc";
            Func<IDataRecord, BookSerialModel> readData = record =>
            {
                BookSerialModel model = new BookSerialModel();
                if (!Convert.IsDBNull(record["Id"])) model.Id = Convert.ToInt32(record["Id"]);
                if (!Convert.IsDBNull(record["BookID"])) model.BookID = Convert.ToInt32(record["BookID"]);
                if (!Convert.IsDBNull(record["SerialNumber"])) model.SerialNumber = Convert.ToString(record["SerialNumber"]);
                if (!Convert.IsDBNull(record["NoteForAll"])) model.NoteForAll = Convert.ToString(record["NoteForAll"]);
                if (!Convert.IsDBNull(record["BookName"])) model.BookName = Convert.ToString(record["BookName"]);
                if (!Convert.IsDBNull(record["IsActive"])) model.IsActive = Convert.ToBoolean(record["IsActive"]);
                if (!Convert.IsDBNull(record["DateCreate"])) model.DateCreate = Convert.ToDateTime(record["DateCreate"]);
                if (!Convert.IsDBNull(record["UserCreate"])) model.UserCreate = Convert.ToInt32(record["UserCreate"]);
                return model;
            };
            data = await _context.GetDataAsync(querry, readData, commandType: CommandType.Text);
        }
        catch (Exception) { throw; }
        finally
        {
            await _context.DisConnect();
        }
        return data;
    }

    /// <summary>
    /// Thêm mới/Cập nhật thông tin loại sách
    /// </summary>
    /// <param name="pRequest"></param>
    /// <returns></returns>
    public async Task<ResponseModel> UpdateBookSerial(RequestModel pRequest)
    {
        ResponseModel response = new ResponseModel();
        try
        {
            await _context.Connect();
            string queryString = "";
            BookSerialModel oItem = JsonConvert.DeserializeObject<BookSerialModel>(pRequest.Json + "")!;
            SqlParameter[] sqlParameters;
            if(pRequest.Type == nameof(EnumType.Add))
            {
                sqlParameters = new SqlParameter[1];
                sqlParameters[0] = new SqlParameter("@SerialNumber", oItem.SerialNumber);
                // kiểm tra tồn tại mã đã có sẵn rồi
                if (await _context.ExecuteScalarAsync("select COUNT(*) from BookSerials with(nolock) where SerialNumber = @SerialNumber", sqlParameters) > 0)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Mã sách/Serial đã tồn tại!";
                    return response;
                }
                queryString = @"INSERT INTO [dbo].[BookSerials] ([BookID],[SerialNumber], [NoteForAll], [IsActive],[DateCreate],[UserCreate],[IsDelete])
                                                        values (@BookID , @SerialNumber, @NoteForAll, @IsActive , @DateTimeNow, @UserId, 0)";
            } 
            else
            {
                sqlParameters = new SqlParameter[2];
                sqlParameters[0] = new SqlParameter("@SerialNumber", oItem.SerialNumber);
                sqlParameters[1] = new SqlParameter("@Id", oItem.Id);
                // kiểm tra tồn tại mã đã có sẵn rồi
                if (await _context.ExecuteScalarAsync("select COUNT(*) from BookSerials with(nolock) where SerialNumber = @SerialNumber and [Id] != @Id", sqlParameters) > 0)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Mã sách/Serial đã tồn tại!";
                    return response;
                }
                queryString = @"Update [dbo].[BookSerials] 
                                   set [BookID] = @BookID
                                       ,[SerialNumber] = @SerialNumber
                                       ,[NoteForAll] = @NoteForAll
                                       ,[IsActive] = @IsActive
                                       ,[DateUpdate] = @DateTimeNow
                                       ,[UserUpdate] = @UserId
                                 where [Id] = @Id";
            }

            sqlParameters = new SqlParameter[7];
            sqlParameters[0] = new SqlParameter("@BookID", oItem.BookID);
            sqlParameters[1] = new SqlParameter("@SerialNumber", oItem.SerialNumber);
            sqlParameters[2] = new SqlParameter("@NoteForAll", oItem.NoteForAll ?? (object)DBNull.Value);
            sqlParameters[3] = new SqlParameter("@IsActive", oItem.IsActive);
            sqlParameters[4] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
            sqlParameters[5] = new SqlParameter("@UserId", pRequest.UserId);
            sqlParameters[6] = new SqlParameter("@Id", oItem.Id);
            var data = await _context.AddOrUpdateAsync(queryString, sqlParameters, CommandType.Text);
            if (data != null && data.Rows.Count > 0)
            {
                response.StatusCode = int.Parse(data.Rows[0]["StatusCode"]?.ToString() ?? "-1");
                response.Message = data.Rows[0]["ErrorMessage"]?.ToString();
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Message = ex.Message;
        }
        finally
        {
            await _context.DisConnect();
        }
        return response;
    }

    /// <summary>
    /// lấy danh sách cán bộ, sinh viên
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<StaffModel>> GetStaffsAsync(LoginRequestModel pRequest)
    {
        IEnumerable<StaffModel> data;
        try
        {
            string strCondition = "";
            SqlParameter[] sqlParameters = new SqlParameter[2];
            sqlParameters[0] = new SqlParameter("@StaffCode", "");
            sqlParameters[1] = new SqlParameter("@Password", "");
            if (pRequest.IsLogin)
            {
                strCondition = @" and T0.[StaffCode] = @StaffCode and T0.[Password] = @Password";
                sqlParameters[0].Value = pRequest.UserName;
                sqlParameters[1].Value = pRequest.Password;
            }    
            await _context.Connect();
            string querry = @$"Select *, case StaffType when N'SV' then N'Sinh viên' 
                                      when N'GV' then N'Giáo viên' when N'CB' then N'Cán bộ'
                                      else N'Sinh viên' end as StaffTypeName
                                 from [dbo].[Staffs] as T0 with(nolock)
                                where T0.[IsDelete] = 0 {strCondition}";
            Func<IDataRecord, StaffModel> readData = record =>
            {
                StaffModel model = new StaffModel();
                if (!Convert.IsDBNull(record["StaffCode"])) model.StaffCode = Convert.ToString(record["StaffCode"]);
                if (!Convert.IsDBNull(record["FullName"])) model.FullName = Convert.ToString(record["FullName"]);
                if (!Convert.IsDBNull(record["StaffType"])) model.StaffType = Convert.ToString(record["StaffType"]);
                if (!Convert.IsDBNull(record["StaffTypeName"])) model.StaffTypeName = Convert.ToString(record["StaffTypeName"]);
                if (!Convert.IsDBNull(record["Department"])) model.Department = Convert.ToString(record["Department"]);
                if (!Convert.IsDBNull(record["PhoneNumber"])) model.PhoneNumber = Convert.ToString(record["PhoneNumber"]);
                if (!Convert.IsDBNull(record["Email"])) model.Email = Convert.ToString(record["Email"]);
                if (!Convert.IsDBNull(record["PlaceOfOrigin"])) model.PlaceOfOrigin = Convert.ToString(record["PlaceOfOrigin"]);
                if (!Convert.IsDBNull(record["Address"])) model.Address = Convert.ToString(record["Address"]);
                if (!Convert.IsDBNull(record["IsActive"])) model.IsActive = Convert.ToBoolean(record["IsActive"]);
                return model;
            };
            data = await _context.GetDataAsync(querry, readData, sqlParameters, commandType: CommandType.Text);
        }
        catch (Exception) { throw; }
        finally
        {
            await _context.DisConnect();
        }
        return data;
    }
    #endregion Public Functions

    #region Private Funtions
    /// <summary>
    /// đọc danh sách Users
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    private UserModel DataRecordToUserModel(IDataRecord record)
    {
        UserModel user = new();
        if (!Convert.IsDBNull(record["Id"])) user.Id = Convert.ToInt32(record["Id"]);
        if (!Convert.IsDBNull(record["UserName"])) user.UserName = Convert.ToString(record["UserName"]);
        if (!Convert.IsDBNull(record["Password"])) user.Password = Convert.ToString(record["Password"]);
        if (!Convert.IsDBNull(record["LastPassword"])) user.LastPassword = Convert.ToString(record["LastPassword"]);
        if (!Convert.IsDBNull(record["FullName"])) user.FullName = Convert.ToString(record["FullName"]);
        if (!Convert.IsDBNull(record["PhoneNumber"])) user.PhoneNumber = Convert.ToString(record["PhoneNumber"]);
        if (!Convert.IsDBNull(record["Email"])) user.Email = Convert.ToString(record["Email"]);
        if (!Convert.IsDBNull(record["Address"])) user.Address = Convert.ToString(record["Address"]);
        if (!Convert.IsDBNull(record["DateOfBirth"])) user.DateOfBirth = Convert.ToDateTime(record["DateOfBirth"]);
        if (!Convert.IsDBNull(record["IsAdmin"])) user.IsAdmin = Convert.ToBoolean(record["IsAdmin"]);
        if (!Convert.IsDBNull(record["DateCreate"])) user.DateCreate = Convert.ToDateTime(record["DateCreate"]);
        if (!Convert.IsDBNull(record["UserCreate"])) user.UserCreate = Convert.ToInt32(record["UserCreate"]);
        if (!Convert.IsDBNull(record["DateUpdate"])) user.DateUpdate = Convert.ToDateTime(record["DateUpdate"]);
        if (!Convert.IsDBNull(record["UserUpdate"])) user.UserUpdate = Convert.ToInt32(record["UserUpdate"]);
        return user;
    }

    /// <summary>
    /// xóa dữ liệu -> cập nhật cột IsDelete
    /// </summary>
    /// <returns></returns>
    private async Task<ResponseModel> deleteDataAsync(string pTableName, string pCondition, SqlParameter[] sqlParameters)
    {
        ResponseModel response = new ResponseModel();
        try
        {
            await _context.BeginTranAsync();
            string queryString = @$"UPDATE [dbo].[{pTableName}] 
                                set [IsDelete] = 1, [ReasonDelete] = @ReasonDelete, [DateUpdate] = @DateTimeNow, [UserUpdate] = @UserId
                                where {pCondition}";

            var data = await _context.AddOrUpdateAsync(queryString, sqlParameters, CommandType.Text);
            if (data != null && data.Rows.Count > 0)
            {
                response.StatusCode = int.Parse(data.Rows[0]["StatusCode"]?.ToString() ?? "-1");
                response.Message = data.Rows[0]["ErrorMessage"]?.ToString();
            }

            if (response.StatusCode == 0) await _context.CommitTranAsync();
            else await _context.RollbackAsync();
        }
        catch (Exception ex)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Message = ex.Message;
            await _context.RollbackAsync();
        }
        return response;
    }

    /// <summary>
    /// lấy những thuộc tính cần thiết khi đăng nhập thành công
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    private UserModel DataRecordToUserModelByLogin(IDataRecord record)
    {
        UserModel user = new();
        if (!Convert.IsDBNull(record["Id"])) user.Id = Convert.ToInt32(record["Id"]);
        if (!Convert.IsDBNull(record["UserName"])) user.UserName = Convert.ToString(record["UserName"]);
        if (!Convert.IsDBNull(record["FullName"])) user.FullName = Convert.ToString(record["FullName"]);
        if (!Convert.IsDBNull(record["IsAdmin"])) user.IsAdmin = Convert.ToBoolean(record["IsAdmin"]);
        if (!Convert.IsDBNull(record["IsDeleted"])) user.IsDeleted = Convert.ToBoolean(record["IsDeleted"]);
        return user;
    }

    /// <summary>
    /// đọc danh sách loại sách
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    private KindBookModel DataRecordToKindBookModel(IDataRecord record)
    {
        KindBookModel kindBook = new();
        if (!Convert.IsDBNull(record["KindBookId"])) kindBook.KindBookId = Convert.ToInt32(record["KindBookId"]);
        if (!Convert.IsDBNull(record["KindBookName"])) kindBook.KindBookName = Convert.ToString(record["KindBookName"]);
        if (!Convert.IsDBNull(record["Description"])) kindBook.Description = Convert.ToString(record["Description"]);
        if (!Convert.IsDBNull(record["DateCreate"])) kindBook.DateCreate = Convert.ToDateTime(record["DateCreate"]);
        if (!Convert.IsDBNull(record["UserCreate"])) kindBook.UserCreate = Convert.ToInt32(record["UserCreate"]);
        if (!Convert.IsDBNull(record["DateUpdate"])) kindBook.DateUpdate = Convert.ToDateTime(record["DateUpdate"]);
        if (!Convert.IsDBNull(record["UserUpdate"])) kindBook.UserUpdate = Convert.ToInt32(record["UserUpdate"]);
        if (!Convert.IsDBNull(record["LocationId"])) kindBook.LocationId = Convert.ToInt32(record["LocationId"]);
        if (!Convert.IsDBNull(record["LocationName"])) kindBook.LocationName = Convert.ToString(record["LocationName"]);
        return kindBook;
    }

    /// <summary>
    /// đọc danh sách nhà xuất bản
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    private PublisherModel DataRecordToPublisherModel(IDataRecord record)
    {
        PublisherModel publisher = new();
        if (!Convert.IsDBNull(record["PublisherId"])) publisher.PublisherId = Convert.ToInt32(record["PublisherId"]);
        if (!Convert.IsDBNull(record["PublisherName"])) publisher.PublisherName = Convert.ToString(record["PublisherName"]);
        if (!Convert.IsDBNull(record["Description"])) publisher.Description = Convert.ToString(record["Description"]);
        if (!Convert.IsDBNull(record["DateCreate"])) publisher.DateCreate = Convert.ToDateTime(record["DateCreate"]);
        if (!Convert.IsDBNull(record["UserCreate"])) publisher.UserCreate = Convert.ToInt32(record["UserCreate"]);
        if (!Convert.IsDBNull(record["DateUpdate"])) publisher.DateUpdate = Convert.ToDateTime(record["DateUpdate"]);
        if (!Convert.IsDBNull(record["UserUpdate"])) publisher.UserUpdate = Convert.ToInt32(record["UserUpdate"]);
        return publisher;
    }

    /// <summary>
    /// đọc danh sách sách
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    private BookModel DataRecordToBookModel(IDataRecord record)
    {
        BookModel book = new();
        if (!Convert.IsDBNull(record["BookId"])) book.BookId = Convert.ToInt32(record["BookId"]);
        if (!Convert.IsDBNull(record["BookName"])) book.BookName = Convert.ToString(record["BookName"]);
        if (!Convert.IsDBNull(record["Description"])) book.Description = Convert.ToString(record["Description"]);
        //if (!Convert.IsDBNull(record["Price"])) book.Price = Convert.ToDecimal(record["Price"]);
        if (!Convert.IsDBNull(record["Qty"])) book.Qty = Convert.ToInt32(record["Qty"]);
        if (!Convert.IsDBNull(record["Language"])) book.Language = Convert.ToString(record["Language"]);
        if (!Convert.IsDBNull(record["Size"])) book.Size = Convert.ToString(record["Size"]);
        if (!Convert.IsDBNull(record["PublisherName"])) book.PublisherName = Convert.ToString(record["PublisherName"]);
        if (!Convert.IsDBNull(record["KindBookName"])) book.KindBookName = Convert.ToString(record["KindBookName"]);
        if (!Convert.IsDBNull(record["NumOfPage"])) book.NumOfPage = Convert.ToInt32(record["NumOfPage"]);
        if (!Convert.IsDBNull(record["DateCreate"])) book.DateCreate = Convert.ToDateTime(record["DateCreate"]);
        if (!Convert.IsDBNull(record["UserCreate"])) book.UserCreate = Convert.ToInt32(record["UserCreate"]);
        if (!Convert.IsDBNull(record["KindBookId"])) book.KindBookId = Convert.ToInt32(record["KindBookId"]);
        if (!Convert.IsDBNull(record["PublisherId"])) book.PublisherId = Convert.ToInt32(record["PublisherId"]);
        if (!Convert.IsDBNull(record["ImageUrl"]))
        {
            book.ImageUrl = Convert.ToString(record["ImageUrl"]);
            book.ImageUrlView = $"{_HOST_API}/{nameof(EnumTable.Books)}/{book.ImageUrl}";
        }    
        if (!Convert.IsDBNull(record["PublishingYear"])) book.PublishingYear = Convert.ToInt32(record["PublishingYear"]);
        if (!Convert.IsDBNull(record["Name"])) book.Name = Convert.ToString(record["Name"]);
        if (!Convert.IsDBNull(record["AuthorId"])) book.AuthorId = Convert.ToInt32(record["AuthorId"]);
        if (!Convert.IsDBNull(record["AuthorName"])) book.AuthorName = Convert.ToString(record["AuthorName"]);
        return book;
    }

    /// <summary>
    /// đọc danh sách đôc giả
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    private ReaderModel DataRecordToReaderModel(IDataRecord record)
    {
        ReaderModel reader = new();
        if (!Convert.IsDBNull(record["ReaderId"])) reader.ReaderId = Convert.ToInt32(record["ReaderId"]);
        if (!Convert.IsDBNull(record["FullName"])) reader.FullName = Convert.ToString(record["FullName"]);
        if (!Convert.IsDBNull(record["DateOfBrirth"])) reader.DateOfBirth = Convert.ToDateTime(record["DateOfBrirth"]);
        if (!Convert.IsDBNull(record["Gender"])) reader.Gender = Convert.ToString(record["Gender"]);
        if (!Convert.IsDBNull(record["Level"])) reader.Level = Convert.ToString(record["Level"]);
        if (!Convert.IsDBNull(record["Job"])) reader.Job = Convert.ToString(record["Job"]);
        if (!Convert.IsDBNull(record["WorkPlace"])) reader.WorkPlace = Convert.ToString(record["WorkPlace"]);
        if (!Convert.IsDBNull(record["IdentityCard"])) reader.IdentityCard = Convert.ToString(record["IdentityCard"]);
        if (!Convert.IsDBNull(record["DateCreate"])) reader.DateCreate = Convert.ToDateTime(record["DateCreate"]);
        if (!Convert.IsDBNull(record["UserCreate"])) reader.UserCreate = Convert.ToInt32(record["UserCreate"]);
        if (!Convert.IsDBNull(record["UserId"])) reader.UserId = Convert.ToInt32(record["UserId"]);
        return reader;
    }

    /// <summary>
    /// đọc danh sách sách client
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    private CliBookModel DataRecordToBookClientModel(IDataRecord record)
    {
        CliBookModel book = new();
        if (!Convert.IsDBNull(record["BookId"])) book.BookId = Convert.ToInt32(record["BookId"]);
        if (!Convert.IsDBNull(record["BookName"])) book.BookName = Convert.ToString(record["BookName"]);
        if (!Convert.IsDBNull(record["Description"])) book.Description = Convert.ToString(record["Description"]);
        if (!Convert.IsDBNull(record["Price"])) book.Price = Convert.ToDecimal(record["Price"]);
        if (!Convert.IsDBNull(record["Price"])) book.Qty = Convert.ToInt32(record["Qty"]);
        if (!Convert.IsDBNull(record["Language"])) book.Language = Convert.ToString(record["Language"]);
        if (!Convert.IsDBNull(record["Size"])) book.Size = Convert.ToString(record["Size"]);
        if (!Convert.IsDBNull(record["PublisherName"])) book.PublisherName = Convert.ToString(record["PublisherName"]);
        if (!Convert.IsDBNull(record["KindBookName"])) book.KindBookName = Convert.ToString(record["KindBookName"]);
        if (!Convert.IsDBNull(record["DateCreate"])) book.DateCreate = Convert.ToDateTime(record["DateCreate"]);
        if (!Convert.IsDBNull(record["UserCreate"])) book.UserCreate = Convert.ToInt32(record["UserCreate"]);
        if (!Convert.IsDBNull(record["KindBookId"])) book.KindBookId = Convert.ToInt32(record["KindBookId"]);
        if (!Convert.IsDBNull(record["PublisherId"])) book.PublisherId = Convert.ToInt32(record["PublisherId"]);
        if (!Convert.IsDBNull(record["FilePath"])) book.FilePath = Convert.ToString(record["FilePath"]);
        if (!Convert.IsDBNull(record["ImageId"])) book.ImageId = Convert.ToInt32(record["ImageId"]);
        return book;
    }


    /// <summary>
    /// lấy kết quả báo cáo
    /// </summary>
    /// <param name="isAdmin"></param>
    /// <returns></returns>
    private async Task<ResponseModel> CheckKeyBindingBeforeDeleting(RequestModel request = null)
    {
        ResponseModel data = new ResponseModel();
        try
        {
            SqlParameter[] sqlParameters = new SqlParameter[2];
            sqlParameters[0] = new SqlParameter("@Type", request.Type); // tableName
            sqlParameters[1] = new SqlParameter("@Json", request.Json); // ds các id cần update isdelete = 1
            var results = await _context.GetDataSetAsync(Constants.LM_CHECK_KEY_BINDING_BEFORE_DELETE, sqlParameters, commandType: CommandType.StoredProcedure);
            if (results.Tables != null && results.Tables.Count > 0 && results.Tables[0].Rows.Count > 0)
            {
                data.StatusCode = int.Parse(results.Tables[0].Rows[0]["StatusCode"].ToString());
                data.Message = results.Tables[0].Rows[0]["Message"].ToString();
            }
        }
        catch (Exception) { throw; }
        return data;
    }
    #endregion Private Funtions
}
