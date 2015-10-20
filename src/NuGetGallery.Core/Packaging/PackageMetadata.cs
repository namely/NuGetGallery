// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging;
using NuGet.Versioning;

namespace NuGetGallery.Packaging
{
    public class PackageMetadata
    {
        private readonly Dictionary<string, string> _metadata;
        private readonly IReadOnlyCollection<PackageDependencyGroup> _dependencyGroups;
        private readonly IReadOnlyCollection<FrameworkSpecificGroup> _frameworkReferenceGroups;

        public PackageMetadata(
            Dictionary<string, string> metadata,
            IEnumerable<PackageDependencyGroup> dependencyGroups, 
            IEnumerable<FrameworkSpecificGroup> frameworkGroups)
        {
            _metadata = new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase);
            _dependencyGroups = dependencyGroups.ToList().AsReadOnly();
            _frameworkReferenceGroups = frameworkGroups.ToList().AsReadOnly();

            SetPropertiesFromMetadata();
        }

        private void SetPropertiesFromMetadata()
        {
            Id = GetValueOr("id", string.Empty);
            
            NuGetVersion nugetVersion;
            if (NuGetVersion.TryParse(GetValueOr("version", string.Empty), out nugetVersion))
            {
                Version = nugetVersion;
            }
            if (NuGetVersion.TryParse(GetValueOr("minClientVersion", string.Empty), out nugetVersion))
            {
                MinClientVersion = nugetVersion;
            }

            IconUrl = GetUriOr("iconUrl", null);
            ProjectUrl = GetUriOr("projectUrl", null);
            LicenseUrl = GetUriOr("licenseUrl", null);
            Copyright = GetValueOr("copyright", null);
            Description = GetValueOr("description", null);
            ReleaseNotes = GetValueOr("releaseNotes", null);
            RequireLicenseAcceptance = GetBoolOr("requireLicenseAcceptance", false);
            Summary = GetValueOr("summary", null);
            Title = GetValueOr("title", null);
            Tags = GetValueOr("tags", null);
            Language = GetValueOr("language", null);

            Owners = GetValueOr("owners", null);

            var authorsString = GetValueOr("authors", Owners ?? string.Empty);
            Authors = new List<string>(authorsString.Split(','));
        }

        public string Id { get; private set; }
        public NuGetVersion Version { get; private set; }

        public Uri IconUrl { get; private set; }
        public Uri ProjectUrl { get; private set; }
        public Uri LicenseUrl { get; private set; }
        public string Copyright { get; private set; }
        public string Description { get; private set; }
        public string ReleaseNotes { get; private set; }
        public bool RequireLicenseAcceptance { get; private set; }
        public string Summary { get; private set; }
        public string Title { get; private set; }
        public string Tags { get; private set; }
        public List<string> Authors { get; private set; }
        public string Owners { get; private set; }
        public string Language { get; private set; }
        public NuGetVersion MinClientVersion { get; set; }

        public string GetValueFromMetadata(string key)
        {
            return GetValueOr(key, null);
        }

        public IReadOnlyCollection<PackageDependencyGroup> GetDependencyGroups()
        {
            return _dependencyGroups;
        }

        public IReadOnlyCollection<FrameworkSpecificGroup> GetFrameworkReferenceGroups()
        {
            return _frameworkReferenceGroups;
        }

        private string GetValueOr(string key, string alternateValue)
        {
            string value;
            if (_metadata.TryGetValue(key, out value))
            {
                return value;
            }

            return alternateValue;
        }

        private bool GetBoolOr(string key, bool alternateValue)
        {
            var value = GetValueOr(key, alternateValue.ToString());

            return bool.Parse(value);
        }

        private Uri GetUriOr(string key, Uri alternateValue)
        {
            var value = GetValueOr(key, null);
            if (!string.IsNullOrEmpty(value))
            {
                return new Uri(value);
            }

            return alternateValue;
        }

        public static PackageMetadata FromNuspecReader(NuspecReader nuspecReader)
        {
            return new PackageMetadata(
                nuspecReader.GetMetadata().ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                nuspecReader.GetDependencyGroups(),
                nuspecReader.GetFrameworkReferenceGroups()
           );
        }
    }
}