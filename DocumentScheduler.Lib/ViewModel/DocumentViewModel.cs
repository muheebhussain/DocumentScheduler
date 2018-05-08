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
        /// <summary>
        /// DateTime the document that was added to document list that needs to be processed
        /// </summary>
        public DateTime QueuedDate { get; set; }
        /// <summary>
        /// Finish By determines this document should be processed no later than finish by time
        /// This is determined by SLA.
        /// </summary>
        public DateTime FinishBy { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is in process.
        /// If the document is in process, this document will not be picked by
        /// any other process.
        /// </summary>
        public bool IsInProcess { get; set; }
        public bool IsCompleted { get; set; }
    }
}
