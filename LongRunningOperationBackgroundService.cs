// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public sealed class LongRunningOperationService
    {
        private readonly ILogger<LongRunningOperationService> logger;
        private readonly BotAdapter botAdapter;
        private readonly ConcurrentDictionary<string, Task> operations = new ConcurrentDictionary<string, Task>();
        // Don't do this in production applications, this is just for demonstration purposes
        private readonly string botId;

        public LongRunningOperationService(IConfiguration configuration, ILogger<LongRunningOperationService> logger, BotAdapter botAdapter) {
            this.logger = logger;
            this.botAdapter = botAdapter;
            this.botId = configuration.GetValue<string>("MicrosoftAppId");
        }

        // Make signature async in case setup is async operations
        public Task AddLongRunningOperationAsync(ConversationReference conversationReference) {
            this.operations.TryAdd(conversationReference.ActivityId, this.StartLongRunningOperation(conversationReference));
            return Task.CompletedTask;
        }

        private async Task StartLongRunningOperation(ConversationReference conversationReference) {
            await Task.Delay(5000);
            this.logger.LogInformation("Task is complete {0} / {1}", conversationReference.Bot.Id, conversationReference.ActivityId);
            await this.botAdapter.ContinueConversationAsync(this.botId, conversationReference, async (ITurnContext continueTurnContext, CancellationToken continueCancellationToken) => {
                this.logger.LogInformation("Sending result");
                var completedActivity = MessageFactory.Text("Completed operation");
                completedActivity.Id = conversationReference.ActivityId;
                await continueTurnContext.UpdateActivityAsync(completedActivity);
            }, CancellationToken.None);
        }
    }
}
