using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Cake.RoslynAnalyzers
{
	public class CodeAnalyzer : IDisposable
	{
		private List<Diagnostic> allDiagnostics;

		private MSBuildWorkspace workspace;

		private bool disposed = false;

		public CodeAnalyzer()
		{
			this.workspace = MSBuildWorkspace.Create();
			this.allDiagnostics = new List<Diagnostic>();
		}

		public IReadOnlyCollection<Diagnostic> Diagnostics
		{
			get { return this.allDiagnostics.AsReadOnly(); }
		}

		public void LoadSolution(string path)
		{
			var solution = this.workspace.OpenSolutionAsync(path).Result;
			foreach (var project in solution.Projects)
			{
				ProcessProject(project);
			}
		}

		public void LoadProject(string path)
		{
			var project = this.workspace.OpenProjectAsync(path).Result;
			ProcessProject(project);
		}

		protected void ProcessProject(Project project)
		{
			var compilation = project.GetCompilationAsync().Result;
			var analyzers = project.AnalyzerReferences.SelectMany(r => r.GetAnalyzersForAllLanguages()).ToImmutableArray();
			var diagnostics = compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync().Result;
			this.allDiagnostics.AddRange(diagnostics.Where(d => !d.IsSuppressed && d.Severity != DiagnosticSeverity.Hidden));
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					if (this.workspace != null)
					{
						this.workspace.Dispose();
						this.workspace = null;
					}
				}

				this.disposed = true;
			}
		}
	}
}
