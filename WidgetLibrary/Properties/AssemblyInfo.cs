using Prysm.AppVision.SDK.Widgets;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("WidgetLibrary")]
[assembly: AssemblyDescription("AppVision SDK sample: a widget library for the synoptic editor toolbox")]
[assembly: AssemblyCompany("Prysm")]
[assembly: AssemblyProduct("AppVision SDK Samples")]
[assembly: AssemblyCopyright("Copyright © Prysm 2026")]
[assembly: ComVisible(false)]

// Version shown in the DLL properties; raise it with each delivery so a site can tell which build it runs.
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// The library node of the toolbox: its name, an optional image (relative path = resource of this DLL) and a tooltip.
[assembly: AppWidgetLibrary("Sample Widgets", Description = "AppVision SDK sample widgets: value tile, state lamp, command button")]
