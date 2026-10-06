using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Mcp;
using Enterprise.RagService.Application.Models;
using Enterprise.RagService.Application.Services;
using Enterprise.RagService.Infrastructure.AzureOpenAI;
using Enterprise.RagService.Infrastructure.Mcp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.RagService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRagInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useDevelopmentEmbedding = false)
    {
        services.Configure<AzureOpenAIOptions>(
            configuration.GetSection("AzureOpenAI"));

        services.Configure<AzureOpenAIChatOptions>(
            configuration.GetSection("AzureOpenAI"));

        services.AddScoped<
            IIntentClassifier,
            DevelopmentIntentClassifier>();

        services.AddScoped<
            IMcpToolService,
            DevelopmentMcpToolService>();

        services.AddScoped<
            IHybridAiService,
            HybridAiService>();

        services.AddScoped<
            IMcpToolRegistry,
            DevelopmentMcpToolRegistry>();

        services.AddScoped<
            IHybridAnswerSynthesizer,
            HybridAnswerSynthesizer>();

        services.AddScoped<
            InventoryMcpTool>();

        services.AddScoped<
            OrderMcpTool>();

        services.AddScoped<
            IMcpToolArgumentExtractor,
            DevelopmentMcpToolArgumentExtractor>();

        services.AddScoped<
            IMcpToolArgumentValidator,
            DevelopmentMcpToolArgumentValidator>();

        services.AddScoped<
            IMcpAuthorizationService,
            DevelopmentMcpAuthorizationService>();

        services.AddScoped<
            IMcpFailureHandler,
            DevelopmentMcpFailureHandler>();

        services.AddScoped<
            IMcpAuditService,
            DevelopmentMcpAuditService>();

        services.AddScoped<
            InventoryMcpToolExecutor>();

        services.AddScoped<
            OrderMcpToolExecutor>();

        services.AddScoped<IMcpToolExecutor>(
            provider =>
                provider.GetRequiredService<
                    InventoryMcpToolExecutor>());

        services.AddScoped<IMcpToolExecutor>(
            provider =>
                provider.GetRequiredService<
                    OrderMcpToolExecutor>());

        services.AddScoped<
            DevelopmentMcpToolExecutorRegistry>();

        services.AddScoped<
            IMcpToolPermissionService,
            DevelopmentMcpPermissionService>();

        services.AddScoped<
            McpToolResilienceExecutor>();

        services.AddScoped<
            IMcpProtocolService,
            DevelopmentMcpProtocolService>();

        services.AddSingleton(
            configuration
                .GetSection("LocalEmbedding")
                .Get<LocalEmbeddingOptions>()
                ?? new LocalEmbeddingOptions());

        services.AddHttpClient<
            IEmbeddingService,
            LocalEmbeddingService>();

        var useDevelopmentLlm =
            configuration.GetValue<bool>(
                "Rag:UseDevelopmentLlm");

        if (useDevelopmentLlm)
        {
            services.AddScoped<
                ILlmService,
                DevelopmentLlmService>();
        }
        else
        {
            services.AddSingleton(
                configuration
                    .GetSection("LocalLlm")
                    .Get<LocalLlmOptions>()
                    ?? new LocalLlmOptions());

            services.AddHttpClient<
                ILlmService,
                LocalLlmService>();
        }

        return services;
    }
}