using System.Threading.Tasks;
using DocumentScheduler.Lib.ViewModel;

namespace DocumentScheduler.Lib.Core.Interface
{
    public interface IDocumentScheduler
    {
        Task QueueDocumentAsync(UserInputViewModel input);

        DocumentViewModel GetDocumentByDocId(string id);

        bool UpdateDocument(DocumentViewModel doc);
    }
}
