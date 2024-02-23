using LM.API.Services;
using LM.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private ILogger<DocumentController> _logger { get; set; }
        private readonly IDocumentService _documentervice;

        public DocumentController(ILogger<DocumentController> logger, IDocumentService documentervice)
        {
            _logger = logger;
            _documentervice = documentervice;
        }

        [HttpPost]
        [Route("UpdateBorrowOrder")]
        public async Task<IActionResult> UpdateBorrowOrder([FromBody] RequestModel request)
        {
            try
            {
                var response = await _documentervice.UpdateBorrowOrderAsync(request);

                if (response == null || response.StatusCode != 0) return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = response?.Message ?? "Vui lòng liên hệ IT để được hổ trợ."
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DocumentController", "UpdateBorrowOrder");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });

            }
        }

        [HttpPost]
        [Route("GetBorrowOrders")]
        public async Task<IActionResult> GetBorrowOrders([FromBody] SearchModel pSearchData)
        {
            try
            {
                var data = await _documentervice.GetBorrowOrdersAsync(pSearchData);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DocumentController", "GetBorrowOrders");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }
        }

        [HttpGet]
        [Route("GetDocById")]
        public async Task<IActionResult> GetDocById(string pVoucherNo)
        {
            try
            {
                var data = await _documentervice.GetDocumentById(pVoucherNo);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DocumentController", "GetDocById");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }

        }

        [HttpPost]
        [Route("ReturnBooks")]
        public async Task<IActionResult> ReturnBooks([FromBody] RequestModel request)
        {
            try
            {
                var response = await _documentervice.ReturnBooksAsync(request);

                if (response == null || response.StatusCode != 0) return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = response?.Message ?? "Vui lòng liên hệ IT để được hổ trợ."
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DocumentController", "UpdateBorrowOrder");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });

            }
        }

        [HttpGet]
        [Route("GetReportIndex")]
        public async Task<IActionResult> GetReportIndex()
        {
            try
            {
                var data = await _documentervice.GetReportIndexAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DocumentController", "GetReportIndex");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }

        }

        [HttpPost]
        [Route("CancleDocList")]
        public async Task<IActionResult> CancleDocList([FromBody] RequestModel request)
        {
            try
            {
                var response = await _documentervice.CancleDocList(request);

                if (response == null || response.StatusCode != 0) return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = response?.Message ?? "Vui lòng liên hệ IT để được hổ trợ."
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DocumentController", "CancleDocList");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });

            }
        }

        [HttpGet]
        [Route("GetDocumentByStaff")]
        public async Task<IActionResult> GetDocumentByStaff(string pStaffCode)
        {
            try
            {
                var data = await _documentervice.GetDocumentByStaffAsync(pStaffCode);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DocumentController", "GetDocumentByStaff");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }

        }

        [HttpPost]
        [Route("GetReport")]
        public async Task<IActionResult> GetDataReport(RequestReportModel pSearchData)
        {
            try
            {
                var data = await _documentervice.GetReportAsync(pSearchData);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DocumentController", "GetDataReport");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }

        }

        [HttpGet]
        [Route("GetRevenueReport")]
        public async Task<IActionResult> GetRevenueReport(int pYear)
        {
            try
            {
                var data = await _documentervice.GetRevenueReportAsync(pYear);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DocumentController", "GetRevenueReport");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }

        }
    }
}
