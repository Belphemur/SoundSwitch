﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SoundSwitch.Localization {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class UpdateDownloadStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal UpdateDownloadStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SoundSwitch.Localization.UpdateDownloadStrings", typeof(UpdateDownloadStrings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cancel.
        /// </summary>
        internal static string cancel {
            get {
                return ResourceManager.GetString("cancel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Changelog.
        /// </summary>
        internal static string changelog {
            get {
                return ResourceManager.GetString("changelog", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Download Failed.
        /// </summary>
        internal static string downloadFailed {
            get {
                return ResourceManager.GetString("downloadFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Install.
        /// </summary>
        internal static string install {
            get {
                return ResourceManager.GetString("install", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The downloaded update isn&apos;t signed with a valid signature. We advise you to delete the file and contact the developer!.
        /// </summary>
        internal static string notSigned {
            get {
                return ResourceManager.GetString("notSigned", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Digital Signature.
        /// </summary>
        internal static string notSignedTitle {
            get {
                return ResourceManager.GetString("notSignedTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Later.
        /// </summary>
        internal static string remindMe {
            get {
                return ResourceManager.GetString("remindMe", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is a problem with the last SoundSwitch update. You&apos;ll need to manually download the new version at {0}.
        /// </summary>
        internal static string wrongSignature {
            get {
                return ResourceManager.GetString("wrongSignature", resourceCulture);
            }
        }
    }
}
