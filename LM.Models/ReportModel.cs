using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM.Models;
public class ReportModel : Auditable
{
    public double TotalReveune { get; set; }
    public string? FullName { get; set; }
    public int Quantity { get; set; }
    public int? BookId { get; set; }
    public string? BookName { get; set; }
    public string? Language { get; set; }
    public string? Size { get; set; }
    public string? NumOfPage { get; set; }
    public int? AuthorId { get; set; }
    public int? KindBookId { get; set; }
    public int? PublisherId { get; set; }
    public int? PublishingYear { get; set; }
    public string? AuthorName { get; set; }
    public string? KindBookName { get; set; }
    public string? PublisherName { get; set; }
    public double Total_01 { get; set; }
    public double Total_02 { get; set; }
    public double Total_03 { get; set; }
    public double Total_04 { get; set; }
    public double Total_05 { get; set; }
    public double Total_06 { get; set; }
    public double Total_07 { get; set; }
    public double Total_08 { get; set; }
    public double Total_09 { get; set; }
    public double Total_10 { get; set; }
    public double Total_11 { get; set; }
    public double Total_12{ get; set; }
    public double LineTotal { get; set; }
    public string? Color_01 { get; set; }
    public string? Color_02 { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Time { get; set; }
}

public class RequestReportModel
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? TypeTime { get; set; } // quí or tháng
    public string? Type { get; set; } // loại báo cáo ( doanh thu theo ldv hay doanh thu theo dv)
    public int UserId { get; set; }
    public int Year  { get; set; }
}



public class ReportChartModel
{
    public List<object> ListValue { get; set; }
    public string[] ListTitle { get; set; }
    public int? BookId { get; set; }
    public string? BookName { get; set; }
    public string? Language { get; set; }
    public string? Size { get; set; }
    public string? NumOfPage { get; set; }
    public int? AuthorId { get; set; }
    public int? KindBookId { get; set; }
    public int? PublishingYear { get; set; }
    public string? AuthorName { get; set; }
    public string? KindBookName { get; set; }
}


