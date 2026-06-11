using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ControlCatalog.Helper;

public static class XmlDocSummary
{
    public static string? GetXmlSummary(this MemberInfo member)
    {
        // The generator's dictionary is keyed by the XML doc id WITHOUT its
        // kind prefix (T:/F:/P:/M:/E:). Build the canonical id for the
        // given reflection member, then strip the prefix before lookup.
        string fullId = member switch
        {
            Type         type => "T:" + type.FullName,
            FieldInfo    => "F:" + member.DeclaringType?.FullName + "." + member.Name,
            PropertyInfo => "P:" + member.DeclaringType?.FullName + "." + member.Name,
            MethodInfo m => "M:" + BuildMethodId(m),
            EventInfo    => "E:" + member.DeclaringType?.FullName + "." + member.Name,
            _ => throw new ArgumentOutOfRangeException(nameof(member), member, null)
        };

        // Drop the leading 2-character prefix so it matches the generator's
        // stored key form ("Avalonia.Controls.Border.BackgroundProperty", etc.).
        string key = fullId.Length >= 2 ? fullId.Substring(2) : fullId;
        return XmlDocProvider.GetSummary(key);
    }

    private static string BuildMethodId(MethodInfo m)
    {
        string decl = (m.DeclaringType?.FullName ?? "") + "." + m.Name;
        ParameterInfo[] ps = m.GetParameters();
        if (ps.Length == 0) return decl;

        var sb = new StringBuilder(decl);
        sb.Append('(');
        sb.Append(string.Join(",", ps.Select(p => p.ParameterType.FullName)));
        sb.Append(')');
        return sb.ToString();
    }
}