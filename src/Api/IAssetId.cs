using System;
using System.Reflection;

namespace UAlbion.Api
{
    public interface IAssetId
    {
        uint ToUInt32();
        string ToStringNumeric();

        delegate T ParserDelegate<out T>(string s);
        static ParserDelegate<T> GetParser<T>() where T : IAssetId
        {
            var type = typeof(T);
            var parser = type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public);

            if(parser == null)
                throw new InvalidOperationException($"Asset type \"{type}\" was expected to contain a static Parse method, but none could be found.");

            var parameters = parser.GetParameters();
            if (parameters.Length != 1)
                throw new InvalidOperationException($"Asset type \"{type}\" contains a static Parse method, but it accepts {parameters.Length} parameters, rather than the expected 1 (a string).");

            if (parameters[0].ParameterType != typeof(string))
                throw new InvalidOperationException($"Asset type \"{type}\" contains a static Parse method, but it accepts a {parameters[0].ParameterType}, rather than the expected string.");

            if (parser.ReturnType != type)
                throw new InvalidOperationException($"Asset type \"{type}\" contains a static Parse method, but it return a {parser.ReturnType}, rather than the type itself.");

            return (ParserDelegate<T>)Delegate.CreateDelegate(typeof(ParserDelegate<T>), parser);
        }
    }
}