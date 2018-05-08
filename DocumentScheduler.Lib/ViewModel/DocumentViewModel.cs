using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentScheduler.Lib.ViewModel
{
    public class DocumentViewModel
    {
        public string UserId { get; set; }
        public string FileName { get; set; }
        public string DocId { get; set; }
        public DateTime QueuedDate { get; set; }
        public DateTime FinishBy { get; set; }
        public bool IsInProcess { get; set; }
        public bool IsCompleted { get; set; }
    }
}
