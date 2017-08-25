// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class FirebirdTypeMapper : RelationalTypeMapper
    {
        private static readonly Regex TypeRe = new Regex(@"([a-z0-9]+)\s*?(?:\(\s*(\d+)?\s*\))?\s*(unsigned)?", RegexOptions.IgnoreCase);

        // boolean
        private readonly FirebirdBoolTypeMapping _bit       = new FirebirdBoolTypeMapping("smallint", DbType.SByte);

        // integers
        private readonly SByteTypeMapping _tinyint          = new SByteTypeMapping("smallint", DbType.SByte);
        private readonly ByteTypeMapping _utinyint          = new ByteTypeMapping("smallint", DbType.Byte);
	    private readonly ShortTypeMapping _smallint         = new ShortTypeMapping("smallint", DbType.Int16);
	    private readonly UShortTypeMapping _usmallint       = new UShortTypeMapping("smallint", DbType.UInt16);
        private readonly IntTypeMapping _int                = new IntTypeMapping("integer", DbType.Int32);
	    private readonly UIntTypeMapping _uint              = new UIntTypeMapping("integer", DbType.UInt32);
	    private readonly LongTypeMapping _bigint            = new LongTypeMapping("bigint", DbType.Int64);
	    private readonly ULongTypeMapping _ubigint          = new ULongTypeMapping("bigint", DbType.UInt64);

	    // decimals
	    private readonly DecimalTypeMapping _decimal        = new DecimalTypeMapping("decimal(18,4)", DbType.Decimal);
	    private readonly DoubleTypeMapping _double          = new DoubleTypeMapping("DOUBLE PRECISION", DbType.Double);
        private readonly FloatTypeMapping _float            = new FloatTypeMapping("float");

	    // binary
	    private readonly RelationalTypeMapping _binary           = new FirebirdByteArrayTypeMapping("BLOB SUB_TYPE 0 SEGMENT SIZE 80", DbType.Binary);
        private readonly RelationalTypeMapping _varbinary        = new FirebirdByteArrayTypeMapping("BLOB SUB_TYPE 0 SEGMENT SIZE 80", DbType.Binary);
	    private readonly FirebirdByteArrayTypeMapping _varbinary767 = new FirebirdByteArrayTypeMapping("BLOB SUB_TYPE 0 SEGMENT SIZE 80", DbType.Binary, 767);
	    private readonly RelationalTypeMapping _varbinarymax     = new FirebirdByteArrayTypeMapping("BLOB SUB_TYPE 0 SEGMENT SIZE 80", DbType.Binary);

	    // string
        private readonly FirebirdStringTypeMapping _char            = new FirebirdStringTypeMapping("char", DbType.AnsiStringFixedLength);
        private readonly FirebirdStringTypeMapping _varchar         = new FirebirdStringTypeMapping("varchar", DbType.AnsiString);
	    private readonly FirebirdStringTypeMapping _varchar127      = new FirebirdStringTypeMapping("varchar(127)", DbType.AnsiString, true, 127);
	    private readonly FirebirdStringTypeMapping _varcharmax      = new FirebirdStringTypeMapping("varchar(4000)", DbType.AnsiString);

	    // DateTime
        private readonly FirebirdDateTimeOffsetTypeMapping _timeStamp = new FirebirdDateTimeOffsetTypeMapping("TIMESTAMP", DbType.DateTime);
        private readonly TimeSpanTypeMapping _time                       = new TimeSpanTypeMapping("time", DbType.Time);

        // json
        private readonly RelationalTypeMapping _json = new FirebirdStringTypeMapping("BLOB SUB_TYPE 1 SEGMENT SIZE 80", DbType.AnsiString);

        // row version
        private readonly RelationalTypeMapping _rowversion   = new FirebirdDateTimeTypeMapping("TIMESTAMP", DbType.DateTime);

        // guid
        private readonly GuidTypeMapping _uniqueidentifier   = new GuidTypeMapping("varchar(38)", DbType.Guid);

        readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;
        readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;
        private readonly HashSet<string> _disallowedMappings;

        public FirebirdTypeMapper([NotNull] RelationalTypeMapperDependencies dependencies)
            : base(dependencies)
        {
            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                { // TODO
                    // boolean
                    { "bit", _bit },

                    // integers
                   // { "tinyint", _tinyint },
                   // { "tinyint unsigned", _utinyint },
                    { "smallint", _smallint },
                   // { "smallint unsigned", _usmallint },
                   // { "mediumint", _int },
                   // { "mediumint unsigned", _uint },
                    { "integer", _int },
                   // { "int unsigned", _uint },
                    { "bigint", _bigint },
                    //{ "bigint unsigned", _ubigint },

                    // decimals
                    { "decimal(18,4)", _decimal },
                    { "DOUBLE PRECISION", _double },
                    { "float", _float },

                    // TODO
                    // binary
                    { "BLOB SUB_TYPE 0", _binary },
                    //{ "varbinary", _varbinary },
                    //{ "tinyblob", _varbinarymax },
                   // { "blob", _varbinarymax },
                   // { "mediumblob", _varbinarymax },
                   // { "longblob", _varbinarymax },

                    // string
                    { "char", _char },
                    { "varchar", _varchar },

                    // TODO
                    { "BLOB SUB_TYPE 1", _json },
                   // { "text", _varcharmax },
                   // { "mediumtext", _varcharmax },
                   // { "longtext", _varcharmax },

                    // DateTime
                    { "time", _time },
                    { "timestamp", _timeStamp },

                    // json
                   // { "json", _json },

                    // guid
                    { "char(38)", _uniqueidentifier }
                };

            _clrTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
	                // boolean
	                { typeof(bool), _bit },

	                // integers
	                { typeof(short), _smallint },
	                { typeof(ushort), _usmallint },
	                { typeof(int), _int },
	                { typeof(uint), _uint },
	                { typeof(long), _bigint },
	                { typeof(ulong), _ubigint },

	                // decimals
	                { typeof(decimal), _decimal },
	                { typeof(float), _float },
	                { typeof(double), _double },

	                // byte / char
	                { typeof(sbyte), _tinyint },
	                { typeof(byte), _utinyint },
	                { typeof(char), _utinyint },

	                // DateTime
	                { typeof(DateTime), _timeStamp },
	                { typeof(DateTimeOffset), _timeStamp },
	                { typeof(TimeSpan), _time },

	                // json
	                { typeof(JsonObject<>), _json },

	                // guid
	                { typeof(Guid), _uniqueidentifier }
                };

            _disallowedMappings
                = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "binary",
                    "char",
                    "varbinary",
                    "varchar"
                };

            ByteArrayMapper // TODO
                = new ByteArrayRelationalTypeMapper(
                    8000,
                    _varbinarymax,
                    _varbinarymax,
                    _varbinary767,
                    _rowversion, size => new FirebirdByteArrayTypeMapping(
                        "BLOB SUB_TYPE 0 SEGMENT SIZE 80",
                        DbType.Binary));

            StringMapper
                = new StringRelationalTypeMapper(
                    maxBoundedAnsiLength: 4000,
                    defaultAnsiMapping: _varcharmax,
                    unboundedAnsiMapping: _varcharmax,
                    keyAnsiMapping: _varchar127,
                    createBoundedAnsiMapping: size => new FirebirdStringTypeMapping(
                        "varchar(" + size + ")",
                        DbType.AnsiString,
                        unicode: false,
                        size: size),
                    maxBoundedUnicodeLength: 4000,
                    defaultUnicodeMapping: _varcharmax,
                    unboundedUnicodeMapping: _varcharmax,
                    keyUnicodeMapping: _varchar127,
                    createBoundedUnicodeMapping: size => new FirebirdStringTypeMapping(
                        "varchar(" + size + ")",
                        DbType.AnsiString,
                        unicode: false,
                        size: size));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IByteArrayRelationalTypeMapper ByteArrayMapper { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IStringRelationalTypeMapper StringMapper { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void ValidateTypeName(string storeType)
        {
            if (_disallowedMappings.Contains(storeType))
            {
                throw new ArgumentException("UnqualifiedDataType" + storeType);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string GetColumnType(IProperty property) => property.Firebird().ColumnType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
            => _clrTypeMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
            => _storeTypeMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override RelationalTypeMapping FindMapping(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            clrType = clrType.UnwrapNullableType().UnwrapEnumType();

            if (clrType.Name == typeof(JsonObject<>).Name)
                return _json;

            return clrType == typeof(string)
                ? _varcharmax
                : (clrType == typeof(byte[])
                    ? _varbinarymax
                    : base.FindMapping(clrType));
        }

        // Indexes in SQL Server have a max size of 900 bytes
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool RequiresKeyMapping(IProperty property)
            => base.RequiresKeyMapping(property) || property.IsIndex();
    }
}
