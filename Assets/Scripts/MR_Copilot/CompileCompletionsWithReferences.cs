using RoslynCSharp.Compiler;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RoslynCSharp;
using TMPro;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

//using static System.Net.Mime.MediaTypeNames;
//using System.Diagnostics;
//using Photon.Pun;
//using Photon.Realtime;
//using System.IO;

public class CompileCompletionsWithReferences : MonoBehaviour
{
    // Private
    private string activeCSharpSource = null;
    private ScriptProxy proxy = null;
    private ScriptDomain domain = null;

    // Public
    /// <summary>
    /// The code generated by codex
    /// </summary>
    public GameObject completionText;
    public GameObject terrainCompletionText;

    public string dynamicScriptName;
    public bool defaultSaveScript = true;



    public AssemblyReferenceAsset[] assemblyReferences;

    List<ScriptAssembly> assemblyRuntimeReferences = new List<ScriptAssembly>();


    private const string sourceCodeA = @"
        using UnityEngine;
        public class Example
        {
            public void LogToConsole(string arg)
            {
                Debug.Log(arg);
            }
        }";

    private const string sourceCodeAboo = @"
        using UnityEngine;
        public class ExampleBoo
        {
            public void LogBoo(string boo)
            {
                Debug.Log(boo);
            }
        }";


    private const string sourceCodeB = @"
        using UnityEngine;
        public class ReferenceExample : MonoBehaviour
        {
            public static void SayHello()
            {
                Example refClass = new Example();
                ExampleBoo booClass = new ExampleBoo();
                refClass.LogToConsole(""Hello World"");
                booClass.LogBoo(""boo"");
            }
            public void Start()
            {
                SayHello();
            }

        }";


    public void Start()
    {
        // Create the domain
        domain = ScriptDomain.CreateDomain("CodexCode", true);

        // Add assembly references
        foreach (AssemblyReferenceAsset reference in assemblyReferences)
        {
            domain.RoslynCompilerService.ReferenceAssemblies.Add(reference);
            
        }
        //string[] files = {"Assets/Scripts/Model.cs", "Assets/Scripts/SketchfabLoader.cs"};
        //domain.CompileAndLoadFiles(files);
        //domain.CompileAndLoadFile("Assets/Scripts/Sketchfab/SketchfabLoader.cs");
        //domain.CompileAndLoadFile("Assets/Scripts/Scripts_test/TestGenCode.cs");
        // Load the template code
        //runCodeInput.text = completionText.text;


        //RunCode();

    }

    public void RunCode()
    {
        // Get the C# code from the input field
        string cSharpSource = completionText.GetComponent<TextMeshPro>().text;
        Debug.Log("C# source is " + cSharpSource);

        // Dont recompile the same code
        if (activeCSharpSource != cSharpSource)
        {

            //try
            {
                // Currently this is not getting the assemblyReferences passed to it - is that a problem? Might have to merge them somehow...
                ScriptAssembly assembly = domain.CompileAndLoadSource(cSharpSource, ScriptSecurityMode.UseSettings, assemblyRuntimeReferences.ToArray());


                //add this compiled assembly for future references
                assemblyRuntimeReferences.Add(assembly);

                // Compile code
                //ScriptType type = domain.CompileAndLoadMainSource(cSharpSource, ScriptSecurityMode.UseSettings, assemblyReferences);

                ScriptType[] types = assembly.FindAllTypes();
                foreach (ScriptType type in types)
                {

                    if (type == null)
                    {
                        if (domain.RoslynCompilerService.LastCompileResult.Success == false)
                            throw new Exception("Codex code contained errors. Please fix and try again");
                        else if (domain.SecurityResult.IsSecurityVerified == false)
                            throw new Exception("Codex code failed code security verification");
                        else
                            throw new Exception("Codex code does not define a class. You must include one class definition of any name that inherits from 'RoslynCSharp.Example.MazeCrawler'"); // change this
                    }
                    //Now instantiate it
                    proxy = type.CreateInstance(gameObject);

                }
                if (defaultSaveScript) { 
                    bool done = WriteScriptFile(cSharpSource);
                    Debug.Log("write script file: " + done);
                }
                //Finally set the current code source to the active one, to stop accidentally compiling the same code twice                                
                activeCSharpSource = cSharpSource;


            }
            //catch (Exception e)
            //{
            //    // Show the code editor window
            //    codeEditorWindow.SetActive(true);
            //    throw e;
            //}
        }
        else
        {
            //Figure this out
        }
    }

