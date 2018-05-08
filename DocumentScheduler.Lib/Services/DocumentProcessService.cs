using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DocumentScheduler.Lib.Core.Interface;
using DocumentScheduler.Lib.Services.Interface;
using DocumentScheduler.Lib.ViewModel;

namespace DocumentScheduler.Lib.Services
{
    public class DocumentSchedulerService : IDocumentSchedulerService
    {
        private readonly IDocumentScheduler _docScheduler;
        public DocumentSchedulerService(IDocumentScheduler docProcessor)
        {
            _docScheduler = docProcessor;
        }

        public DocumentViewModel GetDocument(string id)
        {
            return _docScheduler.GetDocumentByDocId(id);
        }

        public async  Task QueueDocument(UserInputViewModel input)
        {
            await _docScheduler.QueueDocumentAsync(input);
        }
    }
}
