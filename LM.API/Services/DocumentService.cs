using LM.API.Infrastructure;
using LM.Models;
using LM.Models.Shared;
using Newtonsoft.Json;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.POIFS.FileSystem;
using NPOI.SS.Formula.Functions;
using System.Data;
using System.Data.SqlClient;
using System.Net;

namespace LM.API.Services
{
    public interface IDocumentService
    {
        Task<ResponseModel> UpdateBorrowOrderAsync(RequestModel pRequest);
        Task<IEnumerable<BorrowOrderModel>> GetBorrowOrdersAsync(SearchModel pSearchData);
        Task<Dictionary<string, string>?> GetDocumentById(string pVoucherNo);
        Task<ResponseModel> ReturnBooksAsync(RequestModel pRequest);
        Task<Dictionary<string, int>?> GetReportIndexAsync();
        Task<ResponseModel> CancleDocList(RequestModel pRequest);
        Task<IEnumerable<BorrowOrderModel>> GetDocumentByStaffAsync(string pStaffCode);
    }    
    public class DocumentService : IDocumentService
    {
        private readonly IBMDbContext _context;
        private readonly IDateTimeService _dateTimeService;
        private readonly IConfiguration _configuration;
        public DocumentService(IBMDbContext context, IDateTimeService dateTimeService, IConfiguration configuration)
        {
            _context = context;
            _dateTimeService = dateTimeService;
            _configuration = configuration;
        }

        /// <summary>
        /// cập nhật thông tin phiếu mượn
        /// </summary>
        /// <param name="pRequest"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateBorrowOrderAsync(RequestModel pRequest)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                await _context.Connect();
                string queryString = "";
                bool isUpdated = false;
                BorrowOrderModel oDraft = JsonConvert.DeserializeObject<BorrowOrderModel>(pRequest.Json + "")!;
                List<BODetailModel> lstDraftDetails = JsonConvert.DeserializeObject<List<BODetailModel>>(pRequest.JsonDetail + "");
                SqlParameter[] sqlParameters = new SqlParameter[1];
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
                        // lấy mã
                        int intId = await _context.ExecuteScalarAsync("select cast(isnull(max(VoucherNo), '0') as int) + 1 from [dbo].[BorrowOrders] with(nolock)");
                        oDraft.VoucherNo = $"000000000000000{intId}".Substring($"000000000000000{intId}".Length - 12); // lấy 12 số cuối
                        queryString = @"INSERT INTO [dbo].[BorrowOrders] ([VoucherNo], [StaffCode], [Description], [DocDate], [DueDate], [PromiseDate], [StatusCode], [TypeBO], [DateCreate], [UserCreate], [IsDelete])
                                                            values (@VoucherNo, @StaffCode, @Description , @DocDate, @DueDate, @PromiseDate, @StatusCode, @TypeBO,  @DateTimeNow, @UserId, 0)";
                        sqlParameters = getBOParameters(oDraft, pRequest.UserId);
                        await _context.BeginTranAsync();

                        isUpdated = await ExecQuery();
                        if(isUpdated)
                        {
                            // cập nhật thông tin chi tiết sách
                            queryString = @"INSERT INTO [dbo].[BODetails] ([VoucherNo], [BookSerialId], [StatusCode], [NoteForAll], [Quantity], [BookId])
                                                            values (@VoucherNo, @BookSerialId, @StatusCode, @NoteForAll, @Quantity, @BookId)";
                            foreach(var oItem in lstDraftDetails)
                            {
                                oItem.VoucherNo = oDraft.VoucherNo;
                                oItem.StatusCode = nameof(DocStatus.Pending);
                                sqlParameters = getBODetailsParameters(oItem);
                                isUpdated = await ExecQuery();
                                if (!isUpdated)
                                {
                                    await _context.RollbackAsync();
                                    return response;
                                }
                            }    
                        }    
                        if (isUpdated) await _context.CommitTranAsync();
                        else await _context.RollbackAsync();

