using System.Numerics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using ShaderGen;
using ShaderGen.Glsl;

namespace RenderRealm.Graphics.Shaders;

public class ShaderGenManager
{
    public static Shader? GenerateShader(Type source, string code)
    {
        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Vector2).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ShaderSetAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(source.Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ShaderGenManager).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ShaderGen.ShaderGenerator).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ShaderGen.Glsl.Glsl330Backend).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(source.FullName)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(references)
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(code),
                new CSharpParseOptions(LanguageVersion.Latest)));

        var glsl330Backend = new Glsl330Backend(compilation);

        List<IShaderSetProcessor> processors = new List<IShaderSetProcessor>();
        try
        {
            var processor = new TestProcessor();
            processors.Add(processor);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }


        try
        {
            var sg = new ShaderGenerator(compilation, glsl330Backend, $"{source.FullName}.VS",
                $"{source.FullName}.FS", null, processors.ToArray());

            var result = sg.GenerateShaders();

            var output = result.GetOutput(glsl330Backend);
            if (output.Count == 0)
            {
                Console.WriteLine("No shaders generated");
                return null;
            }

            var shader = new Shader(new ShaderStage(output[0].VertexShaderCode, ShaderType.Vertex),
                new ShaderStage(output[0].FragmentShaderCode, ShaderType.Fragment));
            
            sg = null;
            compilation = null;
            return shader;
        }
        catch (Exception e)
        {
            Console.WriteLine("ShaderGen failed: " + e);
            return Shader.ErrorShader;
        }
    }
}

public class TestProcessor : IShaderSetProcessor
{
    public string Result { get; private set; }

    public string UserArgs { get; set; }

    public void ProcessShaderSet(ShaderSetProcessorInput input)
    {
        Result = string.Join(" ", input.Model.AllResources.Select(rd => rd.Name));
    }
}