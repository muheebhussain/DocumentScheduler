using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DocumentScheduler.Lib.ViewModel;

namespace DocumentScheduler.Lib.Services.Interface
{
    public interface IDocumentSchedulerService
    {
        Task QueueDocument(UserInputViewModel input);
        DocumentViewModel GetDocument(string id);
    }
}