                        break;
                    case nameof(EnumType.Update):
                        queryString = @"Update BorrowOrders set [StaffCode] = @StaffCode, [Description] = @Description
                                             , [DocDate] = @DocDate, [DueDate] = @DueDate, [PromiseDate] = @PromiseDate, [StatusCode] = @StatusCode
                                             , [DateUpdate] = @DateTimeNow, [UserUpdate] = @UserId 
                                         where VoucherNo = @VoucherNo";
                        sqlParameters = getBOParameters(oDraft, pRequest.UserId);
                        await _context.BeginTranAsync();
                        isUpdated = await ExecQuery();
                        if (isUpdated)
                        {
                            // xóa đi -> thêm vô lại
                            queryString = @"Delete from BODetails where VoucherNo = @VoucherNo";
                            sqlParameters = new SqlParameter[1];
                            sqlParameters[0] = new SqlParameter("@VoucherNo", oDraft.VoucherNo);
                            await ExecQuery();
                            // cập nhật thông tin chi tiết sách
                            queryString = @"INSERT INTO [dbo].[BODetails] ([VoucherNo], [BookSerialId], [StatusCode], [NoteForAll], [Quantity], [BookId])
                                                            values (@VoucherNo, @BookSerialId, @StatusCode, @NoteForAll, @Quantity, @BookId)";
                            foreach (var oItem in lstDraftDetails)
                            {
                                oItem.VoucherNo = oDraft.VoucherNo;
                                oItem.StatusCode = nameof(DocStatus.Pending);
                                sqlParameters = getBODetailsParameters(oItem);
                                isUpdated = await ExecQuery();
                                if (!isUpdated)
                                {
                                    await _context.RollbackAsync();
                                    return response;
                                }
                            }
                        }
                        if (isUpdated) await _context.CommitTranAsync();
                        else await _context.RollbackAsync();
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
        /// lấy danh sách mượn
        /// </summary>
        /// <param name="pSearchData"></param>
        /// <returns></returns>
        public async Task<IEnumerable<BorrowOrderModel>> GetBorrowOrdersAsync(SearchModel pSearchData)
        {
            IEnumerable<BorrowOrderModel> data;
            try
            {
                await _context.Connect();
                if (pSearchData.FromDate == null) pSearchData.FromDate = new DateTime(2023, 01, 01);
                if (pSearchData.ToDate == null) pSearchData.ToDate = _dateTimeService.GetCurrentVietnamTime();
                string querry = @$"select T0.*
                                          , case StaffType when N'SV' then N'Sinh viên' 
                                            when N'GV' then N'Giáo viên' when N'CB' then N'Cán bộ'
                                            else N'Sinh viên' end as StaffTypeName
		                                  , case T0.[StatusCode]  when '{nameof(DocStatus.Closed)}' then N'Đã trả sách'
                                            when '{nameof(DocStatus.Cancled)}' then N'Đã hủy phiếu'
                                            when '{nameof(DocStatus.Borrowing)}' then N'Đang mượn'
                                            else N'Chờ xử lý' end as [StatusName]
		                                  , T1.FullName, T1.PhoneNumber, T1.Email, T1.Department
                                       from BorrowOrders as T0 with(nolock)
                                 inner join Staffs as T1 with(nolock) on T0.StaffCode = T1.StaffCode 
                                      where cast(T0.[DocDate] as Date) between cast(@FromDate as Date) and cast(@ToDate as Date)
                                            and (@StatusId = 'All' or (@StatusId <> 'All' and T0.[StatusCode] = @StatusId))
                                            and (@TypeBO = 'All' or (@TypeBO <> 'All' and T0.[TypeBO] = @TypeBO))
                                   order by T0.DateCreate desc";
                SqlParameter[] sqlParameters = new SqlParameter[4];
                sqlParameters[0] = new SqlParameter("@StatusId", pSearchData.StatusId);
                sqlParameters[1] = new SqlParameter("@FromDate", pSearchData.FromDate.Value);
                sqlParameters[2] = new SqlParameter("@ToDate", pSearchData.ToDate.Value);
                sqlParameters[3] = new SqlParameter("@TypeBO", pSearchData.TypeBO);
                Func<IDataRecord, BorrowOrderModel> readData = record =>
                {
                    BorrowOrderModel model = new BorrowOrderModel();
                    if (!Convert.IsDBNull(record["VoucherNo"])) model.VoucherNo = Convert.ToString(record["VoucherNo"]);
                    if (!Convert.IsDBNull(record["StatusCode"])) model.StatusCode = Convert.ToString(record["StatusCode"]);
                    if (!Convert.IsDBNull(record["StatusName"])) model.StatusName = Convert.ToString(record["StatusName"]);
                    if (!Convert.IsDBNull(record["StaffCode"])) model.StaffCode = Convert.ToString(record["StaffCode"]);
                    if (!Convert.IsDBNull(record["FullName"])) model.FullName = Convert.ToString(record["FullName"]);
                    if (!Convert.IsDBNull(record["PhoneNumber"])) model.PhoneNumber = Convert.ToString(record["PhoneNumber"]);
                    if (!Convert.IsDBNull(record["StaffTypeName"])) model.StaffTypeName = Convert.ToString(record["StaffTypeName"]);
                    if (!Convert.IsDBNull(record["Description"])) model.Description = Convert.ToString(record["Description"]);
                    if (!Convert.IsDBNull(record["DocDate"])) model.DocDate = Convert.ToDateTime(record["DocDate"]);
                    if (!Convert.IsDBNull(record["DueDate"])) model.DueDate = Convert.ToDateTime(record["DueDate"]);
                    if (!Convert.IsDBNull(record["PromiseDate"])) model.PromiseDate = Convert.ToDateTime(record["PromiseDate"]);
                    if (!Convert.IsDBNull(record["TypeBO"])) model.TypeBO = Convert.ToString(record["TypeBO"]);
                    if (!Convert.IsDBNull(record["Email"])) model.Email = Convert.ToString(record["Email"]);
                    if (!Convert.IsDBNull(record["Department"])) model.Department = Convert.ToString(record["Department"]);
                    if (!Convert.IsDBNull(record["DateCreate"])) model.DateCreate = Convert.ToDateTime(record["DateCreate"]);
                    if (!Convert.IsDBNull(record["UserCreate"])) model.UserCreate = Convert.ToInt32(record["UserCreate"]);
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

        public async Task<Dictionary<string, string>?> GetDocumentById(string pVoucherNo)
        {
            Dictionary<string, string>? data = null;
            try
            {
                await _context.Connect();
                SqlParameter[] sqlParameters = new SqlParameter[1];
                sqlParameters[0] = new SqlParameter("@VoucherNo", pVoucherNo);

                string queryString = @$"select T0.*
					        		        , case StaffType when N'SV' then N'Sinh viên' 
                                              when N'GV' then N'Giáo viên' when N'CB' then N'Cán bộ'
                                              else N'Sinh viên' end as StaffTypeName
		                                    , case T0.[StatusCode]  when '{nameof(DocStatus.Closed)}' then N'Đã trả sách'
                                              when '{nameof(DocStatus.Cancled)}' then N'Đã hủy phiếu'
                                              when '{nameof(DocStatus.Borrowing)}' then N'Đang mượn'
                                              else N'Chờ xử lý' end as [StatusName]
                                            , T2.FullName, T2.PhoneNumber, T2.Email, T2.Department
									        , T3.BookId, T3.BookName, T1.StatusCode as DetailStatusCode, T1.NoteForAll, T1.[Id], T1.[Quantity]
									        , T1.BookSerialId, T4.SerialNumber
                                            ,t5.UserName as 'UserNameCreate'
                            from BorrowOrders as T0 with(nolock)
                                   inner join BODetails as T1 with(nolock) on T0.VoucherNo = T1.VoucherNo
                                   inner join Staffs as T2 with(nolock) on T0.StaffCode = T2.StaffCode 
                                   inner join Books as T3 with(nolock) on T1.BookId = T3.BookId
						            inner join  BookSerials as T4 with(nolock) on T1.BookSerialId = T4.Id
                                    left join Users as T5 with(nolock) on T0.userCreate = T5.Id
                                        where T0.VoucherNo = @VoucherNo";
                var ds = await _context.GetDataSetAsync(queryString, sqlParameters, CommandType.Text);
                data = new Dictionary<string, string>();
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    string DATA_CUSTOMER_EMPTY = "Chưa cập nhật";
                    DataTable dt = ds.Tables[0];
                    DataRow dr = dt.Rows[0];
                    BorrowOrderModel oHeader = new BorrowOrderModel();
                    oHeader.VoucherNo = Convert.ToString(dr["VoucherNo"]);
                    oHeader.StaffCode = Convert.ToString(dr["StaffCode"]);
                    oHeader.FullName = Convert.ToString(dr["FullName"]);
                    oHeader.Description = Convert.ToString(dr["Description"]);
                    oHeader.StatusCode = Convert.ToString(dr["StatusCode"]);
                    oHeader.StatusName = Convert.ToString(dr["StatusName"]);
                    oHeader.DocDate = Convert.ToDateTime(dr["DocDate"]);
                    if (!Convert.IsDBNull(dr["DueDate"])) oHeader.DueDate = Convert.ToDateTime(dr["DueDate"]);
                    if (!Convert.IsDBNull(dr["PromiseDate"])) oHeader.PromiseDate = Convert.ToDateTime(dr["PromiseDate"]);
                    oHeader.TypeBO = Convert.ToString(dr["TypeBO"]);
                    oHeader.StaffTypeName = Convert.ToString(dr["StaffTypeName"]);
                    oHeader.Department = Convert.ToString(dr["Department"]) ?? DATA_CUSTOMER_EMPTY;
                    oHeader.PhoneNumber = Convert.ToString(dr["PhoneNumber"]) ?? DATA_CUSTOMER_EMPTY;
                    oHeader.Email = Convert.ToString(dr["Email"]) ?? DATA_CUSTOMER_EMPTY;
                    if (!Convert.IsDBNull(dr["DateCreate"])) oHeader.DateCreate = Convert.ToDateTime(dr["DateCreate"]);
                    if (!Convert.IsDBNull(dr["UserCreate"])) oHeader.UserCreate = Convert.ToInt32(dr["UserCreate"]);
                    if (!Convert.IsDBNull(dr["DateUpdate"])) oHeader.DateUpdate = Convert.ToDateTime(dr["DateUpdate"]);
                    if (!Convert.IsDBNull(dr["UserUpdate"])) oHeader.UserUpdate = Convert.ToInt32(dr["UserUpdate"]);
                    if (!Convert.IsDBNull(dr["UserNameCreate"])) oHeader.UserNameCreate = Convert.ToString(dr["UserNameCreate"]);
                    oHeader.ReasonDelete = Convert.ToString(dr["ReasonDelete"]);
                    List<BODetailModel> lstDetails = new List<BODetailModel>();
                    foreach (DataRow item in dt.Rows)
                    {
                        BODetailModel oLine = new BODetailModel();
                        oLine.BookSerialId = Convert.ToInt32(item["BookSerialId"]);
                        oLine.BookId = Convert.ToInt32(item["BookId"]);
                        oLine.BookName = Convert.ToString(item["BookName"]);
                        oLine.SerialNumber = Convert.ToString(item["SerialNumber"]);
                        oLine.StatusCode = Convert.ToString(item["DetailStatusCode"]);
                        oLine.NoteForAll = Convert.ToString(item["NoteForAll"]);
                        oLine.Id = Convert.ToInt32(item["Id"]);
                        oLine.Quantity = Convert.ToInt32(item["Quantity"]);
                        oLine.StatusName = Convert.ToString(item["StatusName"]);
                        lstDetails.Add(oLine);
                    }
                    data = new Dictionary<string, string>()
                    {
                        {"oHeader", JsonConvert.SerializeObject(oHeader)},
                        {"oLine", JsonConvert.SerializeObject(lstDetails)}
                    };
                }    
            }
            catch (Exception) { throw; }
            finally
            {
                await _context.DisConnect();
            }
            return data;
        }

        public async Task<ResponseModel> ReturnBooksAsync(RequestModel pRequest)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                await _context.Connect();
                string queryString = "";
                bool isUpdated = false;
                string status = nameof(DocStatus.Closed);
                BorrowOrderModel oDraft = JsonConvert.DeserializeObject<BorrowOrderModel>(pRequest.Json + "")!;
                List<BODetailModel> lstDraftDetails = JsonConvert.DeserializeObject<List<BODetailModel>>(pRequest.JsonDetail + "");
                SqlParameter[] sqlParameters = new SqlParameter[1];
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
                if (pRequest.Type == nameof(DocStatus.All))
                {

                    // nếu trả hết
                    queryString = @$"Update BorrowOrders 
                                        set Description = @Description, StatusCode = @StatusCode, [DueDate] = @DateTimeNow
                                          , [DateUpdate] = @DateTimeNow, [UserUpdate] = @UserId where VoucherNo = @VoucherNo";
                    oDraft.StatusCode = status;
                    sqlParameters = getBOParameters(oDraft, pRequest.UserId);
                    await _context.BeginTranAsync();
                    isUpdated = await ExecQuery();
                    if (isUpdated)
                    {
                        // cập nhật thông tin chi tiết sách
                        queryString = @$"Update BODetails 
                                            set NoteForAll = @NoteForAll, StatusCode = @StatusCode
                                          where Id = @Id";
                        foreach (var oItem in lstDraftDetails)
                        {
                            oItem.VoucherNo = oDraft.VoucherNo;
                            oItem.StatusCode = status;
                            sqlParameters = getBODetailsParameters(oItem);
                            isUpdated = await ExecQuery();
                            if (!isUpdated)
                            {
                                await _context.RollbackAsync();
                                return response;
                            }
                        }
                    }
                    if (isUpdated) await _context.CommitTranAsync();
                    else await _context.RollbackAsync();
                }  
                else
                {
                    // Trả 1 cuốn sách
                    queryString = @$"Update BODetails 
                                            set NoteForAll = @NoteForAll, StatusCode = @StatusCode
                                          where Id = @Id";
                    await _context.BeginTranAsync();
                    var oItem = lstDraftDetails.First();
                    oItem.VoucherNo = oDraft.VoucherNo;
                    oItem.StatusCode = status;
                    sqlParameters = getBODetailsParameters(oItem);
                    isUpdated = await ExecQuery();
                    if(isUpdated)
                    {
                        // Kiểm tra đã hết sách chưa
                        sqlParameters = new SqlParameter[1];
                        sqlParameters[0] = new SqlParameter("@VoucherNo", oDraft.VoucherNo);
                        int count = await _context.ExecuteScalarAsync($"select count(*) from [dbo].[BODetails] where VoucherNo = @VoucherNo and StatusCode != '{DocStatus.Closed}'", sqlParameters);
                        if(count == 0)
                        {
                            // Đóng luôn phiếu
                            // nếu trả hết
                            queryString = @$"Update BorrowOrders 
                                        set Description = @Description, StatusCode = @StatusCode, [DueDate] = @DateTimeNow
                                          , [DateUpdate] = @DateTimeNow, [UserUpdate] = @UserId where VoucherNo = @VoucherNo";
                            oDraft.StatusCode = status;
                            sqlParameters = getBOParameters(oDraft, pRequest.UserId);
                            isUpdated = await ExecQuery();
                        }    
                    }

                    if (isUpdated) await _context.CommitTranAsync();
                    else await _context.RollbackAsync();
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
        /// lấy tính số lượng trên cái trang Index
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, int>?> GetReportIndexAsync()
        {
            Dictionary<string, int>? data = null;
            try
            {
                string queryString = string.Empty;
                await _context.Connect();
                int intTotal = await _context.ExecuteScalarAsync($"select count(*) from BorrowOrders where StatusCode <> '{nameof(DocStatus.Cancled)}'");
                int intBorrowing = await _context.ExecuteScalarAsync($"select count(*) from BorrowOrders where StatusCode = '{nameof(DocStatus.Borrowing)}'");
                int intClosed = await _context.ExecuteScalarAsync($"select count(*) from BorrowOrders where StatusCode = '{nameof(DocStatus.Closed)}'");
                int intPending = await _context.ExecuteScalarAsync($"select count(*) from BorrowOrders where StatusCode = '{nameof(DocStatus.Pending)}'");
                int intDemurrage = await _context.ExecuteScalarAsync(@$"select count(*) from BorrowOrders where StatusCode = '{nameof(DocStatus.Borrowing)}' 
                                   and DueDate is null and cast(PromiseDate as date) < cast(getdate() as date)"); // Trễ hạn

                int intToday = await _context.ExecuteScalarAsync(@$"select count(*) from BorrowOrders where StatusCode = '{nameof(DocStatus.Borrowing)}' 
                                   and DueDate is null and cast(PromiseDate as date) = cast(getdate() as date)"); // Trễ hạn

                data = new Dictionary<string, int>()
                {
                    {"intTotal", intTotal},
                    {"intBorrowing", intBorrowing},
                    {"intClosed", intClosed},
                    {"intDemurrage", intDemurrage},
                    {"intToday", intToday},
                    {"intPending", intPending},
                };
            }
            catch (Exception) { throw; }
            finally
            {
                await _context.DisConnect();
            }
            return data;
        }

        public async Task<ResponseModel> CancleDocList(RequestModel pRequest)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                //pRequest.Type: Chưa table Name
                await _context.Connect();
                SqlParameter[] sqlParameters;
                string queryString = @$"UPDATE [dbo].[{pRequest.Type}] 
                                      set [StatusCode] = '{nameof(DocStatus.Cancled)}', [ReasonDelete] = @ReasonDelete, [DateUpdate] = @DateTimeNow, [UserUpdate] = @UserId
                                    where [VoucherNo] in ( select value from STRING_SPLIT(@ListIds, ',') ) and [IsDelete] = 0";
                sqlParameters = new SqlParameter[4];
                sqlParameters[0] = new SqlParameter("@ReasonDelete", pRequest.JsonDetail ?? (object)DBNull.Value);
                sqlParameters[1] = new SqlParameter("@ListIds", pRequest.Json); // "1,2,3,4"
                sqlParameters[2] = new SqlParameter("@UserId", pRequest.UserId);
                sqlParameters[3] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
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
            finally
            {
                await _context.DisConnect();
            }
            return response;
        }
        
        public async Task<IEnumerable<BorrowOrderModel>> GetDocumentByStaffAsync(string pStaffCode)
        {
            IEnumerable<BorrowOrderModel> data;
            try
            {
                await _context.Connect();
                SqlParameter[] sqlParameters = new SqlParameter[1];
                sqlParameters[0] = new SqlParameter("@StaffCode", pStaffCode);
                string queryString = @$"select T0.*
                                          , case StaffType when N'SV' then N'Sinh viên' 
                                            when N'GV' then N'Giáo viên' when N'CB' then N'Cán bộ'
                                            else N'Sinh viên' end as StaffTypeName
		                                  , case T0.[StatusCode]  when '{nameof(DocStatus.Closed)}' then N'Đã trả sách'
                                            when '{nameof(DocStatus.Cancled)}' then N'Đã hủy phiếu'
                                            when '{nameof(DocStatus.Borrowing)}' then N'Đang mượn'
                                            else N'Chờ xử lý' end as [StatusName]
		                                  , T1.FullName, T1.PhoneNumber, T1.Email, T1.Department, T1.Address
										  , (select T3.BookId, T3.BookName, T01.StatusCode as DetailStatusCode, T01.NoteForAll, T01.[Id], T01.[Quantity]
									        , T01.BookSerialId, T4.SerialNumber from BODetails as T01 with(nolock) 
										  inner join Staffs as T2 with(nolock) on T0.StaffCode = T2.StaffCode 
                                          inner join Books as T3 with(nolock) on T01.BookId = T3.BookId
										  inner join  BookSerials as T4 with(nolock) on T01.BookSerialId = T4.Id
										    where T0.VoucherNo = T01.VoucherNo for json path) as 'JsonDetails'
                                       from BorrowOrders as T0 with(nolock)
                                 inner join Staffs as T1 with(nolock) on T0.StaffCode = T1.StaffCode 
                                      where T1.StaffCode = @StaffCode
                                   order by T0.DateCreate desc";
                Func<IDataRecord, BorrowOrderModel> readData = record =>
                {
                    BorrowOrderModel model = new BorrowOrderModel();
                    if (!Convert.IsDBNull(record["VoucherNo"])) model.VoucherNo = Convert.ToString(record["VoucherNo"]);
                    if (!Convert.IsDBNull(record["StatusCode"])) model.StatusCode = Convert.ToString(record["StatusCode"]);
                    if (!Convert.IsDBNull(record["StatusName"])) model.StatusName = Convert.ToString(record["StatusName"]);
                    if (!Convert.IsDBNull(record["StaffCode"])) model.StaffCode = Convert.ToString(record["StaffCode"]);
                    if (!Convert.IsDBNull(record["FullName"])) model.FullName = Convert.ToString(record["FullName"]);
                    if (!Convert.IsDBNull(record["PhoneNumber"])) model.PhoneNumber = Convert.ToString(record["PhoneNumber"]);
                    if (!Convert.IsDBNull(record["StaffTypeName"])) model.StaffTypeName = Convert.ToString(record["StaffTypeName"]);
                    if (!Convert.IsDBNull(record["Address"])) model.Address = Convert.ToString(record["Address"]);
                    if (!Convert.IsDBNull(record["Description"])) model.Description = Convert.ToString(record["Description"]);
                    if (!Convert.IsDBNull(record["DocDate"])) model.DocDate = Convert.ToDateTime(record["DocDate"]);
                    if (!Convert.IsDBNull(record["DueDate"])) model.DueDate = Convert.ToDateTime(record["DueDate"]);
                    if (!Convert.IsDBNull(record["PromiseDate"])) model.PromiseDate = Convert.ToDateTime(record["PromiseDate"]);
                    if (!Convert.IsDBNull(record["TypeBO"])) model.TypeBO = Convert.ToString(record["TypeBO"]);
                    if (!Convert.IsDBNull(record["Email"])) model.Email = Convert.ToString(record["Email"]);
                    if (!Convert.IsDBNull(record["Department"])) model.Department = Convert.ToString(record["Department"]);
                    if (!Convert.IsDBNull(record["DateCreate"])) model.DateCreate = Convert.ToDateTime(record["DateCreate"]);
                    if (!Convert.IsDBNull(record["UserCreate"])) model.UserCreate = Convert.ToInt32(record["UserCreate"]);
                    if (!Convert.IsDBNull(record["JsonDetails"])) model.JsonDetails = Convert.ToString(record["JsonDetails"]);
                    return model;
                };
                data = await _context.GetDataAsync(queryString, readData, sqlParameters, commandType: CommandType.Text);
            }
            catch (Exception) { throw; }
            finally
            {
                await _context.DisConnect();
            }
            return data;
        }    
        #region Private Functions

        /// <summary>
        /// set params cho BO
        /// </summary>
        /// <param name="oDraft"></param>
        /// <param name="pUserId"></param>
        /// <returns></returns>
        private SqlParameter[] getBOParameters(BorrowOrderModel oDraft, int pUserId)
        {
            SqlParameter[] sqlParameters = new SqlParameter[10];
            sqlParameters[0] = new SqlParameter("@VoucherNo", oDraft.VoucherNo);
            sqlParameters[1] = new SqlParameter("@StaffCode", oDraft.StaffCode);
            sqlParameters[2] = new SqlParameter("@Description", oDraft.Description ?? (object)DBNull.Value);
            sqlParameters[3] = new SqlParameter("@DocDate", oDraft.DocDate);
            sqlParameters[4] = new SqlParameter("@PromiseDate", oDraft.PromiseDate ?? (object)DBNull.Value);
            sqlParameters[5] = new SqlParameter("@DueDate", oDraft.DueDate ?? (object)DBNull.Value);
            sqlParameters[6] = new SqlParameter("@StatusCode", oDraft.StatusCode);
            sqlParameters[7] = new SqlParameter("@TypeBO", oDraft.TypeBO);
            sqlParameters[8] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
            sqlParameters[9] = new SqlParameter("@UserId", pUserId);
            return sqlParameters;
        }

        private SqlParameter[] getBODetailsParameters(BODetailModel oDraft)
        {
            SqlParameter[] sqlParameters = new SqlParameter[8];
            sqlParameters[0] = new SqlParameter("@VoucherNo", oDraft.VoucherNo);
            sqlParameters[1] = new SqlParameter("@BookSerialId", oDraft.BookSerialId);
            sqlParameters[2] = new SqlParameter("@StatusCode", oDraft.StatusCode);
            sqlParameters[3] = new SqlParameter("@NoteForAll", oDraft.NoteForAll ?? (object)DBNull.Value);
            sqlParameters[4] = new SqlParameter("@Quantity", oDraft.Quantity);
            sqlParameters[5] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
            sqlParameters[6] = new SqlParameter("@Id", oDraft.Id);
            sqlParameters[7] = new SqlParameter("@BookId", oDraft.BookId);
            return sqlParameters;
        }


        #endregion
    }
}
