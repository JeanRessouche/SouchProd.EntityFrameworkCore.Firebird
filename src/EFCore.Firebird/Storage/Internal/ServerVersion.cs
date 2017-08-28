// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class ServerVersion
    {
        private static readonly Regex ReVersion = new Regex(@"\d+\.\d+\.?(?:\d+)?");

        public ServerVersion(string versionString)
        {
            Type = versionString.ToLower().Contains("firebird") ? ServerType.Firebird : ServerType.Interbase;
            var semanticVersion = ReVersion.Matches(versionString);
            if (semanticVersion.Count > 0)
                Version = Version.Parse(semanticVersion[0].Value);
            else
                throw new InvalidOperationException($"Unable to determine server version from version string '${versionString}'");
        }

        public readonly ServerType Type;

        public readonly Version Version;
    }

    public enum ServerType
    {
        Firebird,
        Interbase
    }
}
