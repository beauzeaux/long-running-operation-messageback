// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class EchoBot : ActivityHandler
    {
        private static readonly string LroText = "START_LRO_OPERATION";
        
        private readonly LongRunningOperationService backgroundService;

        public EchoBot(LongRunningOperationService backgroundService) {
            this.backgroundService = backgroundService;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Text.Equals(LroText, System.StringComparison.Ordinal)) {
                // Send a placeholder message
                var activity = await turnContext.SendActivityAsync(MessageFactory.Text("One moment..."), cancellationToken);
                // Create a conversation reference so the long running operation knows where to update when it completes
                // Note this is completely serializable information
                var conversationReference = new ConversationReference {
                    Conversation = turnContext.Activity.Conversation,
                    Bot = turnContext.Activity.Recipient, // Assume message was sent to the bot
                    ActivityId = activity.Id,
                    ServiceUrl = turnContext.Activity.ServiceUrl
                };

                // Pass off the conversation reference to the background service that will handle the 
                // long running operation.
                await this.backgroundService.AddLongRunningOperationAsync(conversationReference);
            } else {
                var heroCard = new HeroCard
                {
                    Title = "Long Running Operation Card",
                    Subtitle = $"Echo: {turnContext.Activity.Text}",
                    Text = "This is a card sent from the bot",
                    // Default image from botframework
                    Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                    Buttons = new List<CardAction> { new CardAction {
                        Type = ActionTypes.MessageBack,
                        Title = "Start long-running operation",
                        DisplayText = "",
                        Text = LroText,
                        Value = ""
                    } },
                };

                await turnContext.SendActivityAsync(MessageFactory.Attachment(heroCard.ToAttachment()), cancellationToken);
            }
           
            // await Task.Delay(5000);
            // var updatedActivity = MessageFactory.Text($"Echo (updated): {string.Concat(turnContext.Activity.Text.Split().Reverse())}");
            // // Make sure to modify the existing activity by setting the id
            // updatedActivity.Id = activity.Id;
            // await turnContext.UpdateActivityAsync(updatedActivity, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
