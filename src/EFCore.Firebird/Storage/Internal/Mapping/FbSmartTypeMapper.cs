// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class FbSmartTypeMapper : FbTypeMapper
    {
        private static readonly FbDateTimeTypeMapping DateTime = new FbDateTimeTypeMapping("timestamp", DbType.DateTime);
        private static readonly TimeSpanTypeMapping Time = new TimeSpanTypeMapping("time", DbType.Time);
        private static readonly GuidTypeMapping Guid = new GuidTypeMapping("char(38)", DbType.Guid);

        private readonly IFirebirdOptions _options;

        public FbSmartTypeMapper(
            [NotNull] RelationalTypeMapperDependencies dependencies,
            [NotNull] IFirebirdOptions options)
            : base(dependencies)
        {
            Check.NotNull(options, nameof(options));
            _options = options;
        }

        public override RelationalTypeMapping FindMapping(IProperty property)
        {
            var mapping = base.FindMapping(property);
            return mapping == null ? null : MaybeConvertMapping(mapping);
        }

        public override RelationalTypeMapping FindMapping(Type clrType)
        {
            var mapping = base.FindMapping(clrType);
            return mapping == null ? null : MaybeConvertMapping(mapping);
        }

        public override RelationalTypeMapping FindMapping(string storeType)
        {
            var mapping = base.FindMapping(storeType);
            return mapping == null ? null : MaybeConvertMapping(mapping);
        }

        protected virtual RelationalTypeMapping MaybeConvertMapping(RelationalTypeMapping mapping)
        {
            if (mapping.StoreType == "char(38)" && mapping.ClrType == typeof(Guid))
                return Guid;

            return mapping;
        }
        
    }
}
