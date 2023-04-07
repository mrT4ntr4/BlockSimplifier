using CommandLine;
using de4dot.blocks;
using de4dot.blocks.cflow;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSimplifier
{
    internal class Program
    {
        public class CommandLineOptions
        {
            [Option('f', "filename", Required = true, HelpText = "File to deobfuscate")]
            public string FileName { get; set; }
            [Option('t', "tokens", Required = false, HelpText = "Metadata Tokens of methods to simplify (hex) | eg. -t \"0x06000003\" \"0x06000004\"")]
            public IEnumerable<string> Tokens { get; set; }
            [Option('a', "all", Required = false, HelpText = "Simplify all methods")]
            public bool All { get; set; }
        }

        public static void SimplifyMethods(List<MethodDef> methods)
        {
            BlocksCflowDeobfuscator blocksCflowDeobfuscator = new BlocksCflowDeobfuscator();
            for (int i = 0; i < methods.Count; i++)
            {
                Console.WriteLine($"[~] Simplifying {methods[i].FullName}");
                Blocks blocks = new Blocks(methods[i]);
                blocksCflowDeobfuscator.Initialize(blocks);
                blocksCflowDeobfuscator.Deobfuscate();
                blocks.RepartitionBlocks();
                blocks.GetCode(out IList<Instruction> list, out IList<ExceptionHandler> exceptionHandlers);
                DotNetUtils.RestoreBody(methods[i], list, exceptionHandlers);
            }
        }


        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(options =>
            {
                AssemblyDef assemblyDef = AssemblyDef.Load(options.FileName);
                ModuleDef module = assemblyDef.ManifestModule;
                ModuleWriterOptions moduleWriterOptions = new ModuleWriterOptions(module);
                moduleWriterOptions.MetaDataOptions.Flags |= MetaDataFlags.PreserveAll;
                moduleWriterOptions.MetaDataOptions.Flags |= MetaDataFlags.KeepOldMaxStack;

                List<MethodDef> _methods = new List<MethodDef>();
                List<MethodDef> tokenMethods = new List<MethodDef>();

                foreach (TypeDef type in (from x in module.Types where x.HasMethods select x).ToArray())
                {
                    foreach (MethodDef method in (from x in type.Methods where x.HasBody && x.Body.HasInstructions select x).ToArray())
                    {
                        _methods.Add(method);
                    }
                }

                if (options.All)
                {
                    SimplifyMethods(_methods);
                }
                else
                {
                    List<int> intTokens= new List<int>();
                    foreach (var item in options.Tokens)
                    {
                        intTokens.Add(Convert.ToInt32(item, 16));
                    }

                    foreach (MethodDef method in _methods)
                    {
                        if (intTokens.Contains(method.MDToken.ToInt32()))
                        {
                            Console.WriteLine($"[+] Found Method from Metadata Token ({method.MDToken}) : {method.FullName}");
                            tokenMethods.Add(method);
                        }
                    }
                    if(tokenMethods.Count !=0)
                    {
                        SimplifyMethods(tokenMethods);
                    }
                }
                string simplyFilename = string.Concat(new string[] { Path.GetFileNameWithoutExtension(options.FileName), "_Simplified", Path.GetExtension(options.FileName) });
                module.Write(simplyFilename, moduleWriterOptions);
                Console.WriteLine("===>> Simplified File saved at : " + simplyFilename);
            });


        }
    }
}
