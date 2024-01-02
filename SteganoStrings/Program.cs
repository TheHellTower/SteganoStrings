using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using Leaf.xNet;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace SteganoStrings
{
    internal class Program
    {
        private static ModuleDefMD Module = null;
        private static string StringsResourceName = "THT_Strings", filePath = string.Empty, ClientID = "f6e65da8c7e88bebb109e98426e0161eeb7a7aed976d11cb46b9ce0e487ee2d4",  UnplashAPI_Base = "https://api.unsplash.com/photos/random?client_id={0}&query={1}&count={2}";
        private static string[] Queries = new string[] { "Nature", "Space%20Stars", "Cute%20Animals" };
        private static Stream getRandomImage()
        {
            /* Shuffle + Random Choice */
            Queries.Shuffle();
            string query = Queries.GetRandomString();
            /* Shuffle + Random Choice */
            using (HttpRequest HR = new HttpRequest())
            {
                var response = HR.Get(string.Format(UnplashAPI_Base, ClientID, query, 1)).ToString();
                response = response.Remove(0, 1).Remove(response.Length - 2, 1); //[] Isn't parseable directly using JObject
                JObject JSON = JObject.Parse(response);
                byte[] Image = new System.Net.WebClient().DownloadData(JSON["urls"]["raw"].ToString());
                Stream ImageAsStream = new MemoryStream(Image);

                return ImageAsStream;
            }
        }

        static void Main(string[] args)
        {
            Console.Clear();
            if (args.Length != 0) filePath = args[0];
            while (!File.Exists(filePath))
            {
                Console.WriteLine("File Path: ");
                filePath = Console.ReadLine().Replace("\"", string.Empty);
                Console.Clear();
            }
            Module = ModuleDefMD.Load(filePath);

            Console.WriteLine("Download a random image..");
            Strings(getRandomImage());

            ModuleWriterOptions ModuleWriterOptions = new ModuleWriterOptions(Module) { Logger = DummyLogger.NoThrowInstance };
            ModuleWriterOptions.MetadataOptions.Flags = MetadataFlags.PreserveAll;

            Module.Write(Module.Location.Insert(Module.Location.Length-4, "-Stegano")/*, ModuleWriterOptions*/);
        }

        private static void Strings(Stream stream)
        {
            Importer importer = new Importer(Module);
            IMethod[] Methods = new IMethod[] { importer.Import(typeof(Runtime.Runtime).GetMethod("Initialize", new[] { typeof(string) })), importer.Import(typeof(Runtime.Runtime).GetMethod("GetString", new[] { typeof(int) })) };

            var cctor = Module.GlobalType.FindOrCreateStaticConstructor();
            cctor.Body.Instructions.Insert(0, new Instruction(OpCodes.Call, Methods[0]));
            cctor.Body.Instructions.Insert(0, new Instruction(OpCodes.Ldstr, StringsResourceName));

            string strs = "";

            int index = 0;
            foreach (TypeDef Type in Module.GetTypes().Where(T => T.HasMethods).ToArray())
                foreach (MethodDef Method in Type.Methods.Where(M => !M.IsConstructor && M.HasBody && M.Body.HasInstructions).ToArray()) //We don't want to deal with constructors to void needing to rename types/class and we only take methods that has a body with instructions
                {
                    for (int I = 0; I < Method.Body.Instructions.Count(); I++)
                    {
                        Instruction Instruction = Method.Body.Instructions[I];
                        if (Instruction.OpCode == OpCodes.Ldstr)
                        {
                            string MyString = Instruction.Operand.ToString();
                            strs += @$"{MyString}\_THT_/";

                            Console.WriteLine($"Processing String: \"{MyString}\"");

                            Instruction.OpCode = OpCodes.Call;
                            Instruction.Operand = Methods[1]; //String replaced by our Method

                            Method.Body.Instructions.Insert(I, new Instruction(OpCodes.Ldc_I4, index));
                            index++;
                        }
                    }

                    Method.Body.OptimizeBranches();
                    Method.Body.SimplifyBranches();
                    Method.Body.OptimizeMacros();
                }
            strs = strs.Substring(0, strs.Length - 7);

            MemoryStream memoryStream = new MemoryStream();
            SteganographyHelper.EncodeText(stream, strs).Save(memoryStream, ImageFormat.Png);
            Module.Resources.Add(new EmbeddedResource(StringsResourceName, memoryStream.ToArray(), ManifestResourceAttributes.Private));
        }
    }
}