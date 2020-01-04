using MongoDB.Bson;
using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using vozCompetitionsLibrary;
using vozCompetitionsLibrary.Model;

namespace vozCompetitions
{
    class Program
    {
        static ITelegramBotClient vozCompetitionsBot;

        static void Main()
        {
            vozCompetitionsBot = new TelegramBotClient(Configuration.API());

            vozCompetitionsBot.OnMessage += Bot_OnMessage;
            vozCompetitionsBot.OnMessageEdited += Bot_OnMessage;
            vozCompetitionsBot.OnCallbackQuery += Bot_OnCallbackQuery;

            vozCompetitionsBot.StartReceiving();

            var me = vozCompetitionsBot.GetMeAsync().Result;
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

            Thread.Sleep(int.MaxValue);
            vozCompetitionsBot.StopReceiving();
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs messageEvent)
        {
            DataAccess database = new DataAccess("vozCompetitionsDB");

            var message = messageEvent.Message;
            if (message == null) return;

            // ADD NEW COMPETITION
            // Command format: /newcompetition #hashtag Competition name
            if (message.Text != null)
                if (message.Text.Split(' ')[0].Trim() == "/newcompetition")
                    try
                    {
                        //if (AccessCompetition.Exists(message.Text.Split(' ', 3)[1].Trim()))
                        string hashtag = message.Text.Split(' ', 3)[1].Trim();
                        if (database.GetList<Competition>("Competitions").Exists(x => x.Hashtag == hashtag))
                        {
                            await vozCompetitionsBot.SendTextMessageAsync(
                                message.Chat,
                                $"Hashtag `{message.Text.Split(' ', 3)[1].Trim()}` already exists. Please choose another one.",
                                ParseMode.Markdown,
                                replyToMessageId: message.MessageId
                            );
                        }
                        else
                        {
                            Competition competition = new Competition()
                            {
                                UserId = message.From.Id,
                                ChatId = message.Chat.Id,
                                Hashtag = message.Text.Split(' ', 3)[1].Trim(),
                                Name = message.Text.Split(' ', 3)[2].Trim(),
                                Status = "Opening"
                            };

                            database.InsertRecord<Competition>("Competitions", competition);

                            await vozCompetitionsBot.SendTextMessageAsync(
                                message.Chat,
                                $"Cuộc thi: *{competition.Name}*\nHashtag: `{competition.Hashtag}`\n\nBẮT ĐẦU!!!",
                                ParseMode.Markdown,
                                replyToMessageId: message.MessageId
                            );
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        await vozCompetitionsBot.SendTextMessageAsync(
                            message.Chat,
                            "Wrong format! Use this format to create a new competition:\n`/newcompetition #hashtag Name of the cometition`",
                            ParseMode.Markdown,
                            replyToMessageId: message.MessageId
                        );
                    }


            // LIST ALL COMPETITIONS
            // Command: /listcompetition
            if (message.Text != null)
                if (message.Text == "/listcompetition")
                {
                    string listCompetitions = "COMPETITION LIST\n\n";
                    int count = 0;
                    foreach (Competition competition in database.GetList<Competition>("Competitions"))
                        if (competition.ChatId == message.Chat.Id)
                        {
                            count++;
                            listCompetitions += $"[{count}] {competition.Hashtag} - {competition.Name} - {competition.Status}\n\n";
                        }
                    if (count == 0)
                        listCompetitions += "There's no competition in this chat.";

                    await vozCompetitionsBot.SendTextMessageAsync(
                        message.Chat,
                        listCompetitions,
                        replyToMessageId: message.MessageId
                    );
                }


            // CLOSE COMPETITION - Only the owner can close his competition
            // Command format: /close #hashtag
            if (message.Text != null)
                if (message.Text.Split(' ')[0].Trim() == "/close")
                    try
                    {
                        string hashtag = message.Text.Split(' ')[1].Trim();
                        bool found = database.GetList<Competition>("Competitions").Exists(x => x.Hashtag == hashtag);
                        if (found)
                        {
                            var competitionRecord = database.GetList<Competition>("Competitions").Find(x => x.Hashtag == hashtag);
                            bool isOwner = (message.From.Id == competitionRecord.UserId);
                            if (isOwner)
                            {
                                competitionRecord.Status = "Closed";
                                database.UpdateRecordById<Competition>("Competitions", competitionRecord.Id, competitionRecord);

                                await vozCompetitionsBot.SendTextMessageAsync(
                                    message.Chat,
                                    "Competition closed successfully!",
                                    replyToMessageId: message.MessageId
                                );
                            }
                            else
                            {
                                await vozCompetitionsBot.SendTextMessageAsync(
                                    message.Chat,
                                    "You're not the owner of this competition!",
                                    replyToMessageId: message.MessageId
                            );
                            }
                        }
                        else
                        {
                            await vozCompetitionsBot.SendTextMessageAsync(
                                message.Chat,
                                "This competition doesn't exist!",
                                replyToMessageId: message.MessageId
                            );
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        await vozCompetitionsBot.SendTextMessageAsync(
                            message.Chat,
                            "Wrong format! Use this format close your competition:\n`/close #hashtag`",
                            ParseMode.Markdown,
                            replyToMessageId: message.MessageId
                        );
                    }


            // ADD NEW SUBMISSION
            // Photo with #hashtag
            if (message.Caption != null)
            {
                string hashtag = message.Caption.Split(' ')[0];
                if (database.GetList<Competition>("Competitions").Exists(x => x.Hashtag == hashtag))
                    if (database.GetList<Competition>("Competitions").Find(x => x.Hashtag == hashtag).Status == "Opening")
                    {
                        Submission submission = new Submission()
                        {
                            CompetitionHashtag = hashtag,
                            UserId = message.From.Id,
                            UserInfo = $"{message.From.FirstName} {message.From.LastName} @{message.From.Username}",
                            ChatId = message.Chat.Id,
                            MessageId = message.MessageId,
                            Point = 0
                        };

                        database.InsertRecord<Submission>("Submissions", submission);

                        await vozCompetitionsBot.SendTextMessageAsync(
                            message.Chat,
                            $"Vote for {message.From.FirstName} {message.From.LastName} @{message.From.Username}",
                            replyToMessageId: message.MessageId,
                            replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Vote - 0", hashtag))
                        );
                    }
                    else
                    {
                        await vozCompetitionsBot.SendTextMessageAsync(
                            message.Chat,
                            "This competition already ended.",
                            replyToMessageId: message.MessageId
                        );
                    }
            }


            // SHOW RESULT
            // Command format: /result #hashtag
            if (message.Text != null)
                if (message.Text.Split(' ')[0] == "/result")
                    try
                    {
                        string hashtag = message.Text.Split(' ')[1].Trim();
                        string listSubmissions = $"CURRENT RESULT - `{hashtag}`\n\n";
                        string chaturl;
                        if (message.Chat.Username != null)
                            chaturl = message.Chat.Username;
                        else chaturl = $"c/{- message.Chat.Id - 1000000000000}";
                        foreach (Submission submission in database.GetList<Submission>("Submissions"))
                            if (submission.CompetitionHashtag == hashtag)
                                listSubmissions += $"*POINT: {submission.Point}* - {submission.UserInfo} - [See photo](https://t.me/{chaturl}/{submission.MessageId})\n";

                        await vozCompetitionsBot.SendTextMessageAsync(
                            message.Chat,
                            listSubmissions,
                            ParseMode.Markdown,
                            disableWebPagePreview: true,
                            replyToMessageId: message.MessageId
                        );
                    }
                    catch (IndexOutOfRangeException)
                    {
                        await vozCompetitionsBot.SendTextMessageAsync(
                            message.Chat,
                            "Wrong format! Use this format to check current result of a competition:\n`/result #hashtag`",
                            ParseMode.Markdown,
                            replyToMessageId: message.MessageId
);
                    }
        }

        static async void Bot_OnCallbackQuery(object sender, CallbackQueryEventArgs callbackQueryEvent)
        {
            DataAccess database = new DataAccess("vozCompetitionsDB");
            // VOTE
            var callback = callbackQueryEvent.CallbackQuery;
            Vote vote = new Vote()
            {
                UserId = callback.From.Id,
                ChatId = callback.Message.Chat.Id,
                MessageId = callback.Message.ReplyToMessage.MessageId
            };

            if (database.GetList<Competition>("Competitions").Find(x => x.Hashtag == callback.Data).Status == "Opening")
                if (database.GetList<Vote>("Votes").Exists(x => x.ChatId == vote.ChatId && x.MessageId == vote.MessageId && x.UserId == vote.UserId))
                {
                    await vozCompetitionsBot.AnswerCallbackQueryAsync(callback.Id, "You already voted for this!");
                }
                else
                {
                    database.InsertRecord<Vote>("Votes", vote);
                    var submissionRecord = database.GetList<Submission>("Submissions").Find(x => x.MessageId == callback.Message.ReplyToMessage.MessageId);
                    submissionRecord.Point++;
                    database.UpdateRecordById<Submission>("Submissions", submissionRecord.Id, submissionRecord);

                    await vozCompetitionsBot.AnswerCallbackQueryAsync(callback.Id, "Thank you!");

                    await vozCompetitionsBot.EditMessageTextAsync(
                        callback.Message.Chat,
                        callback.Message.MessageId,
                        $"Vote for {callback.Message.ReplyToMessage.From.FirstName} {callback.Message.ReplyToMessage.From.LastName} @{callback.Message.ReplyToMessage.From.Username}",
                        replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData($"Vote - {submissionRecord.Point}", callback.Data))
                    );
                }
            else
            {
                await vozCompetitionsBot.AnswerCallbackQueryAsync(callback.Id, "This competition already ended!");
            }
        }
    }
}
