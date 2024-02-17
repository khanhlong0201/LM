using LM.API.Infrastructure;
using LM.Models;
using LM.Models.Shared;
using Newtonsoft.Json;
using NPOI.POIFS.FileSystem;
using System.Data;
using System.Data.SqlClient;
using System.Net;

namespace LM.API.Services
{
    public interface IDocumentService
    {
        Task<ResponseModel> UpdateBorrowOrderAsync(RequestModel pRequest);
        Task<IEnumerable<BorrowOrderModel>> GetBorrowOrdersAsync(SearchModel pSearchData);
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
                            queryString = @"INSERT INTO [dbo].[BODetails] ([VoucherNo], [BookSerialId], [StatusCode], [NoteForAll], [Quantity])
                                                            values (@VoucherNo, @BookSerialId, @StatusCode, @NoteForAll, @Quantity)";
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
                                            else N'Chờ xử lý' end as [StatusName]
		                                  , T1.FullName, T1.PhoneNumber, T1.Email, T1.Department
                                       from BorrowOrders as T0 with(nolock)
                                 inner join Staffs as T1 with(nolock) on T0.StaffCode = T1.StaffCode 
                                      where cast(T0.[DocDate] as Date) between cast(@FromDate as Date) and cast(@ToDate as Date)
                                   order by T0.DateCreate desc";
                SqlParameter[] sqlParameters = new SqlParameter[5];
                sqlParameters[0] = new SqlParameter("@StatusId", pSearchData.StatusId);
                sqlParameters[1] = new SqlParameter("@FromDate", pSearchData.FromDate.Value);
                sqlParameters[2] = new SqlParameter("@ToDate", pSearchData.ToDate.Value);
                sqlParameters[3] = new SqlParameter("@IsAdmin", pSearchData.IsAdmin);
                sqlParameters[4] = new SqlParameter("@UserId", pSearchData.UserId);
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
            SqlParameter[] sqlParameters = new SqlParameter[7];
            sqlParameters[0] = new SqlParameter("@VoucherNo", oDraft.VoucherNo);
            sqlParameters[1] = new SqlParameter("@BookSerialId", oDraft.BookSerialId);
            sqlParameters[2] = new SqlParameter("@StatusCode", oDraft.StatusCode);
            sqlParameters[3] = new SqlParameter("@NoteForAll", oDraft.NoteForAll ?? (object)DBNull.Value);
            sqlParameters[4] = new SqlParameter("@Quantity", oDraft.Quantity);
            sqlParameters[5] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
            sqlParameters[6] = new SqlParameter("@Id", oDraft.Id);
            return sqlParameters;
        }


        #endregion
    }
}
