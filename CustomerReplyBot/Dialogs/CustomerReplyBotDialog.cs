#define UseSearch

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Diagnostics;
using AzureSearchBot.Model;
using CustomerReplyBot.Services;
using System.Web.Configuration;
using System.Linq;
using Luis.Model;

namespace CustomerReplyBot.Dialogs
{
    [Serializable]
    public class CustomerReplyDialog : IDialog<object>
    {
#if UseSearch
        private readonly AzureSearchService searchService = new AzureSearchService();
#else
        private readonly LuisService luisService = new LuisService();
#endif
        private IDictionary<string, string> questionDictionary = new Dictionary<string, string>();

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(AskQuestion);
        }
        public virtual async Task AskQuestion(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            await context.PostAsync(Resources.Messages.AskQuestionMessage);
            context.Wait(AskCandidateQuetions);
        }

        public virtual async Task AskCandidateQuetions(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            string inputText = message.Text;
#if UseSearch
            try
            {
                SearchResult searchResults = await searchService.SearchByName(inputText,4);
                if (searchResults.value.Length != 0)
                {
                    foreach (Value searchResult in searchResults.value)
                    {
                        questionDictionary.Add(searchResult.question + " |  類似率 score :" + searchResult.searchScore.ToString(), searchResult.answer);
                    }
                    PromptDialog.Choice(context, this.ShowAnswer, questionDictionary.Keys, "類似質問リストの中から該当する質問をお選びください。");
                }
                else
                {
                    await context.PostAsync($"No questions found by {message.Text} ");
                }
            }
#else
            try
            {
                LuisResult luisResult = await luisService.GetResponse(inputText);
                if (luisResult.intents.Length != 0)
                {
                    foreach (Intent intent in luisResult.intents)
                    {
                        //ここで本来はDBにintentをkeyに問い合わせる
                        questionDictionary.Add(intent.intent + " |  類似率 score :" + intent.score.ToString(), intent.intent);
                    }
                    PromptDialog.Choice(context, this.ShowAnswer, questionDictionary.Keys, "類似質問リストの中から該当する質問をお選びください。");
                }
                else
                {
                    await context.PostAsync($"No questions found by {message.Text} ");
                }
            }
#endif
            catch (Exception e)
            {
                Debug.WriteLine($"Error when searching for good question candidates: {e.Message}");
            }

        }

        public virtual async Task ShowAnswer(IDialogContext context, IAwaitable<string> result)
        {
            var optionSelected = await result;
            string answer = "";
            if (questionDictionary.ContainsKey(optionSelected))
            {
                answer = questionDictionary[optionSelected];
            }
            await context.PostAsync(answer);
            questionDictionary.Clear();

            PromptDialog.Choice(context, ResumeAfterselectoin, new List<string>() { Resources.Messages.YesOption, Resources.Messages.NoOption }, "終了いたしますか？");
        }

        private async Task ResumeAfterselectoin(IDialogContext context, IAwaitable<string> result)
        {
            string selectedString = await result;
            if(selectedString == Resources.Messages.YesOption)
            {
                context.Done<object>(null);
            }
            else
            {
                await context.PostAsync(Resources.Messages.AskQuestionMessage);
                context.Wait(AskCandidateQuetions);
            }
        }
    }
}