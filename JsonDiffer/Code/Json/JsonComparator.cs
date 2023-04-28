using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Automata.Extensions.Collections;

namespace JsonDiffer.Code.Json
{
    public class JsonComparator
    {
        public JsonNode A { get; set; }
        public JsonNode B { get; set; }

        public int Depth { get; set; }

        public LinkedList<JsonNodeDiff> Differences { get; set; }

        public JsonComparator(JsonNode a, JsonNode b, int depth)
        {
            A = a ?? throw new ArgumentNullException(nameof(a));
            B = b ?? throw new ArgumentNullException(nameof(b));
            Depth = depth + 1;

            Differences = new LinkedList<JsonNodeDiff>();
        }

        public async Task<LinkedList<JsonNodeDiff>> Compare()
        {
            if (A is JsonValue av && B is JsonValue bv)
            {
                if (av.ToJsonString() == bv.ToJsonString())
                {
                    return Differences;
                }
                else
                {
                    Differences.AddLast(new JsonNodeDiff(A, B));
                    return Differences;
                }
            }
            else if (A is JsonObject ao && B is JsonObject bo)
            {
                var aProperties = GetProperties(ao);
                var bProperties = GetProperties(bo);

                if (aProperties is null || bProperties is null)
                    return Differences;

                var pairs = new List<JsonNodePair>(aProperties.Count > bProperties.Count
                    ? aProperties.Count : bProperties.Count);

                foreach (var kv in aProperties)
                {
                    var name = kv.Key;

                    var aNode = kv.Value;
                    var bLinkedNode = bProperties.FirstNode(x => x.Key == name);
                    if (bLinkedNode is { } bln)
                    {
                        // Добавляем джсон в пары и убираем ссылку на ноду из списка свойств,
                        // чтобы в последствии подсчитать сколько осталось
                        var bJsonNode = bln.Value.Value;
                        pairs.Add(new JsonNodePair(name, aNode, bJsonNode));
                        bProperties.Remove(bln);
                    }
                    else
                    {
                        // Добавляем свойство без пары
                        pairs.Add(new JsonNodePair(name, aNode, null));
                    }
                }

                // Если у второго объекта остались свойства без пары
                if (bProperties.Count > 0)
                {
                    foreach (var keyValuePair in bProperties)
                    {
                        var name = keyValuePair.Key;
                        var bNode = keyValuePair.Value;
                        pairs.Add(new JsonNodePair(name, null, bNode));
                    }
                }

                // Сравниваем все свойства
                foreach (var pair in pairs)
                {
                    // Если один из членов null, то пара имеет различия
                    if (pair.HasNull())
                    {
                        var jsonDiff = new JsonNodeDiff(pair);
                        Differences.AddLast(jsonDiff);
                        continue;
                    }

                    var comparator = new JsonComparator(pair.A!, pair.B!, Depth);
                    var diffs = await comparator.Compare();
                    Differences.Append(diffs);
                }

                return Differences;
            }
            else if (A is JsonArray aa && B is JsonArray ba)
            {
                var aElements = GetElements(aa);
                var bElements = GetElements(ba);

                var pairs = new List<JsonNodePair>(aElements.Count > bElements.Count
                    ? aElements.Count : bElements.Count);

                var index = 0;
                for (; index < aElements.Count; index++)
                {
                    var aElement = aElements[index];
                    if (index >= bElements.Count)
                    {
                        pairs.Add(new JsonNodePair(index.ToString(), aElement, null));
                    }
                    else
                    {
                        var bElement = bElements[index];
                        pairs.Add(new JsonNodePair(index.ToString(), aElement, bElement));
                    }
                }

                if (index < bElements.Count)
                {
                    for (; index < bElements.Count; index++)
                    {
                        pairs.Add(new JsonNodePair(index.ToString(), null, bElements[index]));
                    }
                }

                foreach (var pair in pairs)
                {
                    if (pair.HasNull())
                    {
                        Differences.AddLast(new JsonNodeDiff(pair));
                        continue;
                    }

                    var comparator = new JsonComparator(pair.A!, pair.B!, Depth);
                    var diffs = await comparator.Compare();
                    Differences.Append(diffs);
                }
            }
            else
            {
                Differences.AddLast(new JsonNodeDiff(A, B));
            }

            return Differences;
        }

