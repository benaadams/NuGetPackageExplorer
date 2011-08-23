﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using NuGet;
using NuGetPackageExplorer.Types;

namespace PackageExplorerViewModel.Rules {
    [Export(typeof(IPackageRule))]
    internal class NonAssemblyInsideLibRule : IPackageRule {
        public string Name {
            get {
                return "Non Assembly Files In Lib";
            }
        }

        public IEnumerable<PackageIssue> Check(IPackage package) {
            var allLibFiles = package.GetFilesInFolder("lib");
            var assembliesSet = new HashSet<string>(allLibFiles.Where(IsAssembly), StringComparer.OrdinalIgnoreCase);

            return from path in allLibFiles
                   where !IsAssembly(path) && !IsMatchingPdbOrXml(path, assembliesSet)
                   select CreatePackageIssue(path);
        }

        private static bool IsMatchingPdbOrXml(string path, HashSet<string> assemblies) {
            if (path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase)) {

                string truncatedPath = Path.Combine(
                    Path.GetDirectoryName(path),
                    Path.GetFileNameWithoutExtension(path)) + ".dll";

                return assemblies.Contains(truncatedPath);
            }

            return false;
        }

        private static bool IsAssembly(string path) {
            return path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
        }

        private static PackageIssue CreatePackageIssue(string target) {
            return new PackageIssue(
                PackageIssueLevel.Warning,
                "Incompatible files in lib folder",
                "The file '" + target + "' is not a valid assembly. If it is a XML documentation file or a .pdb file, there is no matching .dll file specified in the same folder.",
                "Either remove this file from 'lib' folder or add a matching .dll for it."
            );
        }
    }
}