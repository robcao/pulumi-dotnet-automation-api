using Microsoft.Extensions.Logging;
using Pulumi.Automation;
using Pulumi.Random;

using ILoggerFactory loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());

ILogger logger = loggerFactory.CreateLogger<Program>();

PulumiFn program = PulumiFn.Create(() =>
{
	RandomInteger resource = new(
		"my-resource",
		new RandomIntegerArgs() { Min = 0, Max = 100 });
});

InlineProgramArgs stackArgs = new("repro", "example", program)
{
	Logger = logger,
	EnvironmentVariables = new Dictionary<string, string?>()
	{
		["PULUMI_BACKEND_URL"] = "file://~",
		["PULUMI_CONFIG_PASSPHRASE"] = "test"
	}
};

using WorkspaceStack stack = await LocalWorkspace.CreateOrSelectStackAsync(stackArgs).ConfigureAwait(false);

await stack.RefreshAsync(new RefreshOptions()
{
	LogFlow = true,
	OnStandardError = str => logger.LogError(str),
	OnStandardOutput = str => logger.LogInformation(str)
}).ConfigureAwait(false);

await stack.UpAsync(new UpOptions()
{
	Logger = logger,
	OnStandardError = str => logger.LogError(str),
	OnStandardOutput = str => logger.LogInformation(str)
}).ConfigureAwait(false);
