using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.Disassembler;
using System.Threading;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using ICSharpCode.Decompiler.DebugInfo;
using ICSharpCode.Decompiler.PdbProvider;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
// ReSharper disable All

namespace ICSharpCode.Decompiler.Console
{
	[Command(Name = "ilspycmd", Description = "dotnet tool for decompiling .NET assemblies and generating portable PDBs",
		ExtendedHelpText = @"
Remarks:
  -o is valid with every option and required when using -p.
")]
	[HelpOption("-h|--help")]
	[ProjectOptionRequiresOutputDirectoryValidation]
	class ILSpyCmdProgram
	{
		public static int Main(string[] args) => CommandLineApplication.Execute<ILSpyCmdProgram>(args);

		[FileExists]
		[Required]
		[Argument(0, "Assembly file name", "The assembly that is being decompiled. This argument is mandatory.")]
		public string InputAssemblyName { get; }

		[DirectoryExists]
		[Option("-o|--outputdir <directory>", "The output directory, if omitted decompiler output is written to standard out.", CommandOptionType.SingleValue)]
		public string OutputDirectory { get; }

		[Option("-b|--basedir <directory>", "Directory which is used as the base for all build output paths.", CommandOptionType.SingleValue)]
		public string ProjectRootDirectory { get; }

		[Option("-p|--project", "Decompile assembly as compilable project. This requires the output directory option.", CommandOptionType.NoValue)]
		public bool CreateCompilableProjectFlag { get; }

		[Option("-t|--type <type-name>", "The fully qualified name of the type to decompile.", CommandOptionType.SingleValue)]
		public string TypeName { get; }

		[Option("-n|--project-name <project-name>", "Override the name of the project produced.", CommandOptionType.SingleValue)]
		public string ProjectName { get; }

		[Option("-F|--target-framework <framework-moniker>", "Override the detected target framework.", CommandOptionType.SingleValue)]
		public string TargetFramework { get; }

		[Option("-s|--indent <number>", "Switches indentation to <number> spaces per level.", CommandOptionType.SingleValue)]
		public int IndentationLevel { get; } = 0;

		[Option("-K|--clean", "Remove old output directory, if it exists.", CommandOptionType.NoValue)]
		public bool RemoveOldOutputDir { get; }

		[Option("-U|--unity", "Activate Unity-specific behavior.", CommandOptionType.NoValue)]
		public bool UnityFlag { get; private set; }

		[Option("-2|--twopass", "Parse two-pass assembly.", CommandOptionType.NoValue)]
		public bool TwoPassFlag { get; private set; }

		[Option("-il|--ilcode", "Show IL code.", CommandOptionType.NoValue)]
		public bool ShowILCodeFlag { get; }

		[Option("--il-sequence-points", "Show IL with sequence points. Implies -il.", CommandOptionType.NoValue)]
		public bool ShowILSequencePointsFlag { get; }

		[Option("-genpdb", "Generate PDB.", CommandOptionType.NoValue)]
		public bool CreateDebugInfoFlag { get; }

		[FileExistsOrNull]
		[Option("-usepdb", "Use PDB.", CommandOptionType.SingleOrNoValue)]
		public (bool IsSet, string Value) InputPDBFile { get; }

		[Option("-l|--list <entity-type(s)>", "Lists all entities of the specified type(s). Valid types: c(lass), i(nterface), s(truct), d(elegate), e(num)", CommandOptionType.MultipleValue)]
		public string[] EntityTypes { get; } = new string[0];

		[Option("-v|--version", "Show version of ICSharpCode.Decompiler used.", CommandOptionType.NoValue)]
		public bool ShowVersion { get; }

		[Option("-lv|--languageversion <version>", "C# Language version: CSharp1, CSharp2, CSharp3, CSharp4, CSharp5, CSharp6, CSharp7_0, CSharp7_1, CSharp7_2, CSharp7_3, CSharp8_0 or Latest", CommandOptionType.SingleValue)]
		public LanguageVersion LanguageVersion { get; } = LanguageVersion.Latest;

		[DirectoryExists]
		[Option("-r|--referencepath <path>", "Path to a directory containing dependencies of the assembly that is being decompiled.", CommandOptionType.MultipleValue)]
		public string[] ReferencePaths { get; } = new string[0];

		[DirectoryExists]
		[Option("-R|--referencepath-prepend <path>", "Same as --referencepath, but directory is prepended to the search path.", CommandOptionType.MultipleValue)]
		public string[] PrependReferencePaths { get; } = new string[0];

		[Option("--no-dead-code", "Remove dead code.", CommandOptionType.NoValue)]
		public bool RemoveDeadCode { get; }

		[Option("--no-dead-stores", "Remove dead stores.", CommandOptionType.NoValue)]
		public bool RemoveDeadStores { get; }

		private int OnExecute(CommandLineApplication app)
		{
			TextWriter output = System.Console.Out;
			bool outputDirectorySpecified = !string.IsNullOrEmpty(OutputDirectory);
            string outputName =
                !string.IsNullOrEmpty(TypeName) ? TypeName :
                !string.IsNullOrEmpty(ProjectName) ? ProjectName :
				Path.GetFileNameWithoutExtension(InputAssemblyName);
			try {
				if (CreateCompilableProjectFlag) {
					if (TwoPassFlag)
					{
						DecompileProjectFirstPass(InputAssemblyName, OutputDirectory, ProjectRootDirectory, ProjectName);
					}
					return DecompileAsProject(InputAssemblyName, OutputDirectory, ProjectRootDirectory, ProjectName);
				} else if (EntityTypes.Any()) {
					var values = EntityTypes.SelectMany(v => v.Split(',', ';')).ToArray();
					HashSet<TypeKind> kinds = TypesParser.ParseSelection(values);
					if (outputDirectorySpecified) {
						output = File.CreateText(Path.Combine(OutputDirectory, outputName) + ".list.txt");
					}

					return ListContent(InputAssemblyName, output, kinds);
				} else if (ShowILCodeFlag || ShowILSequencePointsFlag) {
					if (outputDirectorySpecified) {
						output = File.CreateText(Path.Combine(OutputDirectory, outputName) + ".il");
					}

					return ShowIL(InputAssemblyName, output, TypeName);
				} else if (CreateDebugInfoFlag) {
					string pdbFileName = null;
					if (outputDirectorySpecified) {
						pdbFileName = Path.Combine(OutputDirectory, outputName) + ".pdb";
					} else {
						pdbFileName = Path.ChangeExtension(InputAssemblyName, ".pdb");
					}

					return GeneratePdbForAssembly(InputAssemblyName, pdbFileName, app);
				} else if (ShowVersion) {
					string vInfo = "ilspycmd: " + typeof(ILSpyCmdProgram).Assembly.GetName().Version.ToString() +
					               Environment.NewLine
					               + "ICSharpCode.Decompiler: " +
					               typeof(FullTypeName).Assembly.GetName().Version.ToString();
					output.WriteLine(vInfo);
				} else {
					if (outputDirectorySpecified) {
						output = File.CreateText(Path.Combine(OutputDirectory, outputName) + ".decompiled.cs");
					}

					return Decompile(InputAssemblyName, output, TypeName);
				}
			} catch (Exception ex) {
				app.Error.WriteLine(ex.ToString());
				return ProgramExitCodes.EX_SOFTWARE;
			} finally {
				output.Close();
			}

			return 0;
		}

		DecompilerSettings GetSettings()
		{
			var settings = new DecompilerSettings(LanguageVersion) {
				ThrowOnAssemblyResolveErrors = false,
				RemoveDeadCode = RemoveDeadCode,
				RemoveDeadStores = RemoveDeadStores,
				ForceTargetFramework = TargetFramework,
				UnityFlag = UnityFlag
			};
			if (IndentationLevel > 0)
			{
				settings.CSharpFormattingOptions.IndentationString = new string(' ', IndentationLevel);
			}
			return settings;
		}

		CSharpDecompiler GetDecompiler(string assemblyFileName)
		{
			var module = new PEFile(assemblyFileName);
			var resolver = new UniversalAssemblyResolver(assemblyFileName, false, module.Reader.DetectTargetFrameworkId());
			foreach (var path in ReferencePaths) {
				resolver.AddSearchDirectory(path);
			}
			for (int i = PrependReferencePaths.Length; i > 0;)
			{
				resolver.AddSearchDirectory(PrependReferencePaths[--i], true);
			}
			return new CSharpDecompiler(assemblyFileName, resolver, GetSettings()) {
				DebugInfoProvider = TryLoadPDB(module)
			};
		}

		int ListContent(string assemblyFileName, TextWriter output, ISet<TypeKind> kinds)
		{
			CSharpDecompiler decompiler = GetDecompiler(assemblyFileName);

			foreach (var type in decompiler.TypeSystem.MainModule.TypeDefinitions) {
				if (!kinds.Contains(type.Kind))
					continue;
				output.WriteLine($"{type.Kind} {type.FullName}");
			}
			return 0;
		}

		int ShowIL(string assemblyFileName, TextWriter output, string typeName = null)
		{
			var module = new PEFile(assemblyFileName);
			output.WriteLine($"// IL code: {module.Name}");
			var disassembler = new ReflectionDisassembler(new PlainTextOutput(output), CancellationToken.None)
			{
				DebugInfo = TryLoadPDB(module),
				ShowSequencePoints = ShowILSequencePointsFlag,
			};
			disassembler.WriteModuleContents(module, typeName);
			return 0;
		}

		int DecompileProjectFirstPass(string assemblyFilename, string outputDirectory, string projectRoot, string projectName = null)
		{
			if (Path.GetExtension(assemblyFilename).ToLowerInvariant() == ".dll")
			{
				string firstPassAssemblyFilename = Path.ChangeExtension(assemblyFilename, null) + "-firstpass.dll";
				if (File.Exists(firstPassAssemblyFilename))
				{
					DecompileAsProject(firstPassAssemblyFilename, outputDirectory, projectRoot,
						string.IsNullOrEmpty(projectName) ? projectName : projectName + "-firstpass");
				}
			}
			return 0;
		}

		int DecompileAsProject(string assemblyFileName, string outputDirectory, string projectRoot, string projectName = null)
		{
			var module = new PEFile(assemblyFileName);
			var resolver = new UniversalAssemblyResolver(assemblyFileName, false, module.Reader.DetectTargetFrameworkId());
			foreach (var path in ReferencePaths) {
				resolver.AddSearchDirectory(path);
			}
			for (int i = PrependReferencePaths.Length; i > 0;)
			{
				resolver.AddSearchDirectory(PrependReferencePaths[--i], true);
			}
			var decompiler = new WholeProjectDecompiler(GetSettings(), resolver, resolver, TryLoadPDB(module));
			decompiler.DecompileNamedProject(module, outputDirectory, projectRoot, projectName);
			return 0;
		}

		int Decompile(string assemblyFileName, TextWriter output, string typeName = null)
		{
			CSharpDecompiler decompiler = GetDecompiler(assemblyFileName);

			if (typeName == null) {
				output.Write(decompiler.DecompileWholeModuleAsString());
			} else {
				var name = new FullTypeName(typeName);
				output.Write(decompiler.DecompileTypeAsString(name));
			}
			return 0;
		}

		int GeneratePdbForAssembly(string assemblyFileName, string pdbFileName, CommandLineApplication app)
		{
			var module = new PEFile(assemblyFileName,
				new FileStream(assemblyFileName, FileMode.Open, FileAccess.Read),
				PEStreamOptions.PrefetchEntireImage,
				metadataOptions: MetadataReaderOptions.None);

			if (!PortablePdbWriter.HasCodeViewDebugDirectoryEntry(module)) {
				app.Error.WriteLine($"Cannot create PDB file for {assemblyFileName}, because it does not contain a PE Debug Directory Entry of type 'CodeView'.");
				return ProgramExitCodes.EX_DATAERR;
			}

			using (FileStream stream = new FileStream(pdbFileName, FileMode.OpenOrCreate, FileAccess.Write)) {
				var decompiler = GetDecompiler(assemblyFileName);
				PortablePdbWriter.WritePdb(module, decompiler, GetSettings(), stream);
			}

			return 0;
		}

		IDebugInfoProvider TryLoadPDB(PEFile module)
		{
			if (InputPDBFile.IsSet) {
				if (InputPDBFile.Value == null)
					return DebugInfoUtils.LoadSymbols(module);
				return DebugInfoUtils.FromFile(module, InputPDBFile.Value);
			}

			return null;
		}
	}
}
