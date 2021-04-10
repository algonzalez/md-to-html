using System.IO;
using System.Linq;
using Sharprompt;
using static System.Console;
using static Bullseye.Targets;
using static SimpleExec.Command;

namespace build
{
    class Program
    {
        const string BUILD_SCRIPT_DIR = "./scripts/build";
        const string OUTPUT_DIR = "./dist";
        const string PROJECT_DIR = "./src";

        const string DEFAULT_RUNTIME = "any";

        const string DEBUG_CONFIG = "Debug";
        const string RELEASE_CONFIG = "Release";

        static readonly string PROJECT = $"{PROJECT_DIR}/md2html.csproj";

        static bool _isSingleFile = false;
        static bool _zipFiles = false;

        static void Main(string[] args)
        {
            // save colors to restore at the end
            var (fgColor, bgColor) = (ForegroundColor, BackgroundColor);

            _isSingleFile = CheckOptionAndRemove(ref args, "--single-file");

            _zipFiles = CheckOptionAndRemove(ref args, "--zip");

            if (args.Length > 0) {
                if (new[] { "debug", "rel" }.Any(c => c == args[0]))
                    args[0] = args[0] + ":any";
            }

            Target("clean", "Delete the {project}/bin and {project}/obj directories.",
                    () => DeleteDirs($"{PROJECT_DIR}/bin", $"{PROJECT_DIR}/obj"));

            Target("clean:all", "Calls clean and then also deletes the /dist directory",
                DependsOn("clean"),
                () => DeleteDirs(OUTPUT_DIR));

            Target("default", "Runs the rel:any target", DependsOn("rel:any"));

            Target("debug:all", $"Builds all debug targets: any, linux, macos & win10",
                DependsOn("clean:all", "debug:any", "debug:linux", "debug:macos", "debug:win10"));

            Target("debug:any", "Debug release that should run on any platform where .NET 5 runtime is installed.",
                DependsOn("clean"),
                () => Publish(DEFAULT_RUNTIME, DEBUG_CONFIG));

            Target("debug:linux", "Self contained debug release targeting Linux.",
                DependsOn("clean"),
                () => Publish("linux-x64", DEBUG_CONFIG));

            Target("debug:macos", $"Self contained debug release targeting Apple macOS.",
                DependsOn("clean"),
                () => Publish("osx-x64", DEBUG_CONFIG));

            Target("debug:win10", "Self contained debug release targeting Microsoft Windows 10.",
                DependsOn("clean"),
                () => Publish("win10-x64", DEBUG_CONFIG));

            Target("prompt", "Interactive mode that will ask for build options.", () => {
                var config = Prompt.Select("Select build configuration"
                    , new[] { DEBUG_CONFIG, RELEASE_CONFIG }
                    , defaultValue: DEBUG_CONFIG);
                var runtime = Prompt.Select("Select target runtime"
                    , new[] { DEFAULT_RUNTIME, "linux-x64", "macos-x64", "win10-x64" }
                    , defaultValue: DEFAULT_RUNTIME);
                _isSingleFile = (runtime != DEFAULT_RUNTIME)
                    && Prompt.Confirm("Set the PublishSingleFile property to true?", defaultValue: false);
                _zipFiles = Prompt.Confirm("Move files to a zip file after the build?", false);
                if (Prompt.Confirm("Proceed with above values?", defaultValue: true)) {
                    Publish(runtime, config);
                }
            });

            Target("rel:all", $"Builds all release targets: any, linux, macos & win10",
                DependsOn("clean:all", "rel:any", "rel:linux", "rel:macos", "rel:win10"));

            Target("rel:any", "Release that should run on any platform where .NET 5 runtime is installed.",
                DependsOn("clean"),
                () => Publish(DEFAULT_RUNTIME, RELEASE_CONFIG));

            Target("rel:linux", "Self contained release targeting Linux.",
                DependsOn("clean"),
                () => Publish("linux-x64", RELEASE_CONFIG));

            Target("rel:macos", $"Self contained release targeting Apple macOS.",
                DependsOn("clean"),
                () => Publish("osx-x64", RELEASE_CONFIG));

            Target("rel:win10", "Self contained release targeting Microsoft Windows 10.",
                DependsOn("clean"),
                () => Publish("win10-x64", RELEASE_CONFIG));

            Target("tool:check", "Checks packages and lists those that have a newer version,\n               have been deprecated or have known vulnerabilities.",
                () => {
                    string[] checkPackageOptions = new[] {
                        "--outdated", "--deprecated", "--vulnerable --include-transitive"
                    };

                    void RunChecks(string prjDir)
                    {
                        foreach (var option in checkPackageOptions) {
                            WriteLine($"\n>>> Checking for {option.Split(' ')[0][2..]} packages");
                            Run("dotnet", $"list {prjDir} package {option}");
                        }
                    };

                    WriteLine(">>> ==============================");
                    WriteLine(">>> Checking build script packages");
                    WriteLine(">>> ==============================");
                    RunChecks(BUILD_SCRIPT_DIR);

                    WriteLine();
                    WriteLine(">>> =========================");
                    WriteLine(">>> Checking project packages");
                    WriteLine(">>> =========================");
                    RunChecks(PROJECT_DIR);
                }
            );

            Target("tool:format", "Calls dotnet-format to enforce format rules in the .editorconfig file.",
                () => {
                    try {
                        Run("dotnet", $"format {PROJECT}");
                    }
                    catch (SimpleExec.NonZeroExitCodeException) {
                        WriteLine("Target requires the dotnet-format tool.");
                        WriteLine("Install it by running: dotnet tool install --global dotnet-format");
                    }
                });

            if (args.HasAny(new[] { "-h", "--help", "-?", "-l", "--list-targets" }, ignoreCase: true)) {
                RunTargetsWithoutExiting(args);
                // display additional usage details and features
                WriteLine();
                WriteLine("NOTES:");
                WriteLine("  - linux, macos & win10 targets build as --self-contained");
                WriteLine("  - use --single-file option to set the PublishSingleFile property to true;\n    available for linux, macos & win10 targets");
                WriteLine("  - use --zip option to generate a compressed .zip file of the release");
                RestoreColors();
                System.Environment.Exit(0);
            }

            try {
                RunTargetsWithoutExiting(args);
            }
            finally {
                RestoreColors();
            }

            static bool CheckOptionAndRemove(ref string[] args, string optionName)
            {
                bool found = args.Has(optionName, ignoreCase: true);
                if (found) {
                    args = args.Remove(optionName, ignoreCase: true);
                }
                return found;
            }

            void RestoreColors()
            {
                ForegroundColor = fgColor;
                BackgroundColor = bgColor;
            }
        }

