using Elastic.Clients.Elasticsearch;
using Enterprise.RagService.API.Security;
using Enterprise.RagService.Application.Identity;
using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Mcp;
using Enterprise.RagService.Application.Models;
using Enterprise.RagService.Application.Services;
using Enterprise.RagService.Application.Security;
using Enterprise.RagService.Domain.Entities;
using Enterprise.RagService.Infrastructure;
using Enterprise.RagService.Infrastructure.Mcp;
using Enterprise.RagService.Infrastructure.Security;
using Enterprise.RagService.Persistence.Repositories;
using Enterprise.RagService.API;

var builder = WebApplication.CreateBuilder(args);

var elasticsearchUrl =
    builder.Configuration["Elasticsearch:Url"]
    ?? "http://localhost:9200";

builder.Services.AddSingleton(
    new ElasticsearchClient(
        new ElasticsearchClientSettings(
            new Uri(elasticsearchUrl))));

builder.Services.AddScoped<
    IRagDocumentRepository,
    ElasticsearchRagDocumentRepository>();

builder.Services.AddScoped<
    IRagChunkRepository,
    ElasticsearchRagChunkRepository>();

builder.Services.AddScoped<
    IDocumentChunkingService,
    DocumentChunkingService>();

builder.Services.AddScoped<
    IRagContextBuilder,
    RagContextBuilder>();

builder.Services.AddScoped<
    IRagPromptBuilder,
    RagPromptBuilder>();

builder.Services.AddScoped<
    IRagIngestionService,
    RagIngestionService>();

builder.Services.AddScoped<
    IRagReindexingService,
    ElasticsearchRagReindexingService>();

builder.Services.AddScoped<
    IRagIndexLifecycleService,
    ElasticsearchRagIndexLifecycleService>();

builder.Services.AddScoped<
    IRagVectorSearchRepository,
    ElasticsearchRagVectorSearchRepository>();

builder.Services.AddScoped<
    IRagKeywordSearchRepository,
    ElasticsearchRagKeywordSearchRepository>();

builder.Services.AddScoped<
    RagKeywordSearchService>();

builder.Services.AddScoped<
    IRagHybridSearchService,
    RagHybridSearchService>();

builder.Services.AddScoped<
    IRagReranker,
    DevelopmentReranker>();

builder.Services.AddScoped<
    IRagRetrievalWithFallbackService,
    RagRetrievalWithFallbackService>();

builder.Services.AddScoped<
    IRagRetrievalEvaluator,
    RagRetrievalEvaluator>();

builder.Services.AddScoped<
    IRagRelevanceService,
    RagRelevanceService>();

builder.Services.AddScoped<
    IRagRetrievalService,
    RagRetrievalService>();

builder.Services.AddScoped<
    IRagAnswerService,
    RagAnswerService>();

builder.Services.AddScoped<
    IAiOrchestrator,
    AiOrchestrator>();

builder.Services.AddScoped<
    IRagRetrievalGuard,
    RagRetrievalGuard>();

builder.Services.AddSingleton(
    builder.Configuration
        .GetSection("RagRetrievalFallback")
        .Get<RagRetrievalFallbackOptions>()
        ?? new RagRetrievalFallbackOptions());

builder.Services.Configure<RagRetrievalOptions>(
    builder.Configuration.GetSection("RagRetrieval"));

builder.Services.AddSingleton(
    builder.Configuration
        .GetSection("RagRetrieval")
        .Get<RagRetrievalOptions>()
        ?? new RagRetrievalOptions());

builder.Services.AddSingleton(
    builder.Configuration
        .GetSection("RagHybridSearch")
        .Get<RagHybridSearchOptions>()
        ?? new RagHybridSearchOptions());

builder.Services.AddRagInfrastructure(
    builder.Configuration,
    useDevelopmentEmbedding: true);

