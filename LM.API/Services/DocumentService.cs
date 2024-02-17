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
