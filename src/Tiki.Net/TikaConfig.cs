using System.Reflection;
using Tiki.Detect;
using Tiki.Parser;
using Tiki.Parser.Parsers;

namespace Tiki;

/// <summary>
/// Configuration for the Tiki facade, managing detectors and parsers.
/// </summary>
public sealed class TikiConfig
{
    private static TikiConfig? s_default;

    public IDetector Detector { get; }
    public AutoDetectParser Parser { get; }

    private TikiConfig(IDetector detector, AutoDetectParser parser)
    {
        Detector = detector;
        Parser = parser;
    }

    /// <summary>
    /// Gets the default configuration that auto-discovers all available parsers.
    /// </summary>
    public static TikiConfig Default => s_default ??= CreateDefault();

    /// <summary>
    /// Creates a new builder for custom configuration.
    /// </summary>
    public static Builder CreateBuilder() => new();

    private static TikiConfig CreateDefault()
    {
        var builder = new Builder();

        // Add built-in parsers
        builder.AddParser(new TextParser());
        builder.AddParser(new XmlParser());

        // Auto-discover parsers from loaded assemblies
        DiscoverParsers(builder);

        return builder.Build();
    }

    private static void DiscoverParsers(Builder builder)
    {
        var parserInterface = typeof(IParser);

        // Force-load any Tiki parser assemblies that are referenced but not yet loaded
        var loadedAssemblyNames = new HashSet<string>(
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName != null)
                .Select(a => a.GetName().Name!));

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().ToArray())
        {
            try
            {
                foreach (var refName in assembly.GetReferencedAssemblies())
                {
                    if (refName.Name?.StartsWith("Tiki") == true && !loadedAssemblyNames.Contains(refName.Name))
                    {
                        try
                        {
                            Assembly.Load(refName);
                            loadedAssemblyNames.Add(refName.Name);
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        // Now scan all loaded Tiki assemblies for parsers
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name?.StartsWith("Tiki") != true)
                continue;

            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface || !parserInterface.IsAssignableFrom(type))
                        continue;

                    // Skip parsers already added (built-in ones)
                    if (type == typeof(TextParser) || type == typeof(XmlParser))
                        continue;

                    if (type.GetConstructor(Type.EmptyTypes) is not null)
                    {
                        var parser = (IParser)Activator.CreateInstance(type)!;
                        builder.AddParser(parser);
                    }
                }
            }
            catch
            {
                // Skip assemblies that can't be reflected
            }
        }
    }

    /// <summary>
    /// Builder for creating custom TikiConfig instances.
    /// </summary>
    public sealed class Builder
    {
        private readonly List<IParser> _parsers = new();
        private readonly List<IDetector> _detectors = new();

        public Builder AddParser(IParser parser)
        {
            _parsers.Add(parser);
            return this;
        }

        public Builder AddDetector(IDetector detector)
        {
            _detectors.Add(detector);
            return this;
        }

        public TikiConfig Build()
        {
            // Set up detectors
            if (_detectors.Count == 0)
            {
                _detectors.Add(new ExtensionDetector());
                _detectors.Add(new MagicBytesDetector());
            }

            var detector = new CompositeDetector(_detectors);
            var compositeParser = new CompositeParser(_parsers);
            var autoDetectParser = new AutoDetectParser(detector, compositeParser);

            return new TikiConfig(detector, autoDetectParser);
        }
    }
}