builder.Services.AddEnterpriseJwtAuthentication(
    builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet(
    "/",
    () => "RAG Service is running");

app.MapPost(
    "/api/rag/dev/token",
    (
        DevTokenRequest request,
        JwtTokenService tokenService) =>
    {
        if (string.IsNullOrWhiteSpace(request.UserId) ||
            string.IsNullOrWhiteSpace(request.TenantId))
        {
            return Results.BadRequest(
                new
                {
                    error =
                        "UserId and TenantId are required."
                });
        }

        var roles =
            request.Roles?.Count > 0
                ? request.Roles
                : new List<string>
                {
                    SecurityRoles.Viewer
                };

        var token =
            tokenService.CreateToken(
                request.UserId,
                request.TenantId,
                roles);

        return Results.Ok(
            new
            {
                accessToken = token,
                tokenType = "Bearer"
            });
    })
    .AllowAnonymous();

app.MapPost(
    "/api/rag/documents",
    async (
        RagDocumentRequest request,
        IUserIdentity identity,
        IRagIngestionService ingestionService,
        CancellationToken cancellationToken) =>
    {
        if (!identity.IsAuthenticated)
        {
            return Results.Unauthorized();
        }

        if (!ValidateTenant(
                identity,
                request.TenantId))
        {
            return Results.Forbid();
        }

        var document =
            new RagDocument
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                Source = request.Source,
                TenantId = request.TenantId
            };

        await ingestionService.IngestAsync(
            document,
            cancellationToken);

        return Results.Created(
            $"/api/rag/documents/{document.Id}",
            document);
    })
    .RequireAuthorization(
        SecurityPolicies.RagUser);

app.MapGet(
    "/api/rag/search",
    async (
        string query,
        string tenantId,
        int topK,
        IUserIdentity identity,
        IRagRetrievalService retrievalService,
        CancellationToken cancellationToken,
        string? source = null) =>
    {
        if (!identity.IsAuthenticated)
        {
            return Results.Unauthorized();
        }

        if (!ValidateTenant(
                identity,
                tenantId))
        {
            return Results.Forbid();
        }

        var filter =
            new RagSearchFilter
            {
                Source = source
            };

        var results =
            await retrievalService.SearchAsync(
                query,
                tenantId,
                filter: filter,
                topK: topK,
                cancellationToken: cancellationToken);

        return Results.Ok(results);
    })
    .RequireAuthorization(
        SecurityPolicies.RagUser);

app.MapGet(
    "/api/rag/context",
    async (
        string query,
        string tenantId,
        int topK,
        IUserIdentity identity,
        IRagRetrievalService retrievalService,
        IRagContextBuilder contextBuilder,
        CancellationToken cancellationToken,
        string? source = null) =>
    {
        if (!identity.IsAuthenticated)
        {
            return Results.Unauthorized();
        }

        if (!ValidateTenant(
                identity,
                tenantId))
        {
            return Results.Forbid();
        }

        var filter =
            new RagSearchFilter
            {
                Source = source
            };

        var results =
            await retrievalService.SearchAsync(
                query,
                tenantId,
                filter: filter,
                topK: topK,
                cancellationToken: cancellationToken);

        var context =
            contextBuilder.Build(results);

        return Results.Ok(context);
    })
    .RequireAuthorization(
        SecurityPolicies.RagUser);

app.MapGet(
    "/api/rag/ask",
    async (
        string? query,
        string? tenantId,
        int topK,
        IUserIdentity identity,
        IAiOrchestrator orchestrator,
        CancellationToken cancellationToken) =>
    {
        if (!identity.IsAuthenticated)
        {
            return Results.Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            return Results.BadRequest(
                new
                {
                    error = "Query cannot be empty."
                });
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Results.BadRequest(
                new
                {
                    error = "TenantId cannot be empty."
                });
        }

        if (!ValidateTenant(
                identity,
                tenantId))
        {
            return Results.Forbid();
        }

        if (topK <= 0)
        {
            return Results.BadRequest(
                new
                {
                    error =
                        "TopK must be greater than zero."
                });
        }

        var request =
            new AiOrchestratorRequest
            {
                Query = query,
                TenantId = tenantId,
                TopK = topK
            };

        var response =
            await orchestrator.ExecuteAsync(
                request,
                cancellationToken);

        return Results.Ok(response);
    })
    .RequireAuthorization(
        SecurityPolicies.RagUser);

app.MapGet(
    "/api/rag/keyword-search",
    async (
        string query,
        string tenantId,
        int topK,
        IUserIdentity identity,
        RagKeywordSearchService keywordSearchService,
        CancellationToken cancellationToken,
        string? source = null) =>
    {
        if (!identity.IsAuthenticated)
        {
            return Results.Unauthorized();
        }

        if (!ValidateTenant(
                identity,
                tenantId))
        {
            return Results.Forbid();
        }

        var filter =
            new RagSearchFilter
            {
                Source = source
            };

        var results =
            await keywordSearchService.SearchAsync(
                query,
                tenantId,
                filter,
                topK,
                cancellationToken);

        return Results.Ok(results);
    })
    .RequireAuthorization(
        SecurityPolicies.RagUser);

