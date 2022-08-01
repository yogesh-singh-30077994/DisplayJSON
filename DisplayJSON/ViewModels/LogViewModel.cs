using DisplayJSON.Serializers;
using System.ComponentModel.DataAnnotations;

namespace DisplayJSON.ViewModels
{
    public class LogViewModel
    {
        [Display(Name = "Upload Log File")]
        [Required]
        public IFormFile? logFile { get; set; }
    }
}
