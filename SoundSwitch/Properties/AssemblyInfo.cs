using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SoundSwitch")]
[assembly: AssemblyDescription("Manage the default audio device.")]
[assembly: AssemblyCompany("SoundSwitch")]
[assembly: AssemblyProduct("SoundSwitch")]
[assembly: AssemblyCopyright("Antoine Aflalo")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("84f4ff94-6c07-4a07-8772-b333693762c2")]

#if BETA
[assembly: AssemblyConfiguration("Beta")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion("3.15.1.*")]