    public void RunTerrrainCode()
    {
        // Get the C# code from the input field
        string cSharpSource = terrainCompletionText.GetComponent<TextMeshPro>().text;
        Debug.Log("C# source is " + cSharpSource);

        // Dont recompile the same code
        if (activeCSharpSource != cSharpSource)
        {

            //try
            {
                // Currently this is not getting the assemblyReferences passed to it - is that a problem? Might have to merge them somehow...
                ScriptAssembly assembly = domain.CompileAndLoadSource(cSharpSource, ScriptSecurityMode.UseSettings, assemblyRuntimeReferences.ToArray());


                //add this compiled assembly for future references
                assemblyRuntimeReferences.Add(assembly);

                // Compile code
                //ScriptType type = domain.CompileAndLoadMainSource(cSharpSource, ScriptSecurityMode.UseSettings, assemblyReferences);

                ScriptType[] types = assembly.FindAllTypes();
                foreach (ScriptType type in types)
                {

                    if (type == null)
                    {
                        if (domain.RoslynCompilerService.LastCompileResult.Success == false)
                            throw new Exception("Codex code contained errors. Please fix and try again");
                        else if (domain.SecurityResult.IsSecurityVerified == false)
                            throw new Exception("Codex code failed code security verification");
                        else
                            throw new Exception("Codex code does not define a class. You must include one class definition of any name that inherits from 'RoslynCSharp.Example.MazeCrawler'"); // change this
                    }
                    //Now instantiate it
                    proxy = type.CreateInstance(gameObject);

                }

                //if (defaultSaveScript)
                //{
                //    bool done = WriteScriptFile(cSharpSource);
                //    Debug.Log("write script file: " + done);
                //}

                //Finally set the current code source to the active one, to stop accidentally compiling the same code twice                                
                activeCSharpSource = cSharpSource;


            }
            
        }
        else
        {
            //Figure this out
        }
    }

    public bool WriteScriptFile(string CodeString = null)
    {
        string GPTClassName = "";
        CodeString = string.IsNullOrEmpty(CodeString) ? null : CodeString;
        string[] lines = CodeString.Split('\n'); // Split the string by newline characters into an array of lines
        foreach (string line in lines) // Loop through each line
        {
            if (line.Contains("public class ")) // Check if the line contains the target phrase
            {
                GPTClassName = line.Split(" ")[2];// Print the line using Debug.Log
                break; // Exit the loop, assuming there is only one line with the phrase
            }
        }

        Debug.Log("Writing Script File...");

        if (GPTClassName == null)
        {
            GPTClassName = "SampleCode";
        }


        string path = Path.Combine("Scripts", "Scripts_Gen", GPTClassName);
        GPTClassName = Path.Combine(Application.dataPath, path);
        GPTClassName = GPTClassName + ".txt";

        File.WriteAllText(GPTClassName, CodeString);
        return true;

    }


    public void LoadCode(string cSharpFile = null)
    {                
        // Currently this is not getting the assemblyReferences passed to it - is that a problem? Might have to merge them somehow...
        ScriptAssembly assembly = domain.CompileAndLoadSource(cSharpFile, ScriptSecurityMode.UseSettings, assemblyRuntimeReferences.ToArray());


        //add this compiled assembly for future references
        assemblyRuntimeReferences.Add(assembly);

        // Compile code
        //ScriptType type = domain.CompileAndLoadMainSource(cSharpSource, ScriptSecurityMode.UseSettings, assemblyReferences);

        ScriptType[] types = assembly.FindAllTypes();
        foreach (ScriptType type in types)
        {

            if (type == null)
            {
                if (domain.RoslynCompilerService.LastCompileResult.Success == false)
                    throw new Exception("Codex code contained errors. Please fix and try again");
                else if (domain.SecurityResult.IsSecurityVerified == false)
                    throw new Exception("Codex code failed code security verification");
                else
                    throw new Exception("Codex code does not define a class. You must include one class definition of any name that inherits from 'RoslynCSharp.Example.MazeCrawler'"); // change this
            }
            //Now instantiate it
            proxy = type.CreateInstance(gameObject);

        }

    }

    

    //public void RunCode()
    //{
    //    // Get the C# code from the input field
    //    string cSharpSource = completionText.GetComponent<TextMeshPro>().text;
    //    Debug.Log("C# source is " + cSharpSource);

    //    // dump the source code into a desginated C# script
    //    // Get the full path of the file by combining the application data path and the file name
    //    string filePath = Path.Combine(Application.dataPath, "Scripts", dynamicScriptName);

    //    // Use the File.WriteAllText method to write the content to the file, overwriting any existing content
    //    //Debug.Log(filePath);
    //    File.WriteAllText(filePath, cSharpSource);



    //    // Dont recompile the same code
    //    if (activeCSharpSource != cSharpSource)
    //    {
    //        // Currently this is not getting the assemblyReferences passed to it - is that a problem? Might have to merge them somehow...
    //        ScriptAssembly assembly = domain.CompileAndLoadSource(cSharpSource, ScriptSecurityMode.UseSettings, assemblyRuntimeReferences.ToArray());


    //        //add this compiled assembly for future references
    //        assemblyRuntimeReferences.Add(assembly);

