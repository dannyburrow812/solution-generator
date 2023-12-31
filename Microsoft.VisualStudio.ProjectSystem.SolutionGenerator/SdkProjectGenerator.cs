﻿using System;
using System.IO;
using System.Xml.Linq;

namespace Microsoft.VisualStudio.ProjectSystem.SolutionGeneration;

public sealed class SdkProjectGenerator(
    string? targetFrameworks = null,
    string projectExtension = "csproj",
    string sdk = "Microsoft.NET.Sdk")
    : IProjectGenerator
{
    private static readonly Guid _csprojLibrary = Guid.Parse("9A19103F-16F7-4668-BE54-9A1E7A4F7556");

    public IProject Generate(int projectIndex)
    {
        return new SdkProject(projectIndex, targetFrameworks, _csprojLibrary, projectExtension, sdk);
    }

    private sealed class SdkProject : IProject
    {
        public string ProjectName { get; }
        public string RelativeProjectFilePath { get; }
        public string RelativeProjectPath { get; }
        public Guid ProjectType { get; }
        public Guid ProjectGuid { get; }
        public XDocument ProjectXml { get; }

        public SdkProject(int projectIndex, string? targetFrameworks, Guid projectType, string projectExtension, string sdk)
        {
            ProjectType = projectType;

            ProjectName = $"Project{projectIndex + 1}";
            RelativeProjectPath = $"{ProjectName}";
            RelativeProjectFilePath = Path.Combine(RelativeProjectPath, $"{ProjectName}.{projectExtension}");
            ProjectGuid = Guid.NewGuid();

            ProjectXml = new XDocument(
                new XElement(
                    "Project",
                    new XAttribute("Sdk", sdk),
                    new XComment("This is a generated file"),
                    new XComment("https://github.com/drewnoakes/solution-generator")));

            if (!string.IsNullOrWhiteSpace(targetFrameworks))
            {
                AddProperty(new XElement(!targetFrameworks.Contains(';') ? "TargetFramework" : "TargetFrameworks", targetFrameworks));
            }
        }

        private XElement? _itemGroup;
        private XElement? _propertyGroup;

        public void AddProperty(XElement property)
        {
            if (_propertyGroup == null)
            {
                _propertyGroup = new XElement("PropertyGroup");
                ProjectXml.Root?.Add(_propertyGroup);
            }

            _propertyGroup.Add(property);
        }

        public void AddItem(XElement item)
        {
            if (_itemGroup == null)
            {
                _itemGroup = new XElement("ItemGroup");
                ProjectXml.Root?.Add(_itemGroup);
            }

            _itemGroup.Add(item);
        }
    }
}