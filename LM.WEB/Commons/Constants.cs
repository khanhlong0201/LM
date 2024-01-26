namespace LM.WEB.Commons;

public static class DefaultConstants
{
    public static readonly DateTime MIN_DATE = DateTime.Now.AddYears(-1);
    public static readonly DateTime MAX_DATE = DateTime.Now.AddYears(1);
    public const string FORMAT_CURRENCY = "#,###0.##";
    public const string FORMAT_GRID_CURRENCY = "{0: #,###0.####}";
    public const string FORMAT_DATE = "dd/MM/yyyy";
    public const string FORMAT_GRID_DATE = "{0: dd/MM/yyyy}";
    public const string FORMAT_GRID_DATE_TIME = "{0: HH:mm dd/MM/yyyy}";
    public const string FORMAT_DATE_TIME = "HH:mm dd/MM/yyyy";
    public const string FORMAT_TIME = "HH:mm";
    public const int PAGE_SIZE = 100;

    public const string MESSAGE_INVALID_DATA = "Không đúng định dạng dữ liệu!";
    public const string MESSAGE_LOGIN_EXPIRED = "Hết phiên đăng nhập!";
    public const string MESSAGE_INSERT = "Đã tạo mới";
    public const string MESSAGE_UPDATE = "Đã cập nhât";
    public const string MESSAGE_DELETE = "Đã xóa các dòng được chọn!";
    public const string MESSAGE_NO_CHOSE_DATA = "Không có dòng nào được chọn!";
    public const string MESSAGE_CONFIRM_DELETE = "Bạn có chắc muốn xóa các dòng được chọn?";
    public const string MESSAGE_NO_DATA = "Không tìm thấy dữ liệu. Vui lòng thử lại!";

    public const string FOLDER_BOOK = "ImagesBook";
}


public static class EndpointConstants
{
    public const string URL_MASTERDATA_GET_USER = "MasterData/GetUsers";
    public const string URL_MASTERDATA_UPDATE_USER = "MasterData/UpdateUser";
    public const string URL_MASTERDATA_USER_LOGIN = "MasterData/Login";
    public const string URL_MASTERDATA_DELETE = "MasterData/DeleteData";

    public const string URL_MASTERDATA_GET_KINDBOOK = "MasterData/GetKindBooks";
    public const string URL_MASTERDATA_UPDATE_KINDBOOK = "MasterData/UpdateKindBook";

    public const string URL_MASTERDATA_GET_PUBLISHER = "MasterData/GetPublishers";
    public const string URL_MASTERDATA_UPDATE_PUBLISHER = "MasterData/UpdatePublisher";


    public const string URL_MASTERDATA_GET_BOOK = "MasterData/GetBooks";
    public const string URL_MASTERDATA_UPDATE_BOOK = "MasterData/UpdateBook";

    public const string URL_MASTERDATA_GET_IMAGE_DETAILS = "MasterData/GetImageDetails";
}