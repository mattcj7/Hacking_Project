using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace HackingProject.Tests.EditMode
{
    public sealed class LegacyInputUsageTests
    {
        private static readonly string[] DisallowedTokens =
        {
            "UnityEngine.Input.",
            "Input.GetKeyDown(",
            "Input.GetKey(",
            "Input.GetAxis(",
            "Input.GetButton("
        };

        [Test]
        public void Scripts_DoNotUseLegacyInputApi()
        {
            var scriptsRoot = Path.Combine(Application.dataPath, "Scripts");
            Assert.IsTrue(Directory.Exists(scriptsRoot), $"Scripts folder not found at {scriptsRoot}.");

            var scriptFiles = Directory.GetFiles(scriptsRoot, "*.cs", SearchOption.AllDirectories);
            foreach (var scriptFile in scriptFiles)
            {
                var contents = File.ReadAllText(scriptFile);
                foreach (var token in DisallowedTokens)
                {
                    if (contents.IndexOf(token, StringComparison.Ordinal) >= 0)
                    {
                        Assert.Fail($"Legacy Input API usage found. File: {scriptFile} Token: {token}");
                    }
                }
            }
        }
    }
}
