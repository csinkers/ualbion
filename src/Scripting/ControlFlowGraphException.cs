using System;
using System.Runtime.Serialization;

namespace UAlbion.Scripting
{
    public sealed class ControlFlowGraphException : Exception
    {
        public IGraph Graph { get; }
        public ControlFlowGraphException() { }
        public ControlFlowGraphException(string message) : base(message) { }
        public ControlFlowGraphException(string message, Exception innerException) : base(message, innerException) { }
        public ControlFlowGraphException(string message, IGraph graph) : base(message) => Graph = graph;
        public ControlFlowGraphException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}