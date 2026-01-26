# Copilot directives

## Directives to be applied ONLY when implementing NEW features

### Project structure
- Service Layer is the "core" of our implementation. It contains Models, Interfaces of Services, Interfaces of Repositories, Domain Exceptions and whatever the Service implementation may need.
- Data is accessed through Repositories (or ExternalServices when it comes to interact with external API instead of a database) defined in the Data Layer.
- The repositories use entities belonging to Service Layer and NEVER their own private data entities mapping physical data representation (eg: tables, collections, or jsons returned by remote API).
- The repositories throw Domain Exceptions only.
- Use AutoMapper to map between Entities and Models, placing its Profiles files into the outermost project in the hierarchy (Controllers Layer) and register them in Program.cs file.


### Code Consistency
- Methods belonging to the Service Layer NEVER handle Domain Exceptions coming from underlying architecture layers: they are meant to bubble up to the Controllers Layer.
- When a method belonging to the Service Layer needs to raise a business error, it MUST use Domain Exceptions.


### Logging Rules
- NEVER use LogError within the Service Layer: logging errors (with their stack trace) is demandated to Controllers Layer which handles Domain Exceptions.
- PUBLIC Methods belonging to the Service Layer MUST have at least one LogInformation line describing what happened, even on data read methods.
- PUBLIC Methods belonging to the Service Layer MAY use LogDebug for additional detailed logging, even multiple times within the same method when needed.
- Controllers handle Domain Exceptions and the unhandled error case (Exception).
- Controllers use LogWarning to describe Domain Exceptions without logging the stack trace.
- Controllers use LogError to describe unhandled exceptions (Exception), this time logging also the stack trace.
- Allow ONLY LogDebug for any other Layer (eg: Data, Infrastructure, etc.), regardless method visibility scope.

