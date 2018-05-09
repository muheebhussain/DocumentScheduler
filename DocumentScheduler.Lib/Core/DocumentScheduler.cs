using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DocumentScheduler.Lib.Core.Interface;
using DocumentScheduler.Lib.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DocumentScheduler.Lib.Core
{
    public class DocumentScheduler : IDocumentScheduler
    {
        #region Private Properties

        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly int _maxServerLimit = 1;
        private readonly int _noOfCores = 4;

        //private readonly IRepository _repository;
        private LinkedList<DocumentViewModel> _docList;
        private List<ServerViewModel> _servers;
        #endregion

        #region Constructor
        //Inject repository in constructor to fetch SLA Tier from DB.
        //Note: I have not implemented Repository to focus on given task.
        public DocumentScheduler(
            ILogger<DocumentScheduler> logger,
            IConfiguration config
            //IRepository repository
            )
        {
            _logger = logger;
            _config = config;
            int.TryParse(_config["DocumentSchedulerSettings:MaxServerLimit"], out _maxServerLimit);
            int.TryParse(_config["DocumentSchedulerSettings:NoOfCores"], out _noOfCores);
            //_repository = repository

            if (_servers is null)
                _servers = new List<ServerViewModel>();
            if (_docList is null)
                _docList = new LinkedList<DocumentViewModel>();
        }
        #endregion


        /// <summary>
        /// Queues the file names to document list by generating new document for each filename.
        /// Step 1: Get SLA from DB by UserId
        /// Step 2: Get FinishBy Time to determine, when this document should be processed and upserted
        /// into the list to be processed.
        /// Step 3: Use Parallel Foreach to utilize all the cores of the system and complete task in the loop.
        /// Step 4: Generate New Document for given filename
        /// step 5: Place the newly generated document into the list based on finish by time.
        /// </summary>
        /// <param name="input">User input.</param>
        public async Task QueueDocumentAsync(UserInputViewModel input)
        {
            var slaTier = GetSLAByUserId(input.UserId);
            var finishBy = GetFinishByTime(slaTier);

            Parallel.ForEach(input.FileNames, fileName =>
            {
                try
                {
                    var newDoc = GetNewDocument(input.UserId, fileName, finishBy);

                    //If queue is empty, then add new document first
                    if (_docList.Count == 0)
                        _docList.AddFirst(newDoc);
                    else
                    {
                        var docLookup = _docList.FirstOrDefault(d => d.FinishBy <= finishBy);

                        if (docLookup is null)
                            _docList.AddFirst(newDoc);
                        else
                        {
                            var node = _docList.Find(docLookup);
                            _docList.AddAfter(node, newDoc);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical($"Failed to Queue Document. UserId: {input.UserId}, FileName: {fileName}, ErrorMessage: {ex.Message}");
                }
            });

            await ProcessDocumentsAsync();
        }

        /// <summary>
        /// Gets the document by document identifier to be processed by another process
        /// If the document is found by given id then mark it as is in process, so the
        /// same document is not processed by two or more different processors.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <returns>reutrns null if document is in process else returns Document</returns>
        public DocumentViewModel GetDocumentByDocId(string id)
        {
            var doc = _docList.FirstOrDefault(d => d.DocId == id &&
                                                  !d.IsCompleted &&
                                                  !d.IsInProcess);
            if (doc != null)
                _docList.Find(doc).Value.IsInProcess = true;

            return doc;
        }

        private async Task ProcessDocumentsAsync()
        {
            //Assuming max server limit is 4 and all servers have quad core processors

            int upRunningserverCount = 0;
            var serverToSpinUp = DetermineNoOfServersToSpinUp();
            for (upRunningserverCount = _servers.Count; upRunningserverCount < serverToSpinUp; upRunningserverCount++)
            {
                _servers.Add(await SpinUpNewServerAsync());
            }

            Parallel.ForEach(_servers, server =>
            {
                Parallel.ForEach(
                    _docList.Where(d =>
                        !d.IsCompleted &&
                        !d.IsInProcess)
                        .OrderBy(d => d.FinishBy),
                    doc =>
                    {
                        try
                        {
                            Task.Delay(TimeSpan.FromMinutes(1));
                            //Processing the document in this block
                            doc.IsInProcess = false;
                            doc.IsCompleted = true;
                            _logger.LogInformation($"Document # {doc.DocId} with User # {doc.UserId} processed successfully.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogCritical($"Failed to Process Document. UserId: {doc.UserId}, FileName: {doc.FileName}, ErrorMessage: {ex.Message}");
                        }
                    });
            });

            await SpinDownServerAsync();
        }

        public bool UpdateDocument(DocumentViewModel updatedDoc)
        {
            var document = _docList.FirstOrDefault(d => d.DocId == updatedDoc.DocId &&
                                                  d.IsInProcess);
            if (document != null)
            {
                var node = _docList.Find(document);
                if (node != null)
                {
                    node.Value.IsInProcess = false;
                    node.Value.IsCompleted = updatedDoc.IsCompleted;
                }
            }
            return !(document is null);
        }

        #region Helper Methods



        /// <summary>
        /// Creates new document by generating Guid, filename, queuedDate, finishBy
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="finishBy">The finish by.</param>
        /// <returns></returns>
        private DocumentViewModel GetNewDocument(string userId, string fileName, DateTime finishBy)
        {
            return new DocumentViewModel()
            {
                DocId = Guid.NewGuid().ToString(),
                UserId = userId,
                FileName = fileName,
                QueuedDate = DateTime.Now,
                FinishBy = finishBy,
                IsInProcess = false,
                IsCompleted = false
            };
        }

        private byte GetSLAByUserId(string userId)
        {
            //Get SLA From DB
            var slaTier = (byte)0;//_repository.GetSLAByUserId(userId);
            return slaTier;
        }

        /// <summary>
        /// This Helper method is used to get finishBy time of the document based on SLA
        /// Document will be inserted into processing list based on FinishBy time to guarantee
        /// document is processed on or before guaranteed SLA tier.
        /// </summary>
        /// <param name="sla">The sla.</param>
        /// <returns></returns>
        private DateTime GetFinishByTime(byte sla)
        {
            var finishBy = DateTime.Now;
            switch (sla)
            {
                case (byte)Enums.SLA.OneMin:
                    break;
                case (byte)Enums.SLA.TenMin:
                    finishBy.AddMinutes(9);
                    break;
                case (byte)Enums.SLA.OneHr:
                    finishBy.AddMinutes(59);
                    break;
                default:
                    finishBy = DateTime.MinValue;
                    break;
            }

            return finishBy;
        }

        /// <summary>
        /// Determines the no of servers to spin up.
        /// </summary>
        /// <returns></returns>
        private int DetermineNoOfServersToSpinUp()
        {
            var noOfDocs = _docList.Count(d => !d.IsCompleted &&
                                              !d.IsInProcess);
            //If noOfCores is less than noOfDocs we will only need 1 server, else divide noOfDocs by noOfCores to findout 
            var noOfDocForEachServer = noOfDocs <= _noOfCores ? 1 : noOfDocs / _noOfCores;

            if (noOfDocForEachServer <= _noOfCores)
                return noOfDocForEachServer;
            else
                return _maxServerLimit;
        }

        /// <summary>
        /// Spins up new server and returns the server configuration to process documents
        /// </summary>
        /// <returns></returns>
        private async Task<ServerViewModel> SpinUpNewServerAsync()
        {
            var server = new ServerViewModel();
            try
            {
                server = await Task.Run(() =>new ServerViewModel(){Status = ""});
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Failed to Spin Up Server. ServerId: {server?.ServerId}, Server Name: {server?.ServerName}, Error Message: {ex.Message}");
            }

            return server;
        }

        private Task SpinDownServerAsync()
        {
            foreach (var server in _servers)
            {
                if (server?.Status?.ToLower() == "idle")
                    ShutDownServerAsync(server);
            }

            return Task.FromResult(0);
        }

        private Task ShutDownServerAsync(ServerViewModel server)
        {
            //Shut Down Server
            return Task.FromResult(0);
        }
        #endregion
    }
}
