#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Cysharp.Threading.Tasks;
using fastJSON;
using Photon;
using UnityEditor;
using Object = UnityEngine.Object;

[Serializable]
public class ExtendedEvent2
{
    public string expression;
    public Type type;
    public Object target;
    
  
    
    public ExtendedEvent2(ParameterInfo dd)
    {
        expression = dd.Name;
        type = dd.ParameterType;
    }
    public ExtendedEvent2()
    {
        
    }
    public ExtendedEvent2(Expression<Func<Object>> dd)
    {
        expression = dd.ToString();
        type = (dd.Body as MemberExpression).Type;
    }
    public void Invoke(object o)
    {
        
    }
    public int index = -1;
    public string m_parameters="";
    public object[] parameters { get { return string.IsNullOrEmpty(m_parameters) ? new object[10] : JSON.ToObject<object[]>(m_parameters); } set { m_parameters = JSON.ToJSON(value); } } 
    public void OnInspectorGUI()
    {
        
        
        if (string.IsNullOrEmpty(expression))
            target = EditorGUILayout.ObjectField("target", target, typeof(MonoBehaviour));
        
        if (target != null)
            type = target.GetType();
        if (type != null)
        {
            members.Clear();
            Fill(type);
            if (bs.HasChanged(EditorGUILayout.Popup(index, members.Select(a => a.path).ToArray()), ref index))
                parameters = new object[] {  };
            Member member = members.ArrayGetValue(index);
            if (member != null)
            {
                if (member.info is MethodInfo mi)
                {
                    var parameterInfos = mi.GetParameters();
                    for (var i = 0; i < parameterInfos.Length; i++)
                    {
                        var p = parameterInfos[i];
                        VarParse2.DrawObject(p, p.Name);
                    }
                }
            }
        }
        
    }
    
    
    public class Member
    {
        public MemberInfo info;
        public string path;
        public string code;
    }
    private List<Member> members = new List<Member>();
    private void Fill(Type type, string path = "")
        {
             

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
            if (path == "")
            {
                path = type.Name + "/";
                foreach (FieldInfo a in type.GetFields(flags).Where(f => !f.IsSpecialName && f.DeclaringType == type && !f.FieldType.IsValueType && typeof(MonoBehaviour).IsAssignableFrom(f.FieldType)))
                {
                    Fill(a.FieldType, path + a.Name + "/");
                }
            }


            var methods = Base.GetMethods(type);
            foreach (var method in methods)
            {
                members.Add(new Member(){info = method, path = $"{path}Methods/{method})"});
            }

            foreach (var method in Base.GetMethods(type).Where(a => a.CustomAttributes.Any(b => b.AttributeType.Name.StartsWith("PunRPC"))))
            {
                members.Add(new Member(){info = method, path = $"{path}MethodsRPC/{method})"});
            }
            

            
        }
    
    // private string GetParameters(MethodInfo info)
    // {
    //     var p = "";
    //     var parameters = info.GetParameters();
    //     foreach (var item in parameters)
    //     {
    //         p += item.Name +" "+item.ParameterType + ", ";
    //     }
    //     return p.Trim(' ', ',');
    // }
    
}
#endif