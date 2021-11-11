using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace KumaKaiNi.Core
{
    public class KumaClient
    {
        private readonly Dictionary<string, MethodInfo> _commands;
        private readonly Dictionary<string, MethodInfo> _phrases;
        private Dictionary<string, string> _customCommands;

        public KumaClient()
        {
            _commands = GetCommands();
            _phrases = GetPhrases();
            _customCommands = GetCustomCommands();
        }

        public async Task<Response> GetResponseAsync(Request request)
        {
            Response response = new Response();
            await Task.Run(() => { response = GetResponse(request); });

            return response;
        }

        public Response GetResponse(Request request)
        {
            request.Parse();

            Response response = new Response();
            MethodInfo method = null;

            if (request.IsCommand)
            {
                if (_commands.ContainsKey(request.Command))
                {
                    method = _commands[request.Command];
                }
                else if (_customCommands.ContainsKey(request.Command))
                {
                    response.Message = _customCommands[request.Command];
                }
            }
            else if (_phrases.ContainsKey(request.Message.ToLower()))
            {
                method = _phrases[request.Message.ToLower()];
            }

            if (method != null)
            {
                bool requiresAdmin = method.DeclaringType?.GetCustomAttribute<RequireAdminAttribute>() != null;
                bool requiresModerator = method.DeclaringType?.GetCustomAttribute<RequireModeratorAttribute>() != null;

                if (requiresAdmin && request.Authority != UserAuthority.Admin) return new Response();
                if (requiresModerator && request.Authority == UserAuthority.User) return new Response();

                if (method.GetParameters().Length > 0) response = (Response)method.Invoke(new object(), new object[] { request });
                else response = (Response)method.Invoke(new object(), new object[] { });

                if (method.DeclaringType == typeof(CustomCommands) && response.Message != "") _customCommands = GetCustomCommands();
            }

            Logging.LogToDatabase(request, response);

            return response;
        }

        private static Dictionary<string, MethodInfo> GetCommands()
        {
            Dictionary<string, MethodInfo> commands = new Dictionary<string, MethodInfo>();
            Assembly core = Assembly.GetCallingAssembly();
            Assembly entry = Assembly.GetEntryAssembly();

            MethodInfo[] coreCommands = core.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes<CommandAttribute>().Any())
                .ToArray();

            MethodInfo[] entryCommands = entry?.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes<CommandAttribute>().Any())
                .ToArray();

            MethodInfo[] allCommands = coreCommands.Concat(entryCommands ?? Array.Empty<MethodInfo>()).ToArray();

            foreach (MethodInfo method in allCommands)
            {
                IEnumerable<CommandAttribute> commandAttrs = method.GetCustomAttributes<CommandAttribute>();
                foreach (CommandAttribute commandAttr in commandAttrs)
                {
                    foreach (string command in commandAttr.Commands)
                    {
                        commands.Add(command, method);
                    }
                }
            }

            return commands;
        }

        private static Dictionary<string, MethodInfo> GetPhrases()
        {
            Dictionary<string, MethodInfo> phrases = new Dictionary<string, MethodInfo>();
            Assembly core = Assembly.GetCallingAssembly();
            Assembly entry = Assembly.GetEntryAssembly();

            MethodInfo[] corePhrases = core.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes<PhraseAttribute>().Any())
                .ToArray();

            MethodInfo[] entryPhrases = entry?.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes<PhraseAttribute>().Any())
                .ToArray();

            MethodInfo[] allPhrases = corePhrases.Concat(entryPhrases ?? Array.Empty<MethodInfo>()).ToArray();

            foreach (MethodInfo method in allPhrases)
            {
                IEnumerable<PhraseAttribute> phraseAttrs = method.GetCustomAttributes<PhraseAttribute>();
                foreach (PhraseAttribute phraseAttr in phraseAttrs)
                {
                    foreach (string phrase in phraseAttr.Phrases)
                    {
                        phrases.Add(phrase, method);
                    }
                }
            }

            return phrases;
        }

        private static Dictionary<string, string> GetCustomCommands()
        {
            Dictionary<string, string> customCommands = new Dictionary<string, string>();
            List<CustomCommand> commands = Database.GetMany<CustomCommand>();

            foreach (CustomCommand command in commands)
            {
                customCommands.Add(command.Command, command.Response);
            }

            return customCommands;
        }
    }
}
