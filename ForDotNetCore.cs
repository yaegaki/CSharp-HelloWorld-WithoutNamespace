class Program
{
    public static void Main(string[] args)
    {
        var assembly = typeof(Program).Assembly;
        var assemblyType = assembly.GetType().BaseType;


        /*
        // 型を書けないので変数をnullで初期化できない
        // 絶対にnullが返ってくる引数でGetMethodを呼び出して型推論させる
        var assemblyLoadMethodInfo = assemblyType.GetMethod("XXXX");
        foreach (var method in assemblyType.GetMethods())
        {
            if (method.ToString() == "System.Reflection.Assembly Load(System.Reflection.AssemblyName)")
            {
                assemblyLoadMethodInfo = method;
                break;
            }
        }
        */

        // stringを引数に使うほうならGetMethodでも大丈夫
        var assemblyLoadMethodInfo = assemblyType.GetMethod("Load", new[] { typeof(string) });

        // Instance | NonPublicのフラグ作成
        var bindingFlagsType = assemblyType.Assembly.GetType("System.Reflection.BindingFlags");
        // 既存のEnum(MemberType)からenumの型情報取得
        var enumType = assemblyLoadMethodInfo.MemberType.GetType().BaseType;
        var toObjectMethodInfo = enumType.GetMethod("ToObject", new[] { enumType.GetType().BaseType, typeof(int) });
        const int BINDING_FLAGS_INSTANCE_AND_NONPUBLIC = 36; 
        var instanceAndNonPublic = toObjectMethodInfo.Invoke(null, new object[] { bindingFlagsType, BINDING_FLAGS_INSTANCE_AND_NONPUBLIC });

        var typeType = assemblyLoadMethodInfo.GetType().GetType();
        var getFieldMethodInfo = typeType.GetMethod("GetField", new[] { typeof(string), bindingFlagsType });
        var invocationFlagsField = getFieldMethodInfo.Invoke(assemblyLoadMethodInfo.GetType(), new object[] { "m_invocationFlags", instanceAndNonPublic });

        // invocationFlagsFieldがobjectなのでGet/Setもリフレクション経由
        var filedGetValueMethodInfo = invocationFlagsField.GetType().GetMethod("GetValue", new[] { typeof(object) });
        var filedSetValueMethodInfo = invocationFlagsField.GetType().GetMethod("SetValue", new[] { typeof(object), typeof(object) });

        // INITIALIZEDのフラグ設定
        var originalInvocationFlags = filedGetValueMethodInfo.Invoke(invocationFlagsField, new object[] { assemblyLoadMethodInfo });
        const int INVOCATION_FLAGS_INITIALIZED = 0x00000001;
        var invocationFlags = toObjectMethodInfo.Invoke(null, new object[] { originalInvocationFlags.GetType(), INVOCATION_FLAGS_INITIALIZED });
        filedSetValueMethodInfo.Invoke(invocationFlagsField, new object[] { assemblyLoadMethodInfo, invocationFlags });


        // var consoleType = FindType(assembly, assemblyLoadMethodInfo, "System.Console");
        var consoleAssembly = assemblyLoadMethodInfo.Invoke(null, new[] { "System.Console" });
        var getTypeMethodInfo = consoleAssembly.GetType().GetMethod("GetType", new[] { typeof(string) });
        var consoleType = getTypeMethodInfo.Invoke(consoleAssembly, new[] { "System.Console" });
        var getMethodMethodInfo = consoleType.GetType().GetMethod("GetMethod", new[] { typeof(string), (new []{ typeof(string) }).GetType() });
        var writeLineMethodInfo = getMethodMethodInfo.Invoke(consoleType, new object[] { "WriteLine", new[] { typeof(string) } });
        var invokeMethodInfo = writeLineMethodInfo.GetType().GetMethod("Invoke", new [] { typeof(object), typeof(object[]) });
        invokeMethodInfo.Invoke(writeLineMethodInfo, new object[] { null, new object[] { "Hello, World!!" } });
    }

    public static object FindType(object assembly, object assemblyLoadMethodInfo, string typeName)
    {
        var getTypeMethodInfo = assembly.GetType().GetMethod("GetType", new[] { typeof(string) });
        var consoleType = getTypeMethodInfo.Invoke(assembly, new[] { typeName });
        if (consoleType != null)
        {
            return consoleType;
        }
        var getReferencedAssembliesMethodInfo = assembly.GetType().GetMethod("GetReferencedAssemblies");
        var assemblies = getReferencedAssembliesMethodInfo.Invoke(assembly, new object[0]) as object[];
        foreach (var refAssembyName in assemblies)
        {
            var invokeMethodInfo = assemblyLoadMethodInfo.GetType().GetMethod("Invoke", new [] { typeof(object), typeof(object[]) });
            var refAssembly = invokeMethodInfo.Invoke(assemblyLoadMethodInfo, new object[] { null, new object[] { refAssembyName } });
            consoleType = FindType(refAssembly, assemblyLoadMethodInfo, typeName);
            if (consoleType != null)
            {
                return consoleType;
            }
        }

        return null;
    }
}

