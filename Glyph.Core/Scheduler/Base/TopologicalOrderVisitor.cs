﻿using System.Collections.Generic;
using System.Linq;
using Diese.Graph;

namespace Glyph.Core.Scheduler.Base
{
    public class TopologicalOrderVisitor<T> : IVisitor<SchedulerGraph<T>.Vertex, SchedulerGraph<T>.Edge>
    {
        private readonly List<T> _result;
        private readonly IReadOnlyCollection<T> _readOnlyResult;
        private readonly Stack<SchedulerGraph<T>.Vertex> _visited;
        public IEnumerable<T> Result => _readOnlyResult;

        public TopologicalOrderVisitor()
        {
            _result = new List<T>();
            _readOnlyResult = _result.AsReadOnly();

            _visited = new Stack<SchedulerGraph<T>.Vertex>();
        }

        public void Process(SchedulerGraph<T> graph)
        {
            _result.Clear();
            _visited.Clear();

            int count = graph.Vertices.Sum(x => x.Items.Count);

            foreach (IGrouping<Priority, SchedulerGraph<T>.Vertex> group in graph.Vertices.GroupBy(x => x.Priority).OrderByDescending(x => (int)x.Key))
                foreach (SchedulerGraph<T>.Vertex vertex in group)
                {
                    vertex.Accept(this);

                    if (_result.Count >= count)
                        return;
                }
        }

        public void Visit(SchedulerGraph<T>.Vertex vertex)
        {
            if (_visited.Contains(vertex))
                throw new CyclicDependencyException(_visited.Select(x => x.Items.First()).Cast<object>());

            if (vertex.Items.Count == 0 || _result.Contains(vertex.Items.First()))
                return;

            _visited.Push(vertex);

            foreach (SchedulerGraph<T>.Edge successor in vertex.Successors)
                successor.End.Accept(this);

            _visited.Pop();

            _result.AddRange(vertex.Items);
        }
    }
}