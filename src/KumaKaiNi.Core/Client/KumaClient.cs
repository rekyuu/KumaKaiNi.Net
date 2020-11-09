using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KumaKaiNi.Core
{
    public class KumaClient
    {
        private readonly Dictionary<string, MethodInfo> _commands;
        private readonly Dictionary<string, MethodInfo> _phrases;

        public KumaClient()
        {
            _commands = new Dictionary<string, MethodInfo>();
            _phrases = new Dictionary<string, MethodInfo>();

            Assembly assembly = Assembly.GetExecutingAssembly();

            // Compile a dictionary of commands.
            MethodInfo[] commandMethods = assembly.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes<CommandAttribute>().Any())
                .ToArray();

            foreach (MethodInfo method in commandMethods)
            {
                IEnumerable<CommandAttribute> commandAttrs = method.GetCustomAttributes<CommandAttribute>();
                foreach (CommandAttribute commandAttr in commandAttrs)
                {
                    foreach (string command in commandAttr.Commands)
                    {
                        _commands.Add(command, method);
                    }
                }
            }

            // Compile a dictionary of phrases.
            MethodInfo[] phraseMethods = assembly.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes<PhraseAttribute>().Any())
                .ToArray();

            foreach (MethodInfo method in phraseMethods)
            {
                IEnumerable<PhraseAttribute> phraseAttrs = method.GetCustomAttributes<PhraseAttribute>();
                foreach (PhraseAttribute phraseAttr in phraseAttrs)
                {
                    foreach (string phrase in phraseAttr.Phrases)
                    {
                        _phrases.Add(phrase, method);
                    }
                }
            }
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

            if (request.IsCommand && _commands.ContainsKey(request.Command))
            {
                method = _commands[request.Command];
            }
            else if (_phrases.ContainsKey(request.Message.ToLower()))
            {
                method = _phrases[request.Message.ToLower()];
            }

            if (method != null)
            {
                if (method.DeclaringType == typeof(Admin) && !request.UserIsAdmin) return new Response();

                if (method.GetParameters().Length > 0) response = (Response)method.Invoke(new object(), new object[] { request });
                else response = (Response)method.Invoke(new object(), new object[] { });
            }

            if (request.Message != "") Logging.LogToDatabase(request, response);

            return response;
        }
    }
}