    //        // Compile code
    //        //ScriptType type = domain.CompileAndLoadMainSource(cSharpSource, ScriptSecurityMode.UseSettings, assemblyReferences);

    //        ScriptType[] types = assembly.FindAllTypes();
    //        foreach (ScriptType type in types)
    //        {

    //            if (type == null)
    //            {
    //                if (domain.RoslynCompilerService.LastCompileResult.Success == false)
    //                    throw new Exception("Codex code contained errors. Please fix and try again");
    //                else if (domain.SecurityResult.IsSecurityVerified == false)
    //                    throw new Exception("Codex code failed code security verification");
    //                else
    //                    throw new Exception("Codex code does not define a class. You must include one class definition of any name that inherits from 'RoslynCSharp.Example.MazeCrawler'"); // change this
    //            }
    //            //Now instantiate it
    //            proxy = type.CreateInstance(gameObject);
    //        }

    //        //Finally set the current code source to the active one, to stop accidentally compiling the same code twice                                
    //        activeCSharpSource = cSharpSource;
    //    }
    //}


    public void Test()
    {
        

        // Compile and load the first batch of source code.
        // The public types in this source code will be accessible to any assemblies that reference it.
        ScriptAssembly assemblyA = domain.CompileAndLoadSource(sourceCodeA, ScriptSecurityMode.UseSettings, assemblyRuntimeReferences.ToArray());

        ScriptAssembly assemblyAboo = domain.CompileAndLoadSource(sourceCodeAboo, ScriptSecurityMode.UseSettings, assemblyRuntimeReferences.ToArray());


        assemblyRuntimeReferences.Add(assemblyA);
        assemblyRuntimeReferences.Add(assemblyAboo);


        // Compile and load the second batch of source code.
        // Note that we pass 'assemblyA' as part of the third argument 'additionalAssemblyReferences'. This will allow the code we are compiling to access any public types defined in assemblyA.
        // We could also add many more reference assemblies if required by providing a bigger array of references.
        ScriptAssembly assemblyB = domain.CompileAndLoadSource(sourceCodeB, ScriptSecurityMode.UseSettings, assemblyRuntimeReferences.ToArray());

        ScriptType typeB = assemblyB.MainType;

        ScriptType[] types = assemblyB.FindAllTypes();
        foreach (ScriptType type in types)
        {
            Debug.Log("Found type");
            //proxy = type.CreateInstance(gameObject);
        }

        proxy = typeB.CreateInstance(gameObject);

        // Call the static method 'SayHello' which will call the method 'LogToConsole' which is defined in assemblyA
        //assemblyB.MainType.SafeCallStatic("SayHello");

        // Note that there are many other ways to add assembly references. 
        // Any type that implements 'IMetadataAssemblyProvider' can be passed including RoslynCSharp.ScriptAssembly, and RoslynCSharp.Compiler.CompilationResult.
        // You can also use the 'RoslynCSharp.Compiler.AssemblyReference' type to reference other assemblies in a few different ways. All of the following AssemblyReference method calls return an IMetadataReferenceProvider value.

        // Get a metadata reference from a System.Reflection.Assembly which is already loaded. Note that the 'Location' property of the assembly cannot be empty otherwise this will fail.
        AssemblyReference.FromAssembly(typeof(object).Assembly);
                
    }


    /// <summary>
    /// Main run method.
    /// This causes any modified code to be recompiled and executed.
    /// </summary>
    public void RunCodeOld()
    {
        // Get the C# code from the input field
        string cSharpSource = completionText.GetComponent<TextMeshPro>().text;
        Debug.Log("C# source is " + cSharpSource);

        // Dont recompile the same code
        if (activeCSharpSource != cSharpSource)
        {

            //try
            {
                // Compile code
                ScriptType type = domain.CompileAndLoadMainSource(cSharpSource, ScriptSecurityMode.UseSettings, assemblyReferences);

                // Check for null
                if (type == null)
                {
                    if (domain.RoslynCompilerService.LastCompileResult.Success == false)
                        throw new Exception("Codex code contained errors. Please fix and try again");
                    else if (domain.SecurityResult.IsSecurityVerified == false)
                        throw new Exception("Codex code failed code security verification");
                    else
                        throw new Exception("Codex code does not define a class. You must include one class definition of any name that inherits from 'RoslynCSharp.Example.MazeCrawler'"); // change this
                }

                // Check for base class
                //if (type.IsSubTypeOf<MazeCrawler>() == false)
                //    throw new Exception("Maze crawler code must define a single type that inherits from 'RoslynCSharp.Example.MazeCrawler'");




                // Create an instance
                proxy = type.CreateInstance(gameObject);
                activeCSharpSource = cSharpSource;


            }
            //catch (Exception e)
            //{
            //    // Show the code editor window
            //    codeEditorWindow.SetActive(true);
            //    throw e;
            //}
        }
        else
        {
            //Figure this out
        }
    }
}