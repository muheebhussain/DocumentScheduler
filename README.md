# DocumentScheduler
A Demo/Prototype to Implement Document Scheduler

Requirements:

Users can add any number of new documents to be processed at any time
Users are in 3 "tiers": 10 minute SLA, 1 hour SLA, best effort
The scheduler takes incoming job requests with the UserID (string) and an array of filenames (string)
You assign each document a unique DocId Guid identifier
You can spin up as many new servers as needed to make sure we meet our SLAs, up to some limit
The scheduler is also called by "processors" on each of the pool of servers to grab a DocId to work on, and to later mark it as complete.

I made following additional assumptions

The solution in C#, Implemented the core logic in a .net standard class library and wrap it in a very simple .NET Core, self-hosted REST API.
Used fake stub function, When spinning up or down servers.
You don't need to actually receive the documents, just the filenames
All documents take 1 minute to process
All the servers are quad core, and max limit for servers are 4.(Values are assigned in AppSettings.config, can be changes if needed).
Didnt implement Unit Test, to just focus on implementation of requriements.

Implementation:

Core Login is in Class Library in Document Scheduler.cs //DocumentScheduler.Lib.Core


