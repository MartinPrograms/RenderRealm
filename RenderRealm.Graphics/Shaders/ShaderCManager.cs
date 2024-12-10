using System.ComponentModel.Design;
using System.Security.Cryptography;
using Hexa.NET.Shaderc;
using Hexa.NET.SPIRV.Core;
using Hexa.NET.SPIRVReflect;
using Extensions = RenderRealm.Graphics.Common.Extensions;

namespace RenderRealm.Graphics.Shaders;

public class ShaderCManager
{
    public ShadercCompiler Compiler { get; }
    public ShadercCompileOptions Options { get; private set; }

    public static ShadercCompileOptions DefaultOptions
    {
        get
        {
            var options = Shaderc.CompileOptionsInitialize();
            options.SetOptimizationLevel(ShadercOptimizationLevel.Zero);
            options.SetTargetEnv(ShadercTargetEnv.Opengl, (uint)ShadercEnvVersion.Opengl45);
            options.SetTargetSpirv(ShadercSpirvVersion.Version10);
            return options;
        }
    }

    public ShaderCManager(ShadercCompileOptions options)
    {
        Compiler = Shaderc.CompilerInitialize();
        Options = options;
    }
    
    public ShadercCompilationResult? Compile(string source, string path, ShadercShaderKind kind, string entryPoint,out ShaderReflectionData reflectionData)
    {
        reflectionData = new ShaderReflectionData();
        
        if (!GraphicsContext.CompatibleSpirV)
        {
            // Incompatible platform, return null
            return null;
        }
        
        var result = Compiler.CompileIntoSpv(source, new UIntPtr((uint)source.Length), kind, path, entryPoint, Options);
        
        if (result != null)
        {
            var numWarnings = result.GetNumWarnings();
            var numErrors = result.GetNumErrors();
            if (numWarnings > 0 || numErrors > 0)
            {
                Console.WriteLine($"Shader compilation result: {numWarnings} warnings, {numErrors} errors");
            }
            
            var error = result.GetErrorMessageS();
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"Shader compilation error: {error}");
            }
            
            ExtractReflectionData(result, ref reflectionData); 
        }
        
        return result;
    }

    private unsafe void ExtractReflectionData(ShadercCompilationResult result, ref ShaderReflectionData reflectionData)
    {
        var source = result.GetBytes();
        var len = result.GetLength();
        
        SpvReflectShaderModule reflection;
        var spirvReflect = SPIRVReflect.GetShaderModule(len, source, &reflection);
        if (spirvReflect != SpvReflectResult.Success)
        {
            Console.WriteLine($"SPIRV-Reflect error: {spirvReflect}");
            return;
        }
        
        Console.WriteLine($"Shader reflection: {Extensions.FromPointer(reflection.EntryPointName)}");
        Console.WriteLine($"Shader reflection: {reflection.InputVariableCount} inputs");
        Console.WriteLine($"Shader reflection: {reflection.OutputVariableCount} outputs");
        Console.WriteLine($"Shader reflection: {reflection.DescriptorBindingCount} descriptor sets"); // Could be considered uniform buffers (UBOs)
        
        for (uint i = 0; i < reflection.InputVariableCount; i++)
        {
            SpvReflectResult res = SpvReflectResult.Success;
            var input = SPIRVReflect.GetInputVariable(&reflection, i, &res);
            if (res != SpvReflectResult.Success)
            {
                Console.WriteLine($"SPIRV-Reflect error: {res}");
                continue;
            }
            
            reflectionData.Inputs.Add(Extensions.FromPointer(input->Name));
        }
        
        for (uint i = 0; i < reflection.OutputVariableCount; i++)
        {
            SpvReflectResult res = SpvReflectResult.Success;
            var output = SPIRVReflect.GetOutputVariable(&reflection, i, &res);
            if (res != SpvReflectResult.Success)
            {
                Console.WriteLine($"SPIRV-Reflect error: {res}");
                continue;
            }
            
            reflectionData.Outputs.Add(Extensions.FromPointer(output->Name));
        }

        for (uint i = 0; i < reflection.DescriptorBindingCount; i++)
        {
            var binding = reflection.DescriptorBindings[i];
            
            var members = binding.Block.MemberCount;
            
            var block = new UniformBlock
            {
                Name = Extensions.FromPointer(binding.TypeDescription->TypeName),
                Size = binding.Block.Size,
                Binding = binding.Binding,
                Stride = binding.Block.PaddedSize, // Padded size includes padding to the next block
            };
            
            if (members > 0)
            {
                Console.WriteLine($"Uniform block: {Extensions.FromPointer(binding.TypeDescription->TypeName)}");
                for (uint j = 0; j < members; j++)
                {
                    var member = binding.Block.Members[j];
                    Console.WriteLine($"Uniform member: {Extensions.FromPointer(member.Name)}");
                }
                
                var offset = 0u;
                for (uint j = 0; j < members; j++)
                {
                    var member = binding.Block.Members[j];
                    block.Members.Add(new UniformVariable
                    {
                        Name = Extensions.FromPointer(member.Name),
                        Type = (member.TypeDescription->TypeFlags),
                        Size = member.Size,
                        Offset = offset,
                        Binding = member.SpirvId,
                    });
                    Console.WriteLine($"Uniform member: {Extensions.FromPointer(member.Name)} at offset {offset}");
                    offset += member.Size;
                }
            }
            
            reflectionData.Blocks.Add(block);
        }
        
        SPIRVReflect.DestroyShaderModule(&reflection);
    }
}

public class ShaderReflectionData
{
    public List<string> Inputs { get; } = new List<string>();
    public List<string> Outputs { get; } = new List<string>();
    public List<UniformBlock> Blocks { get; } = new List<UniformBlock>();
}

public class UniformVariable
{
    public string Name { get; set; }
    public SpvReflectTypeFlags Type { get; set; }
    public uint Size { get; set; }
    public uint Offset { get; set; } // Offset from the owner block
    public uint Binding { get; set; }
}

public class UniformBlock
{
    public string Name { get; set; }
    public uint Size { get; set; }
    public uint Binding { get; set; }
    public uint Stride { get; set; } // Until the next block
    
    public List<UniformVariable> Members { get; } = new List<UniformVariable>();
}