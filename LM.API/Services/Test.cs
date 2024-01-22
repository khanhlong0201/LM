///// <summary>
///// Thêm mới/Cập nhật thông tin nhà xuất bản
///// </summary>
///// <param name="pRequest"></param>
///// <returns></returns>
//public async Task<ResponseModel> UpdatePublishers(RequestModel pRequest)
//{
//    ResponseModel response = new ResponseModel();
//    try
//    {
//        await _context.Connect();
//        string queryString = "";
//        PublisherModel oPublisher = JsonConvert.DeserializeObject<PublisherModel>(pRequest.Json + "")!;
//        SqlParameter[] sqlParameters;
//        async Task ExecQuery()
//        {
//            var data = await _context.AddOrUpdateAsync(queryString, sqlParameters, CommandType.Text);
//            if (data != null && data.Rows.Count > 0)
//            {
//                response.StatusCode = int.Parse(data.Rows[0]["StatusCode"]?.ToString() ?? "-1");
//                response.Message = data.Rows[0]["ErrorMessage"]?.ToString();
//            }
//        }
//        switch (pRequest.Type)
//        {
//            case nameof(EnumType.Add):
//                sqlParameters = new SqlParameter[1];
//                sqlParameters[0] = new SqlParameter("@KindBookName", oPublisher.KindBookName);
//                // kiểm tra tên đăng nhập
//                if (await _context.ExecuteScalarAsync("select COUNT(*) from KindBooks with(nolock) where KindBookName = @KindBookName", sqlParameters) > 0)
//                {
//                    response.StatusCode = (int)HttpStatusCode.BadRequest;
//                    response.Message = "Tên đăng nhập đã tồn tại!";
//                    break;
//                }
//                queryString = @"INSERT INTO [dbo].[KindBooks] ([KindBookName],[Description],[DateCreate],[UserCreate],[IsDelete])
//                                                        values (@KindBookName , @Description , @DateTimeNow, @UserId, 0)";

//                sqlParameters = new SqlParameter[4];
//                sqlParameters[0] = new SqlParameter("@KindBookName", oPublisher.KindBookName ?? (object)DBNull.Value);
//                sqlParameters[1] = new SqlParameter("@Description", oPublisher.Description ?? (object)DBNull.Value);
//                sqlParameters[2] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
//                sqlParameters[3] = new SqlParameter("@UserId", pRequest.UserId);
//                await ExecQuery();
//                break;
//            case nameof(EnumType.Update):
//                queryString = @"UPDATE [dbo].[KindBooks]
//                               SET [KindBookName] =@KindBookName
//                                  ,[Description] = @Description
//                                  ,[DateUpdate] = @DateTimeNow
//                                  ,[UserUpdate] = @UserId
//                                 WHERE [KindBookId] = @KindBookId";

//                sqlParameters = new SqlParameter[5];
//                sqlParameters[0] = new SqlParameter("@KindBookId", oPublisher.KindBookId);
//                sqlParameters[1] = new SqlParameter("@KindBookName", oPublisher.KindBookName ?? (object)DBNull.Value);
//                sqlParameters[2] = new SqlParameter("@Description", oPublisher.Description ?? (object)DBNull.Value);
//                sqlParameters[3] = new SqlParameter("@UserId", pRequest.UserId);
//                sqlParameters[4] = new SqlParameter("@DateTimeNow", _dateTimeService.GetCurrentVietnamTime());
//                await ExecQuery();
//                break;
//            default:
//                response.StatusCode = (int)HttpStatusCode.BadRequest;
//                response.Message = "Không xác định được phương thức!";
//                break;
//        }
//    }
//    catch (Exception ex)
//    {
//        response.StatusCode = (int)HttpStatusCode.BadRequest;
//        response.Message = ex.Message;
//    }
//    finally
//    {
//        await _context.DisConnect();
//    }
//    return response;
//}