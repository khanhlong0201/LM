using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM.Models.Shared;
public interface IDateTimeService
{
    DateTime GetCurrentVietnamTime();
}
public class VietnamDateTimeService : IDateTimeService
{
    private readonly TimeZoneInfo vietnamTimeZone;
    public VietnamDateTimeService()
    {
        // Lấy múi giờ của Việt Nam
        vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
    }

    public DateTime GetCurrentVietnamTime()
    {
        // Lấy thời gian hiện tại theo múi giờ Việt Nam
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
    }
}
