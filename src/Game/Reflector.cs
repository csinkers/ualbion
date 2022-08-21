using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace UAlbion.Game;

public class ReflectedObject
{
    public ReflectedObject(ReflectedObject parent, int index) { Parent = parent; CollectionIndex = index; }
    public ReflectedObject Parent { get; }
    public int CollectionIndex { get; }
    public object Target { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
    public IEnumerable<ReflectedObject> SubObjects { get; set; }
    public override string ToString() => $"{CollectionIndex} {Name} = {Value}";
}

public static class Reflector
{
    static object GetPropertySafe(PropertyInfo x, object o)
    {
        try
        {
            return !x.CanRead ? "<< No Getter! >>" : x.GetValue(o);
        }
        catch (TargetException e) { return e; }
        catch (TargetParameterCountException e) { return e; }
        catch (NotSupportedException e) { return e; }
        catch (MethodAccessException e) { return e; }
        catch (TargetInvocationException e) { return e; }
    }

    static object GetFieldSafe(FieldInfo x, object o)
    {
        try { return x.GetValue(o); }
        catch (TargetException e) { return e; }
        catch (NotSupportedException e) { return e; }
        catch (FieldAccessException e) { return e; }
        catch (ArgumentException e) { return e; }
    }

    public static ReflectedObject Reflect(string name, object o, ReflectedObject parent, int collectionIndex = 0)
    {
        var result = ReflectCommonType(name, o, parent, collectionIndex);
        if (result != null)
            return result;

        result = new ReflectedObject(parent, collectionIndex);
        var t = o.GetType();
        result.Name = name;
        result.Value = o.ToString();
        result.Target = o;
        var publicProperties =
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => !x.GetIndexParameters().Any());

        var publicFields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);

        var privateProperties =
            t.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => !x.GetIndexParameters().Any());

        var privateFields = t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => !x.Name.Contains("__BackingField", StringComparison.InvariantCulture));

        ReflectedObject FormatProperty(PropertyInfo x) => Reflect(x.Name, GetPropertySafe(x, o), result);
        ReflectedObject FormatField(FieldInfo x) => Reflect(x.Name, GetFieldSafe(x, o), result);

        var formattedPublic =
            publicProperties.Select(FormatProperty)
                .Concat(publicFields.Select(FormatField))
                .OrderBy(x => x.Name);

        var formattedPrivate =
            privateProperties
                .Select(FormatProperty)
                .Concat(privateFields.Select(FormatField))
                .OrderBy(x => x.Name);

        result.SubObjects = formattedPublic.Concat(formattedPrivate);
        return result;
    }

    public static ReflectedObject ReflectCommonType(string name, object o, ReflectedObject parent, int index)
    {
        switch (o)
        {
            case null:
                return new ReflectedObject(parent, index)
                    { Name = name, Target = null, Value = "null" };
            case string s:
                return new ReflectedObject(parent, index)
                    { Name = name, Target = o, Value = $"\"{s.Replace("\"", "\\\"", StringComparison.Ordinal)}\"" };

            case bool:   case byte:
            case ushort: case short:
            case uint:   case int:
            case ulong:  case long:
            case float:  case double:
                return new ReflectedObject(parent, index)
                    { Name = name, Target = o, Value = o.ToString() };
            case Enum e:
                return new ReflectedObject(parent, index)
                    { Name = name, Target = o, Value = e.ToString() };
            case Vector2 v:
                return new ReflectedObject(parent, index)
                    { Name = name, Target = o, Value = $"({v.X}, {v.Y})" };
            case Vector3 v:
                return new ReflectedObject(parent, index)
                    { Name = name, Target = o, Value = $"({v.X}, {v.Y}, {v.Z})" };
            case ICollection e:
                var coll = new ReflectedObject(parent, index) { Name = name, Target = o, Value = e.Count.ToString(CultureInfo.InvariantCulture) };
                coll.SubObjects = e.Cast<object>().Select((x, i) => Reflect(i.ToString(CultureInfo.InvariantCulture), x, coll, i));
                return coll;

            case IEnumerable e:
                var enumerable = new ReflectedObject(parent, index) { Name = name, Target = o, Value = "", };
                enumerable.SubObjects = e.Cast<object>().Select((x, i) => Reflect(i.ToString(CultureInfo.InvariantCulture), x, enumerable, i));
                return enumerable;
        }

        return null;
    }
}