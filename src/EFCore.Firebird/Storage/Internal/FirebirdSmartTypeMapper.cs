// Copyright (c) SouchProd. All rights reserved. // TODO: Credits Pomelo Foundation & EFCore
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
    public class FirebirdSmartTypeMapper : FirebirdTypeMapper
    {
        private static readonly FirebirdDateTimeTypeMapping DateTime             = new FirebirdDateTimeTypeMapping("datetime", DbType.DateTime);
        private static readonly FirebirdDateTimeOffsetTypeMapping DateTimeOffset = new FirebirdDateTimeOffsetTypeMapping("datetime", DbType.DateTime);
        private static readonly TimeSpanTypeMapping Time                 = new TimeSpanTypeMapping("time", DbType.Time);
        private static readonly GuidTypeMapping OldGuid                  = new GuidTypeMapping("binary(16)", DbType.Guid);

        private readonly IFirebirdOptions _options;

        public FirebirdSmartTypeMapper(
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
            // OldGuids
            if (_options.ConnectionSettings.OldGuids)
            {
                if (mapping.StoreType == "binary(16)" && mapping.ClrType == typeof(byte[]))
                    return OldGuid;
                if (mapping.StoreType == "char(36)" && mapping.ClrType == typeof(Guid))
                    return OldGuid;
            }

            // SupportsDateTime6
            if (!_options.ConnectionSettings.ServerVersion.SupportsDateTime6)
            {
                if (mapping.StoreType == "datetime(6)" && mapping.ClrType == typeof(DateTime))
                    return DateTime;
                if (mapping.StoreType == "datetime(6)" && mapping.ClrType == typeof(DateTimeOffset))
                    return DateTimeOffset;
                if (mapping.StoreType == "time(6)")
                    return Time;
            }

            return mapping;
        }
        
    }
}
