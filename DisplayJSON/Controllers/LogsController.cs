using Microsoft.AspNetCore.Mvc;
using DisplayJSON.Utlis;
using DisplayJSON.Serializers;
using System.Text.Json.Nodes;
using JsonFlatten;
using DisplayJSON.ViewModels;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace DisplayJSON.Controllers
{
    public class LogsController : Controller
    {
        private readonly ILogger<LogsController> _logger;
        private List<Log>? logs;

        public LogsController(ILogger<LogsController> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            var fileContent = HttpContext.Session.GetString("fileContent");
            if (!string.IsNullOrEmpty(fileContent))
            {
                logs = new List<Log>();
                int guid = 1;
                string[] jsonStrings = fileContent!.Split("\r\n");
                foreach (string jsonStr in jsonStrings)
                {
                    var log = Deserializer.DeserializeLog(jsonStr);
                    log!.guid = guid;
                    logs!.Add(log);
                    ++guid;
                }
            }
        }

        public IActionResult Index()
        {
            // If no logs, then redirect to upload log file page
            if (logs == null) return RedirectToAction("Index", "Home");

            var fileName = HttpContext.Session.GetString("fileName");
            if (!string.IsNullOrEmpty(fileName)) ViewData["fileName"] = fileName;

            return View(logs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upload([Bind("logFile")] LogViewModel logViewModel)
        {
            if (ModelState.IsValid)
            {
                HttpContext.Session.SetString("fileName", logViewModel!.logFile!.FileName);
                try
                {
                    using (var sr = new StreamReader(logViewModel.logFile!.OpenReadStream()))
                    {
                        string fileContent = sr.ReadToEnd().Trim();
                        HttpContext.Session.SetString("fileContent", fileContent);
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                }
                return RedirectToAction(nameof(Index));
            }

            var fileName = HttpContext.Session.GetString("fileName");
            if (!string.IsNullOrEmpty(fileName)) ViewData["fileName"] = fileName;

            return View("~/Views/Home/Index.cshtml", logViewModel);
        }

        public IActionResult GetLogData(int? guid)
        {
            if (guid == null)
            {
                return BadRequest();
            }

            // if no logs, then redirect to upload log file page
            if (logs == null) return RedirectToAction("Index", "Home");

            foreach (var log in logs!)
            {
                if (log.guid == guid)
                {
                    if (log.Properties!.MethodName!.Contains("BillDesk"))
                    {
                        string billDeskPayload = log.Properties!.LogData!.Split(".")[1].ToString();
                        string billDeskPayloadDecoded = Decoder.Base64UrlDecode(billDeskPayload);

                        var jObj = Newtonsoft.Json.Linq.JObject.Parse(billDeskPayloadDecoded);
                        Dictionary<string, object> flattened = new Dictionary<string, object>(jObj.Flatten());
                        return Json(flattened);
                    }
                    else
                    {
                        try
                        {
                            var jObj = Newtonsoft.Json.Linq.JObject.Parse(log.Properties!.LogData!);
                            Dictionary<string, object> flattened = new Dictionary<string, object>(jObj.Flatten());
                            return Json(flattened);
                        }
                        catch (Newtonsoft.Json.JsonReaderException)
                        {
                            var jsonNode = JsonNode.Parse(log.Properties!.LogData!);

                            _logger.LogInformation(jsonNode!.ToString());

                            try
                            {
                                var jObj = Newtonsoft.Json.Linq.JObject.Parse(jsonNode!.ToString());
                                Dictionary<string, object> flattened = new Dictionary<string, object>(jObj.Flatten());
                                return Json(flattened);
                            }
                            catch (Newtonsoft.Json.JsonReaderException)
                            {
                                return Json(new { LogData = log.Properties!.LogData });
                            }
                        }
                        catch (JsonException)
                        {
                            return Json(new { LogData = log.Properties!.LogData });
                        }
                    }
                }
            }
            return NotFound();
        }
    }
}
