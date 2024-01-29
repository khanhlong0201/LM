//private SeriesModel DataRecordToSeriesModel(IDataRecord record)
//{
//    SeriesModel series = new();
//    if (!Convert.IsDBNull(record["BatchId"])) series.BatchId = Convert.ToInt32(record["BatchId"]);
//    if (!Convert.IsDBNull(record["Qty"])) series.Qty = Convert.ToInt32(record["Qty"]);
//    if (!Convert.IsDBNull(record["Price"])) series.Price = Convert.ToInt32(record["Price"]);
//    if (!Convert.IsDBNull(record["DateCreate"])) series.DateCreate = Convert.ToDateTime(record["DateCreate"]);
//    if (!Convert.IsDBNull(record["UserCreate"])) series.UserCreate = Convert.ToInt32(record["UserCreate"]);
//    if (!Convert.IsDBNull(record["BookId"])) series.BookId = Convert.ToInt32(record["BookId"]);
//    if (!Convert.IsDBNull(record["BookName"])) series.BookName = Convert.ToString(record["BookName"]);
//    if (!Convert.IsDBNull(record["BookName"])) series.KindBookName = Convert.ToString(record["KindBookName"]);
//    if (!Convert.IsDBNull(record["PublisherName"])) series.PublisherName = Convert.ToString(record["PublisherName"]);
//    if (!Convert.IsDBNull(record["PublisherId"])) series.PublisherId = Convert.ToInt32(record["PublisherId"]);
//    if (!Convert.IsDBNull(record["KindBookId"])) series.KindBookId = Convert.ToInt32(record["KindBookId"]);
//    return series;
//}