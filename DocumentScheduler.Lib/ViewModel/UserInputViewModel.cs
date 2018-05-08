using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace DocumentScheduler.Lib.ViewModel
{
    public class UserInputViewModel
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string[] FileNames { get; set; }
    }

    
}
