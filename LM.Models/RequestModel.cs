namespace LM.Models;

public class RequestModel
{
    public int UserId { get; set; }
    public string? Json { get; set; }
    public string? Type { get; set; }
    public string? JsonDetail { get; set; }
    public int BaseEntry { get; set; }
    public int BaseLine { get; set; }
}

public class ResponseModel
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public ResponseModel()
    {
        StatusCode = -1;
        Message = string.Empty;
    }

    public ResponseModel(int status, string? message)
    {
        StatusCode = status;
        Message = message;
    }
}

public class ComboboxModel
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool IsCheck{ get; set; }
}

public class SuppliesOutBoundModel
{
    public string? SuppliesCode { get; set; }
    public string? SuppliesName { get; set; }
    public decimal? Qty { get; set; }
    public decimal? QtyInv { get; set; }
    public string? EnumId { get; set; }
    public string? EnumName { get; set; }
    public int? BaseLine { get; set; }

}


public class SearchModel
{
    public int UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? StatusId { get; set; }
    public bool IsAdmin { get; set; }
    public int IdDraftDetail { get; set; }
    public string? BranchId { get; set; }
    public string? Type { get; set; }
    public string? CusNo { get; set; }
}

public enum EnumType
{
    @Add,
    @Update,
    @Delete,
    @SaveAndClose,
    @ChangePassWord
}

public enum EnumTable
{

    @Users,
    @KindBooks,
    @Publishers,
    @Books
}

public enum DocStatus
{
    @Pending,
    @Closed,
    @All,
    @Cancled
}

public enum TypeTime
{
    @Qui,
    @Thang
}

public enum ReportType
{
    @DoanhThuDichVuLoaiDichVu,
    @BaoCaoKPINhanVien,
    @BaoCaoNhapXuatKho
}

public enum Kind
{
    @QuiThang,
    @TuNgayDenNgay
}

public enum ServiceType
{
    @Service,
    @ServiceType
}

public enum UserType
{
    @ConsultUser,
    @ImplementUser
}

public enum ChartReportType
{
    @List,
    @Chart
}
