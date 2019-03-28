using SharpCollections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace ArisaTwitchBot.Commands
{
    public class CommandHandler
    {
        protected readonly ArisaTwitchClient ArisaTwitchClient;

        private readonly CompactPrefixTree<ICommand> _commandTree;

        public CommandHandler(ArisaTwitchClient arisaTwitchClient)
        {
            ArisaTwitchClient = arisaTwitchClient;
            _commandTree = new CompactPrefixTree<ICommand>(16, 32, 32);

            Add(new CommandListCommand(this));
        }

        public CommandHandler Add(ICommand command)
        {
            _commandTree.Add(command.Command, command);
            if (command is ICommandAlias commandAlias)
            {
                foreach (var alias in commandAlias.CommandAliases)
                {
                    _commandTree.Add(alias, command);
                }
            }
            return this;
        }
        public CommandHandler Add<TCommand>()
            where TCommand : ICommand, new()
        {
            return Add(new TCommand());
        }

        public void Handle(ChatCommand chatCommand)
        {
            if (_commandTree.TryMatchLongest(chatCommand.CommandText, out KeyValuePair<string, ICommand> commandMatch))
            {
                Log($"Received a command \"{chatCommand.CommandText}\" from {chatCommand.ChatMessage.Username}");
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var context = new CommandContext(ArisaTwitchClient, chatCommand);
                        await commandMatch.Value.Handle(context);
                    }
                    catch (Exception ex)
                    {
                        ArisaTwitchClient.LogException(ex);
                    }
                });
            }
            else
            {
                Log($"Received an unknown command \"{chatCommand.CommandText}\" from {chatCommand.ChatMessage.Username}");
            }
        }

        private void Log(string message) => ArisaTwitchClient.Log($"{nameof(CommandHandler)} {message}");

        private class CommandListCommand : ICommand
        {
            public string Command => "commands";

            private readonly CommandHandler _commandHandler;

            public CommandListCommand(CommandHandler commandHandler)
            {
                _commandHandler = commandHandler;
            }

            public Task Handle(CommandContext context)
            {
                List<string> commands = _commandHandler._commandTree.Keys.ToList();

                if (commands.Count == 1)
                {
                    context.SendMention("There are no available commands :(");
                }
                else
                {
                    StringBuilder messageBuilder = new StringBuilder();
                    messageBuilder.Append(" Here's a list: ");

                    char commandIdentifier = context.ChatCommand.CommandIdentifier;

                    for (int i = 1; i < commands.Count - 2; i++)
                    {
                        messageBuilder.Append(commandIdentifier);
                        messageBuilder.Append(commands[i]);
                        messageBuilder.Append(", ");
                    }
                    if (commands.Count > 2)
                    {
                        messageBuilder.Append(commandIdentifier);
                        messageBuilder.Append(commands[commands.Count - 2]);
                        messageBuilder.Append(" and ");
                    }
                    messageBuilder.Append('!');
                    messageBuilder.Append(commands[commands.Count - 1]);

                    context.SendMention(messageBuilder.ToString());
                }

                return Task.CompletedTask;
            }
        }
    }
}
