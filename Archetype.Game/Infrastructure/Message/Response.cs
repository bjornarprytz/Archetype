using System;
using System.Collections.Generic;
using System.Linq;
using MediatR;

namespace Archetype.Game.Infrastructure
{
    public class Response<T> : IResponseWrapper<T>
    {
        public bool IsError => Outcome == Outcome.Error;
        public bool IsValid => Outcome == Outcome.Ok;
        public bool IsCancelled => Outcome == Outcome.Cancel;
        public T Payload { get; private set; }
        public Outcome Outcome { get; private set; }
        public IList<string> Failures { get; private set; }
        public string Message => Failures.FirstOrDefault() ?? string.Empty;
		
        private Response(T payload, Outcome outcome, IEnumerable<string> failures = null)
        {
            Payload = payload;
            Outcome = outcome;
            Failures = failures != null ? failures.ToList() : new List<string>();
        }

        public static IResponseWrapper<T> Ok(T payload) => new Response<T>(payload, Outcome.Ok);
        public static IResponseWrapper<T> Cancel() => new Response<T>(default, Outcome.Cancel);
        public static IResponseWrapper<T> Error(IEnumerable<string> failures) => new Response<T>(default, Outcome.Error, failures);
        public static IResponseWrapper<T> Error(string error) => new Response<T>(default, Outcome.Error, new []{ error });
    }

    public static class Response
    {
        public static IResponseWrapper<T> Ok<T>(T payload) => Response<T>.Ok(payload);
        public static IResponseWrapper<T> Cancel<T>() => Response<T>.Cancel();
        public static IResponseWrapper<Unit> Ok() => Response<Unit>.Ok(default);

        public static T GenerateTypedErrorResponse<T>(IList<string> errors)
            where  T : IResponseWrapper
        {
            const string methodName = nameof(Response<Unit>.Error);
            
            var responseType = typeof(T).IsGenericType
                ? typeof(Response<>).MakeGenericType(typeof(T).GetGenericArguments())
                : typeof(Response<Unit>);
            
            // Get a static method which accepts errors as a parameter
            var methodInfo = responseType.GetMethods()
                .Where(m => m.Name == methodName)
                .FirstOrDefault(m => m.ReturnType.IsAssignableTo(typeof(T))
                                     && m.GetParameters().Length == 1
                                     && m.GetParameters().Single().ParameterType.IsInstanceOfType(errors));

            if (methodInfo == null)
            {
                throw new MissingMethodException($"Missing response constructor {methodName} for type: {typeof(T)}");
            }
			
            return (T)methodInfo.Invoke(null, new []{ errors });
        }
    }
	
    public interface IResponseWrapper<out T> : IResponseWrapper
    {
        T Payload { get; }
    }
	
    public interface IResponseWrapper
    {
        bool IsError { get; }
        bool IsValid { get; }
        bool IsCancelled { get; }
        Outcome Outcome { get; }
        string Message { get; }
        IList<string> Failures { get; }
    }

    public enum Outcome
    {
        Ok,
        Error,
        Cancel
    }
}