        static void DeleteDirs(params string[] dirs)
        {
            foreach (var dir in dirs) {
                if (Directory.Exists(dir)) {
                    Directory.Delete(dir, recursive: true);
                }
            }
        }

        static void Publish(string runtime, string configuration = DEBUG_CONFIG)
        {
            string outputDir = $"{OUTPUT_DIR}/{(configuration == DEBUG_CONFIG ? "debug/" : "")}{runtime}";

            DeleteDirs(outputDir);

            if (runtime == DEFAULT_RUNTIME) {
                Run("dotnet", $"publish -c {configuration} -o {outputDir} {PROJECT}");
            }
            else {
                Run("dotnet", $"publish -c {configuration} -o {outputDir} -r {runtime} -p:PublishSingleFile={_isSingleFile} {PROJECT}");
            }

            if (_zipFiles) {
                string zipfilePath = Path.Combine(OUTPUT_DIR, Path.GetFileNameWithoutExtension(PROJECT))
                    + $"{(configuration == DEBUG_CONFIG ? "-debug" : "")}-{runtime}.zip";

                if (File.Exists(zipfilePath)) {
                    File.Delete(zipfilePath);
                }

                System.IO.Compression.ZipFile.CreateFromDirectory(outputDir, zipfilePath
                    , System.IO.Compression.CompressionLevel.Optimal
                    , includeBaseDirectory: false);

                WriteLine($"  {Path.GetFileNameWithoutExtension(PROJECT)} -> {zipfilePath}");
                DeleteDirs(outputDir);
            }
        }
    }

    public static class ExtensionMethods
    {
        public static bool Has(this string[] array, string value, bool ignoreCase = false)
            => array.Any(e => string.Compare(e, value, ignoreCase: ignoreCase) == 0);

        public static bool HasAny(this string[] array, string[] values, bool ignoreCase = false)
            => array.Any(e => values.Has(e, ignoreCase));

        public static string[] Remove(this string[] array, string value, bool ignoreCase = false)
            => array.Where(e => string.Compare(e, value, ignoreCase: ignoreCase) != 0).ToArray();
    }
}
