using System.Collections;
using System.Reflection;
using TaskPipeline.Abstractions;

namespace ClaimFlow.Web.Pipeline;

public static class ExecutionSummaryMapper
{
    public static ExecutionSummary Map(PipelineExecutionResult result)
    {
        return new ExecutionSummary
        {
            Status = result.Status.ToString(),
            DurationMs = result.Duration.TotalMilliseconds,
            Root = MapNode(result.Root),
            FailedNodes = result.FailedNodes.Select(MapNode).ToList(),
            CancelledNodes = result.CancelledNodes.Select(MapNode).ToList()
        };
    }

    private static ExecutionNodeSummary MapNode(object node)
    {
        return new ExecutionNodeSummary
        {
            Name = Read<string>(node, "Name") ?? Read<string>(node, "NodeName") ?? "node",
            Type = Read<string>(node, "NodeType") ?? node.GetType().Name,
            Status = ReadValue(node, "Status")?.ToString() ?? "Unknown",
            DurationMs = Read<TimeSpan>(node, "Duration").TotalMilliseconds,
            Exception = ReadException(node),
            Metadata = ReadMetadata(node),
            Children = ReadChildren(node)
        };
    }

    private static List<ExecutionNodeSummary> ReadChildren(object node)
    {
        var children = ReadValue(node, "Children") as IEnumerable;
        if (children is null)
        {
            return [];
        }

        var list = new List<ExecutionNodeSummary>();
        foreach (var child in children)
        {
            if (child is not null)
            {
                list.Add(MapNode(child));
            }
        }

        return list;
    }

    private static Dictionary<string, string> ReadMetadata(object node)
    {
        var metadata = ReadValue(node, "Metadata") as IEnumerable;
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (metadata is null)
        {
            return result;
        }

        foreach (var item in metadata)
        {
            if (item is null)
            {
                continue;
            }

            var key = Read<string>(item, "Key");
            var value = ReadValue(item, "Value")?.ToString();

            if (!string.IsNullOrWhiteSpace(key))
            {
                result[key] = value ?? string.Empty;
            }
        }

        return result;
    }

    private static string? ReadException(object node)
        => (ReadValue(node, "Exception") as Exception)?.Message ?? ReadValue(node, "Exception")?.ToString();

    private static T? Read<T>(object target, string propertyName)
    {
        var value = ReadValue(target, propertyName);
        return value is T typed ? typed : default;
    }

    private static object? ReadValue(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return property?.GetValue(target);
    }
}
