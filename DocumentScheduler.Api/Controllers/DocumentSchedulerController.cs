using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentScheduler.Lib;
using DocumentScheduler.Lib.Services.Interface;
using DocumentScheduler.Lib.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocumentScheduler.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/DocumentScheduler")]
    public class DocumentSchedulerController : Controller
    {
        private readonly IDocumentSchedulerService _docSchedulerService;
        public DocumentSchedulerController(IDocumentSchedulerService docSchedulerService)
        {
            _docSchedulerService = docSchedulerService;
        }
        // GET: api/DocumentScheduler
        [HttpGet]
        public IEnumerable<DocumentViewModel> Get()
        {
            return new List<DocumentViewModel>();
        }

        // GET: api/DocumentScheduler/5
        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();
            //Note: Ideally I use Auto mapper library to map Domain to View Model, Didn't 
            //Implement auto mapper to stick on task
            var docList = _docSchedulerService.GetDocument(id);
            if (docList is null)
                return NotFound();
            return Ok(new DocumentViewModel());
        }

        // POST: api/DocumentScheduler
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]UserInputViewModel input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _docSchedulerService.QueueDocument(input);

            return Ok();

        }

        // PUT: api/DocumentScheduler/5
        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody]DocumentViewModel document)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated =_docSchedulerService.UpdateDocument(document);

            if (updated)
                return Ok();
            else
            {
                return BadRequest();
            }
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
        }
    }
}