app.MapGet(
    "/api/rag/hybrid-search",
    async (
        string query,
        string tenantId,
        int topK,
        IUserIdentity identity,
        IRagHybridSearchService hybridSearchService,
        CancellationToken cancellationToken,
        string? source = null) =>
    {
        if (!identity.IsAuthenticated)
        {
            return Results.Unauthorized();
        }

        if (!ValidateTenant(
                identity,
                tenantId))
        {
            return Results.Forbid();
        }

        var filter =
            new RagSearchFilter
            {
                Source = source
            };

        var results =
            await hybridSearchService.SearchAsync(
                query,
                tenantId,
                filter,
                topK,
                cancellationToken);

        return Results.Ok(results);
    })
    .RequireAuthorization(
        SecurityPolicies.RagUser);

app.MapPost(
    "/api/rag/evaluation/run",
    async (
        RagEvaluationRequest request,
        IUserIdentity identity,
        IRagRetrievalEvaluator evaluator,
        CancellationToken cancellationToken) =>
    {
        if (!identity.IsAuthenticated)
        {
            return Results.Unauthorized();
        }

        if (request.Cases is null ||
            request.Cases.Count == 0)
        {
            return Results.BadRequest(
                new
                {
                    error =
                        "At least one evaluation case is required."
                });
        }

        if (request.Cases.Any(
                item =>
                    !ValidateTenant(
                        identity,
                        item.TenantId)))
        {
            return Results.Forbid();
        }

        if (request.TopK <= 0)
        {
            return Results.BadRequest(
                new
                {
                    error =
                        "TopK must be greater than zero."
                });
        }

        var evaluationCases =
            request.Cases
                .Select(
                    item =>
                        new RagEvaluationCase
                        {
                            Name = item.Name,
                            Query = item.Query,
                            TenantId = item.TenantId,
                            Source = item.Source,
                            ExpectedChunkIds =
                                item.ExpectedChunkIds,
                            ForbiddenChunkIds =
                                item.ForbiddenChunkIds
                        })
                .ToList();

        var summary =
            await evaluator.EvaluateAsync(
                evaluationCases,
                request.TopK,
                cancellationToken);

        return Results.Ok(summary);
    })
    .RequireAuthorization(
        SecurityPolicies.RagUser);

app.MapPost(
    "/api/rag/admin/reindex",
    async (
        IUserIdentity identity,
        IRagReindexingService reindexingService,
        CancellationToken cancellationToken) =>
    {
        var count =
            await reindexingService.ReindexAsync(
                cancellationToken);

        return Results.Ok(
            new
            {
                message =
                    "RAG chunks reindexed successfully.",
                count
            });
    })
    .RequireAuthorization(
        SecurityPolicies.McpAdmin);

app.MapPost(
    "/api/rag/admin/cleanup-indexes",
    async (
        IUserIdentity identity,
        IRagIndexLifecycleService lifecycleService,
        CancellationToken cancellationToken) =>
    {
        var count =
            await lifecycleService
                .CleanupOldIndexesAsync(
                    cancellationToken);

        return Results.Ok(
            new
            {
                message =
                    "Old RAG indexes cleaned up successfully.",
                deletedCount = count
            });
    })
    .RequireAuthorization(
        SecurityPolicies.McpAdmin);

app.MapPost(
    "/api/rag/mcp/tools/call",
    async (
        McpProtocolRequest request,
        IMcpProtocolService protocolService,
        CancellationToken cancellationToken) =>
    {
        var result =
            await protocolService.InvokeAsync(
    request,
    cancellationToken);

        return Results.Ok(result);
    })
    .RequireAuthorization(
        SecurityPolicies.McpUser);

app.MapPost(
    "/api/rag/admin/mcp-test",
    async (
        McpToolRequest request,
        IUserIdentity identity,
        IMcpToolService service,
        CancellationToken cancellationToken) =>
    {
        if (!ValidateTenant(
                identity,
                request.TenantId))
        {
            return Results.Forbid();
        }

        var result =
    await service.ExecuteAsync(
        request,
        cancellationToken);

        return Results.Ok(result);
    })
    .RequireAuthorization(
        SecurityPolicies.McpAdmin);

static bool ValidateTenant(
    IUserIdentity identity,
    string tenantId)
{
    return identity.IsAuthenticated &&
           !string.IsNullOrWhiteSpace(
               identity.TenantId) &&
           !string.IsNullOrWhiteSpace(
               tenantId) &&
           string.Equals(
               identity.TenantId,
               tenantId,
               StringComparison.OrdinalIgnoreCase);
}

app.Run();

public sealed class DevTokenRequest
{
    public string UserId { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = [];
}