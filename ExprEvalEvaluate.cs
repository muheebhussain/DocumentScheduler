using System;
using System.Collections.Generic;
using ExpressionEvaluator;

public class MyObject
{
    public int Property1 { get; set; }
    public int Property2 { get; set; }
}

public class Program
{
    public static void Main()
    {
        List<MyObject> objects = new List<MyObject>
        {
            new MyObject { Property1 = 1, Property2 = 2 },
            new MyObject { Property1 = 3, Property2 = 4 },
        };

        string expression = "Property1 + Property2 * 2";
        List<int> results = EvaluateExpression(objects, expression);

        foreach (int result in results)
        {
            Console.WriteLine(result);
        }
    }

    public static List<int> EvaluateExpression(List<MyObject> objects, string expression)
    {
        List<int> results = new List<int>();

        foreach (MyObject obj in objects)
        {
            var evaluator = new CompiledExpression(expression)
            {
                TypeRegistry = new TypeRegistry()
            };

            evaluator.TypeRegistry.RegisterSymbol("Property1", obj.Property1);
            evaluator.TypeRegistry.RegisterSymbol("Property2", obj.Property2);

            results.Add(Convert.ToInt32(evaluator.Eval()));
        }

        return results;
    }
}
