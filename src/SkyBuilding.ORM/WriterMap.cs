﻿using System;

namespace SkyBuilding.ORM
{
    /// <summary>
    /// SQL写入流
    /// </summary>
    public class WriterMap : IWriterMap
    {
        private readonly ISQLCorrectSettings _settings;
        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="settings">SQL 矫正配置</param>
        public WriterMap(ISQLCorrectSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }
        /// <summary>
        /// ,
        /// </summary>
        public string Delimiter => ",";
        /// <summary>
        /// (
        /// </summary>
        public string OpenBrace => "(";
        /// <summary>
        /// )
        /// </summary>
        public string CloseBrace => ")";
        /// <summary>
        /// ''
        /// </summary>
        public string EmptyString => "''";
        /// <summary>
        /// 空格
        /// </summary>
        public string WhiteSpace => " ";

        /// <summary>
        /// 参数名称
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        public virtual string ParamterName(string name) => _settings.ParamterName(name);
        /// <summary>
        /// 字段
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        public virtual string Name(string name) => _settings.Name(name);
    }
}