        public LinkedList<KeyValuePair<string, JsonNode?>>? GetProperties(JsonObject jsonObject)
        {
            try
            {
                var list = new LinkedList<KeyValuePair<string, JsonNode?>>();
                foreach (var keyValuePair in jsonObject)
                {
                    list.AddLast(keyValuePair);
                }

                return list;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<JsonNode?> GetElements(JsonArray jsonArray)
        {
            var list = new List<JsonNode?>();
            foreach (var node in jsonArray)
            {
                list.Add(node);
            }

            return list;
        }
    }

    public class JsonNodeDiff
    {
        public static JsonSerializerOptions Options { get; set; } = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public JsonNode? A { get; private set; }
        public JsonNode? B { get; private set; }

        public string Path { get; }
        public string? AExpand { get; }
        public string? BExpand { get; }
        public string? Modification { get; }

        public JsonNodeDiff(JsonNode? a, JsonNode? b)
        {
            A = a;
            B = b;

            Path = (A?.GetPath() ?? B?.GetPath() ?? "NULL");

            AExpand = A?.ToJsonString(Options);
            BExpand = B?.ToJsonString(Options);

            Modification = AnalyzeDiffType();
        }

        public JsonNodeDiff(JsonNodePair pair)
        {
            A = pair.A;
            B = pair.B;

            Path = (A?.GetPath() ?? B?.GetPath() ?? "NULL");

            AExpand = A?.ToJsonString(Options);
            BExpand = B?.ToJsonString(Options);

            Modification = AnalyzeDiffType();
        }

        public string AnalyzeDiffType()
        {
            var modification = string.Empty;

            var a = AExpand;
            var b = BExpand;

            if (a is null)
            {
                if (b is not null)
                    modification = "added";
                else
                    modification = "corrupt";
            }
            else
            {
                if (b is null)
                    modification ="removed";
                else
                    modification = "changed";
            }

            return modification;
        }

        public override string ToString()
        {
            var valueDiff = string.Empty;

            var a = A?.ToJsonString(Options);
            var b = B?.ToJsonString(Options);

            if (a is null)
            {
                if (b is not null)
                    valueDiff = "added " + b;
            }
            else
            {
                if (b is null)
                    valueDiff = a + " removed";
                else
                    valueDiff = a + " changed to " + b;
            }

            return (A?.GetPath() ?? B?.GetPath() ?? "NULL") + $" {valueDiff}";

            //if (A is not null)
            //{
            //    if (B is null)
            //    {
            //        valueDiff = "removed";
                    
            //    }else if (A is JsonObject ao)
            //    {
            //        valueDiff =
            //            $"{ao.ToJsonString(Options)} changed to {B.AsObject().ToJsonString(Options)}";

            //    }else if (A is JsonArray aa)
            //    {
            //        valueDiff =
            //            $"{aa.ToJsonString(Options)} changed to {B.AsArray().ToJsonString(Options)}";
            //    }else if (A is JsonValue)
            //    {
            //        valueDiff =
            //            $"{A.AsValue().ToJsonString(Options)} changed to {B.AsValue().ToJsonString(Options)}";
            //    }
            //    return (A?.GetPath() ?? B?.GetPath() ?? "NULL") + $" {valueDiff}";

            //}else if (B is not null)
            //{
            //    valueDiff = $"added";
            //}
            //else
            //{
            //    valueDiff = "invalid difference";
            //}

           // return (A?.GetPath() ?? B?.GetPath() ?? "NULL") + $" {valueDiff}";
        }
    }

    public class JsonNodePair
    {
        public JsonNodePair(string name, JsonNode? a, JsonNode? b)
        {
            Name = name;
            A = a;
            B = b;
        }

        public string Name { get; set; }
        public JsonNode? A { get; set; }
        public JsonNode? B { get; set; }

        public bool HasNull()
        {
            return A is null || B is null;
        }
    }

    public enum JsonArrayCompareType
    {
        Order,
        Objects
    }
}
