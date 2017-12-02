using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Cake.RoslynAnalyzers
{
	[CakeAliasCategory("Code Analysis"), CakeAliasCategory("Code Quality")]
	public static class CakeAliases
	{
		[CakeMethodAlias()]
		public static void RoslynAnalyzer(this ICakeContext context, IEnumerable<string> solutionFiles = null, IEnumerable<string> projectFiles = null)
		{
			var codeAnalyzer = new CodeAnalyzer();
			foreach (var path in solutionFiles)
			{
				context.Log.Write(Verbosity.Verbose, LogLevel.Information, $"Analyzing code for solution '${path}'");
				codeAnalyzer.LoadSolution(path);
			}
			foreach (var path in projectFiles)
			{
				context.Log.Write(Verbosity.Verbose, LogLevel.Information, $"Analyzing code for project '${path}'");
				codeAnalyzer.LoadProject(path);
			}

			context.Log.Write(Verbosity.Verbose, LogLevel.Information, $"Code diagnostic results:");
			foreach (var diagnostic in codeAnalyzer.Diagnostics)
			{
				context.Log.Write(Verbosity.Normal, GetLogLevel(diagnostic), $"${diagnostic.Descriptor.Title}: ${diagnostic.Location}: ${diagnostic.GetMessage()}");
			};
		}

		private static LogLevel GetLogLevel(Diagnostic diagnostic)
		{
			switch (diagnostic.Severity)
			{
				case DiagnosticSeverity.Error:
					return LogLevel.Error;
				case DiagnosticSeverity.Warning:
					return diagnostic.IsWarningAsError ? LogLevel.Error : LogLevel.Warning;
				case DiagnosticSeverity.Info:
					return LogLevel.Information;
				default:
					return LogLevel.Verbose;
			}
		}
	}
}
