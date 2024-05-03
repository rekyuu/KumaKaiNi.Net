using System.Reflection;
using KumaKaiNi.Core.Attributes;
using KumaKaiNi.Core.Database;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;
using Serilog;

namespace KumaKaiNi.Core;

public class KumaClient
{
    /// <summary>
    /// Delegate for informing that a response is being created
    /// </summary>
    public delegate void IsProcessingEventHandler(long? channelId);
    
    /// <summary>
    /// Event handler for informing that a response is being created
    /// </summary>
    public event IsProcessingEventHandler? Processing;
    
    /// <summary>
    /// Delegate for informing that a response has been completed
    /// </summary>
    public delegate void ResponseEventHandler(KumaResponse kumaResponse);
    
    /// <summary>
    /// Event handler for informing that a response has been completed
    /// </summary>
    public event ResponseEventHandler? Responded;
    
    private readonly Dictionary<string, MethodInfo> _phrases = GetAllMethodsWithAttribute<PhraseAttribute>();
    private readonly Dictionary<string, MethodInfo> _commands = GetAllMethodsWithAttribute<CommandAttribute>();

    /// <summary>
    /// Asynchronously interprets a request and determines if a response should be generated.
    /// </summary>
    /// <param name="kumaRequest">The request to process.</param>
    public async Task ProcessRequest(KumaRequest kumaRequest)
    {
        try
        {
            // Immediately log the incoming request
            await Logging.LogRequestToDatabaseAsync(kumaRequest);
            
            KumaResponse? response = null;
            MethodInfo? method = null;
            string lowerCaseMessage = kumaRequest.Message.ToLowerInvariant();

            // Message starts with a ! command of some kind
            if (kumaRequest is { Command: not null, IsCommand: true })
            {
                // Check if this is a Core command
                if (_commands.TryGetValue(kumaRequest.Command, out MethodInfo? commandMethod))
                {
                    method = commandMethod;
                }
                // Check if this is a custom command
                else
                {
                    await using KumaKaiNiDbContext db = new();
                    string? customCommandResponse = db.CustomCommands
                        .Where(x => x.Command == kumaRequest.Command)
                        .Select(x => x.Response)
                        .FirstOrDefault();
                    
                    if (!string.IsNullOrEmpty(customCommandResponse)) response = new KumaResponse(customCommandResponse);
                }
            }
            // Check if a phrase is mentioned in the message if it is not a command
            else if (!string.IsNullOrEmpty(lowerCaseMessage) && 
                     _phrases.TryGetValue(lowerCaseMessage, out MethodInfo? value))
            {
                method = value;
            }

            // If a Core command was found, run the corresponding method
            if (method != null)
            {
                CommandAttribute? commandAttribute = method.GetCustomAttribute<CommandAttribute>();
                if (commandAttribute != null)
                {
                    int requiredAuthorityLevel = (int)commandAttribute.UserAuthority;
                    int userAuthorityLevel = (int)kumaRequest.UserAuthority;

                    if (userAuthorityLevel < requiredAuthorityLevel)
                    {
                        Log.Information("{Username} is not authorized to run {Method}",
                            kumaRequest.Username,
                            method.Name);
                    }
                    else if (commandAttribute.Nsfw && !kumaRequest.ChannelIsNsfw)
                    {
                        Log.Information("{Method} is not allowed to run in non-NSFW channel",
                            method.Name);
                    }
                    else
                    {
                        // Notify that a response is being generated
                        Processing?.Invoke(kumaRequest.ChannelId);
                        
                        object?[]? methodParams = null;
                        if (method.GetParameters().Length > 0) methodParams = [kumaRequest];
                    
                        // Run asynchronous method
                        if (method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null)
                        {
                            Task<KumaResponse?>? methodResponse = (Task<KumaResponse?>?)method.Invoke(null, methodParams);
                            if (methodResponse != null) response = await methodResponse;
                        }
                        // Run synchronous method
                        else
                        {
                            KumaResponse? methodResponse = (KumaResponse?)method.Invoke(null, methodParams);
                            if (methodResponse != null) response = methodResponse;
                        }
                    }
                }
            }
            
            if (response != null)
            {
                // Log the response
                await Logging.LogResponseToDatabaseAsync(kumaRequest, response);
                
                // Fire off the response
                response.ChannelId = kumaRequest.ChannelId;
                response.SourceSystem = kumaRequest.SourceSystem;
                Responded?.Invoke(response);
            }
        }
        catch (Exception ex)
        {
            await Logging.LogExceptionToDatabaseAsync(ex, 
                "An exception was thrown while processing the request: {Request}", kumaRequest);
        }
    }

    private static Dictionary<string, MethodInfo> GetAllMethodsWithAttribute<T>() where T : BaseResponseAttribute
    {
        // Get all the methods within KumaKaiNi.Core
        Assembly core = Assembly.GetCallingAssembly();
        MethodInfo[] coreMethods = core.GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes<T>().Any())
            .Where(m => m.IsStatic)
            .Where(m => m.IsPublic)
            .ToArray();

        // Get all the methods from the calling library using KumaKaiNi.Core
        Assembly? entry = Assembly.GetEntryAssembly();
        MethodInfo[]? entryMethods = entry?.GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes<T>().Any())
            .Where(m => m.IsStatic)
            .Where(m => m.IsPublic)
            .ToArray();

        MethodInfo[] allMethods = coreMethods.Concat(entryMethods ?? []).ToArray();

        // Create a map using each individual value from the attribute
        Dictionary<string, MethodInfo> methods = new();
        foreach (MethodInfo method in allMethods)
        {
            IEnumerable<T> attributes = method.GetCustomAttributes<T>();
            foreach (T attribute in attributes)
            {
                foreach (string value in attribute.Values) methods.Add(value, method);
            }
        }

        return methods;
    }
}