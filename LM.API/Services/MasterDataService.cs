using LM.API.Commons;
using LM.API.Infrastructure;
using LM.Models;
using LM.Models.Shared;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;

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
    Task<IEnumerable<BookModel>> GetBooksAsync();
    Task<ResponseModel> UpdateBooks(RequestModel pRequest);
}

public class MasterDataService : IMasterDataService
{
    private readonly IBMDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    public MasterDataService(IBMDbContext context, IDateTimeService dateTimeService)
    {
        _context = context;
        _dateTimeService = dateTimeService;
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
                    queryString = @"INSERT INTO [dbo].[Users]([UserName],[Password],[LastPassword],[FullName],[PhoneNumber] ,[Email] ,[Address],[DateOfBirth],[IsAdmin],[DateCreate] ,[UserCreate],[Isdelete] )
                                                        values (@UserName , @Password , @LastPassword, @FullName, @PhoneNumber , @Email, @Address, @DateOfBirth, @IsAdmin, @DateTimeNow, @UserId, 0)";

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
                    // kiểm tra điều kiện trước khi xóa
                    //
                    queryString = "[KindBookId] in ( select value from STRING_SPLIT(@ListIds, ',') ) and [IsDelete] = 0";
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
                    response = await deleteDataAsync(nameof(EnumTable.KindBooks), queryString, sqlParameters);
                    break;
                case nameof(EnumTable.Publishers):
                    // kiểm tra điều kiện trước khi xóa
                    //
                    queryString = "[PublisherId] in ( select value from STRING_SPLIT(@ListIds, ',') ) and [IsDelete] = 0";
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
                    response = await deleteDataAsync(nameof(EnumTable.Publishers), queryString, sqlParameters);
                    break;
                case nameof(EnumTable.Books):
                    // kiểm tra điều kiện trước khi xóa
                    //
                    queryString = "[BookId] in ( select value from STRING_SPLIT(@ListIds, ',') ) and [IsDelete] = 0";
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
                    response = await deleteDataAsync(nameof(EnumTable.Books), queryString, sqlParameters);
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
            data = await _context.GetDataAsync(@$"SELECT [KindBookId]
                                                  ,[KindBookName]
                                                  ,[Description]
                                                  ,[DateCreate]
                                                  ,[UserCreate]
                                                  ,[DateUpdate]
                                                  ,[UserUpdate]
                                              FROM [dbo].[KindBooks] t0 where ISNULL(t0.IsDelete,0) = 0" // không lấy lên tk Support
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
                    queryString = @"INSERT INTO [dbo].[KindBooks] ([KindBookName],[Description],[DateCreate],[UserCreate],[IsDelete])
                                                        values (@KindBookName , @Description , @DateTimeNow, @UserId, 0)";

                    sqlParameters = new SqlParameter[4];
                    sqlParameters[0] = new SqlParameter("@KindBookName", oKindBook.KindBookName ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@Description", oKindBook.Description ?? (object)DBNull.Value);
                    sqlParameters[2] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    sqlParameters[3] = new SqlParameter("@UserId", pRequest.UserId);
                    await ExecQuery();
                    break;
                case nameof(EnumType.Update):
                    queryString = @"UPDATE [dbo].[KindBooks]
                               SET [KindBookName] =@KindBookName
                                  ,[Description] = @Description
                                  ,[DateUpdate] = @DateTimeNow
                                  ,[UserUpdate] = @UserId
                                 WHERE [KindBookId] = @KindBookId";

                    sqlParameters = new SqlParameter[5];
                    sqlParameters[0] = new SqlParameter("@KindBookId", oKindBook.KindBookId);
                    sqlParameters[1] = new SqlParameter("@KindBookName", oKindBook.KindBookName ?? (object)DBNull.Value);
                    sqlParameters[2] = new SqlParameter("@Description", oKindBook.Description ?? (object)DBNull.Value);
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
    public async Task<IEnumerable<BookModel>> GetBooksAsync()
    {
        IEnumerable<BookModel> data;
        try
        {
            await _context.Connect();
            data = await _context.GetDataAsync(@$"t0.[BookId]
                                                  ,t0.[BookName]
                                                  ,t0.[Description]
                                                  ,isnull(t0.[Price],0) as 'Price'
                                                  ,isnull(t0.[Qty],0) as 'Qty'
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
	                                              ,t4.FilePath
                                              FROM [dbo].[Books] t0 
                                              inner join  Publishers t1 on t0.PublisherId = t1.PublisherId
                                              inner join  KindBooks t2 on t0.KindBookid = t2.KindBookid
                                              left join  Images t3 on t0.ImageId = t3.ImageId
                                              left join  ImageDetails t4 on t3.ImageId = t4.ImageId where ISNULL(t0.IsDelete,0) = 0"
                    , DataRecordToBookModel, commandType: CommandType.Text);
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
            BookModel oBook = JsonConvert.DeserializeObject<BookModel>(pRequest.Json + "")!;
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
                    sqlParameters[0] = new SqlParameter("@BookName", oBook.BookName);
                    // kiểm tra tên đăng nhập
                    if (await _context.ExecuteScalarAsync("select COUNT(*) from Books with(nolock) where BookName = @BookName", sqlParameters) > 0)
                    {
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        response.Message = "Tên đăng nhập đã tồn tại!";
                        break;
                    }
                    queryString = @"INSERT INTO [dbo].[Books] ([BookName], [Description], [Price], [Qty], [Language], [Size], [NumOfPage], [DateCreate], [UserCreate], [IsDelete], [KindBookId], [PublisherId], [ImageId])
                                                            values (@BookName , @Description , @Price, @Qty, @Language, @Size, @NumOfPage,  @DateTimeNow, @UserId, 0, @KindBookId, @PublisherId, @ImageId )";

                    sqlParameters = new SqlParameter[12];
                    sqlParameters[0] = new SqlParameter("@BookName", oBook.BookName ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@Description", oBook.Description ?? (object)DBNull.Value);
                    sqlParameters[2] = new SqlParameter("@Price", oBook.Price ?? (object)DBNull.Value);
                    sqlParameters[3] = new SqlParameter("@Qty", oBook.Qty ?? (object)DBNull.Value);
                    sqlParameters[4] = new SqlParameter("@Language", oBook.Language ?? (object)DBNull.Value);
                    sqlParameters[5] = new SqlParameter("@Size", oBook.Size ?? (object)DBNull.Value);
                    sqlParameters[6] = new SqlParameter("@NumOfPage", oBook.NumOfPage ?? (object)DBNull.Value);
                    sqlParameters[7] = new SqlParameter("@KindBookId", oBook.KindBookId ?? (object)DBNull.Value);
                    sqlParameters[8] = new SqlParameter("@PublisherId", oBook.PublisherId ?? (object)DBNull.Value);
                    sqlParameters[9] = new SqlParameter("@ImageId", oBook.ImageId ?? (object)DBNull.Value);
                    sqlParameters[10] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    sqlParameters[11] = new SqlParameter("@UserId", pRequest.UserId);
                    await ExecQuery();
                    break;
                case nameof(EnumType.Update):
                    queryString = @"UPDATE [dbo].[Books]
                               SET [BookName] = @BookName
                                  ,[Description] = @Description 
                                  ,[Price] = @Price
                                  ,[Qty] = @Qty
                                  ,[Language] = @Language
                                  ,[Size] = @Size
                                  ,[NumOfPage] = @NumOfPage
                                  ,[KindBookId] = @KindBookId
                                  ,[PublisherId] = @PublisherId
                                  ,[ImageId] = @ImageId
                                  ,[DateUpdate] = @DateTimeNow
                                  ,[UserUpdate] = @UserId
                                 WHERE [BookId] = @BookId";

                    sqlParameters = new SqlParameter[12];
                    sqlParameters[0] = new SqlParameter("@BookName", oBook.BookName ?? (object)DBNull.Value);
                    sqlParameters[1] = new SqlParameter("@Description", oBook.Description ?? (object)DBNull.Value);
                    sqlParameters[2] = new SqlParameter("@Price", oBook.Price ?? (object)DBNull.Value);
                    sqlParameters[3] = new SqlParameter("@Qty", oBook.Qty ?? (object)DBNull.Value);
                    sqlParameters[4] = new SqlParameter("@Language", oBook.Language ?? (object)DBNull.Value);
                    sqlParameters[5] = new SqlParameter("@Size", oBook.Size ?? (object)DBNull.Value);
                    sqlParameters[6] = new SqlParameter("@NumOfPage", oBook.NumOfPage ?? (object)DBNull.Value);
                    sqlParameters[7] = new SqlParameter("@KindBookId", oBook.KindBookId ?? (object)DBNull.Value);
                    sqlParameters[8] = new SqlParameter("@PublisherId", oBook.PublisherId ?? (object)DBNull.Value);
                    sqlParameters[9] = new SqlParameter("@ImageId", oBook.ImageId ?? (object)DBNull.Value);
                    sqlParameters[10] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
                    sqlParameters[11] = new SqlParameter("@UserId", pRequest.UserId);
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
        if (!Convert.IsDBNull(record["Price"])) book.Price = Convert.ToDecimal(record["Price"]);
        if (!Convert.IsDBNull(record["Price"])) book.Qty = Convert.ToInt32(record["Qty"]);
        if (!Convert.IsDBNull(record["Language"])) book.Language = Convert.ToString(record["Language"]);
        if (!Convert.IsDBNull(record["Size"])) book.Size = Convert.ToString(record["Size"]);
        if (!Convert.IsDBNull(record["PublisherName"])) book.PublisherName = Convert.ToString(record["PublisherName"]);
        if (!Convert.IsDBNull(record["KindBookName"])) book.KindBookName = Convert.ToString(record["KindBookName"]);
        if (!Convert.IsDBNull(record["FilePath"])) book.FilePath = Convert.ToString(record["FilePath"]);
        if (!Convert.IsDBNull(record["NumOfPage"])) book.NumOfPage = Convert.ToInt32(record["NumOfPage"]);
        if (!Convert.IsDBNull(record["DateCreate"])) book.DateCreate = Convert.ToDateTime(record["DateCreate"]);
        if (!Convert.IsDBNull(record["UserCreate"])) book.UserCreate = Convert.ToInt32(record["UserCreate"]);
        if (!Convert.IsDBNull(record["DateUpdate"])) book.DateUpdate = Convert.ToDateTime(record["DateUpdate"]);
        if (!Convert.IsDBNull(record["UserUpdate"])) book.UserUpdate = Convert.ToInt32(record["UserUpdate"]);
        return book;
    }
    #endregion Private Funtions
}
