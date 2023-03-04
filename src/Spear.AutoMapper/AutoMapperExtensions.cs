using AutoMapper;
using Spear.AutoMapper.Attributes;
using Spear.Core;
using Spear.Core.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.AutoMapper
{
    /// <summary> AutoMapper扩展 </summary>
    public static class AutoMapperExtensions
    {
        private static readonly ConcurrentDictionary<MapperType, Lazy<IMapper>> Mappers =
            new ConcurrentDictionary<MapperType, Lazy<IMapper>>();

        private static IMapper Create(MapperType mapperType, Type sourceType, Type destinationType,
            TypeMap[] maps = null)
        {
            var cfg = new MapperConfiguration(config =>
            {
                if (maps != null && maps.Any())
                {
                    foreach (var map in maps)
                    {
                        CreateMapWithAttribute(config, map.SourceType, map.DestinationType);
                    }
                }

                CreateMapWithAttribute(config, sourceType, destinationType);

                config.CreateMissingTypeMaps = true;
                config.ValidateInlineMaps = false;
                switch (mapperType)
                {
                    case MapperType.FromUrl:
                        config.SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
                        break;
                    case MapperType.ToUrl:
                        config.DestinationMemberNamingConvention = new LowerUnderscoreNamingConvention();
                        break;
                }
            });
            return cfg.CreateMapper();
        }

        private static void CreateMapWithAttribute(IMapperConfigurationExpression config, Type sourceType, Type destinationType)
        {
            var express = config.CreateMap(sourceType, destinationType);

            destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Foreach(prop =>
                {
                    var mapfrom = prop.GetCustomAttribute<MapFromAttribute>();
                    if (mapfrom != null)
                        express.ForMember(prop.Name, opt => opt.MapFrom(mapfrom.Name));
                });
        }

        private static IMapper CreateFromCache(Type sourceType, Type destinationType,
            MapperType mapperType = MapperType.Normal)
        {
            if (Mappers.TryGetValue(mapperType, out var lazyMapper))
            {
                var mapper = lazyMapper.Value;
                var m = mapper.ConfigurationProvider.FindTypeMapFor(sourceType, destinationType);
                if (m != null)
                    return mapper;
            }

            lazyMapper = Mappers.AddOrUpdate(mapperType,
                type => new Lazy<IMapper>(() => Create(type, sourceType, destinationType)),
                (type, old) =>
                {
                    var maps = old.Value.ConfigurationProvider.GetAllTypeMaps();
                    return new Lazy<IMapper>(() => Create(type, sourceType, destinationType, maps));
                });
            return lazyMapper.Value;
        }

        /// <summary> 映射实体 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="mapperType"></param>
        /// <param name="fromCache"></param>
        /// <returns></returns>
        public static T MapTo<T>(this object source, MapperType mapperType = MapperType.Normal, bool fromCache = false)
        {
            if (source == null)
                return default;
            if (source is Task task)
            {
                var taskType = task.GetType().GetTypeInfo();
                var prop = taskType.GetProperty("Result");
                if (!taskType.IsGenericType || prop == null)
                    return default;
                task.SyncRun();
                source = prop.GetValue(task);
            }

            Type sourceType = source.GetType(), destinationType = typeof(T);
            if (sourceType == destinationType)
                return (T)source;
            if (source is IEnumerable listSource)
            {
                foreach (var item in listSource)
                {
                    sourceType = item.GetType();
                    break;
                }
            }

            var mapper = fromCache
                ? CreateFromCache(sourceType, destinationType, mapperType)
                : Create(mapperType, sourceType, destinationType);

            return mapper.Map<T>(source);
        }

        /// <summary> 映射实体 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="mapperType"></param>
        /// <param name="fromCache"></param>
        /// <returns></returns>
        public static List<T> MapTo<T>(this IEnumerable source, MapperType mapperType = MapperType.Normal,
            bool fromCache = false)
        {
            return ((object)source).MapTo<List<T>>(mapperType, fromCache);
        }

        /// <summary> 映射成分页类型 </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="pagedList"></param>
        /// <param name="mapperType"></param>
        /// <param name="fromCache"></param>
        /// <returns></returns>
        public static PagedList<T> MapPagedList<T, TSource>(this PagedList<TSource> pagedList,
            MapperType mapperType = MapperType.Normal, bool fromCache = false)
        {
            if (pagedList == null)
                return new PagedList<T>(new List<T>(), 0);
            var list = pagedList.List.MapTo<T>(mapperType, fromCache);
            return new PagedList<T>(list, pagedList.Index, pagedList.Size, pagedList.Total);
        }

        /// <summary> DataReader映射 </summary>
        public static IEnumerable<T> DataReaderMapTo<T>(this IDataReader reader)
        {
            var cfg = new MapperConfiguration(config => { config.CreateMap<IDataReader, IEnumerable<T>>(); });
            return cfg.CreateMapper().Map<IDataReader, IEnumerable<T>>(reader);
        }
    }
}
