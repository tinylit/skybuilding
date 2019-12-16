﻿using CodeArts;
using CodeArts.Net;
using CodeArts.Serialize.Json;
using CodeArts.Serialize.Xml;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// 地址拓展
    /// </summary>
    public static class UriExtentions
    {
        private static readonly Regex UriPattern = new Regex(@"\w+://.+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private abstract class Requestable<T> : IRequestable<T>
        {
            public T Get(int timeout = 5000) => Request("GET", timeout);

            public Task<T> GetAsync(int timeout = 5000) => RequestAsync("GET", timeout);

            public T Delete(int timeout = 5000) => Request("DELETE", timeout);

            public Task<T> DeleteAsync(int timeout = 5000) => RequestAsync("DELETE", timeout);

            public T Post(int timeout = 5000) => Request("POST", timeout);

            public Task<T> PostAsync(int timeout = 5000) => RequestAsync("POST", timeout);

            public T Put(int timeout = 5000) => Request("PUT", timeout);

            public Task<T> PutAsync(int timeout = 5000) => RequestAsync("PUT", timeout);

            public T Head(int timeout = 5000) => Request("HEAD", timeout);

            public Task<T> HeadAsync(int timeout = 5000) => RequestAsync("HEAD", timeout);

            public T Patch(int timeout = 5000) => Request("PATCH", timeout);

            public Task<T> PatchAsync(int timeout = 5000) => RequestAsync("PATCH", timeout);

            public abstract T Request(string method, int timeout = 5000);

#if NET40
            public Task<T> RequestAsync(string method, int timeout = 5000)
                => Task.Factory.StartNew(() => Request(method, timeout));
#else

            public abstract Task<T> RequestAsync(string method, int timeout = 5000);
#endif
        }

        private class Requestable : Requestable<string>, IRequestable
        {
            private string __data;
            private Uri __uri;
            private NameValueCollection __form;
            private readonly Dictionary<string, string> __headers;

            public Requestable(string uriString) : this(new Uri(uriString)) { }

            public Requestable(Uri uri)
            {
                __uri = uri ?? throw new ArgumentNullException(nameof(uri));
                __headers = new Dictionary<string, string>();
            }

            public IRequestable AppendHeader(string header, string value)
            {
                __headers.Add(header, value);

                return this;
            }

            public IRequestable AppendHeaders(IEnumerable<KeyValuePair<string, string>> headers)
            {
                foreach (var kv in headers)
                {
                    __headers.Add(kv.Key, kv.Value);
                }

                return this;
            }

            public IRequestable ToForm(string param, NamingType namingType = NamingType.Normal)
                => ToForm(JsonHelper.Json<Dictionary<string, string>>(param, namingType));

            public IRequestable ToForm<T>(T param, NamingType namingType = NamingType.Normal) where T : IEnumerable<KeyValuePair<string, string>>
            {
                __form = __form ?? new NameValueCollection();

                foreach (var kv in param)
                {
                    __form.Add(kv.Key.ToNamingCase(namingType), kv.Value);
                }

                return AppendHeader("Content-Type", "application/x-www-form-urlencoded");
            }

            public IRequestable ToForm(IEnumerable<KeyValuePair<string, DateTime>> param, NamingType namingType = NamingType.Normal)
                => ToForm(param.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString("yyyy-MM-dd HH:mm:ss.FFFFFFFK"))), namingType);

            public IRequestable ToForm<T>(IEnumerable<KeyValuePair<string, T>> param, NamingType namingType = NamingType.Normal)
                => ToForm(param.Select(x => new KeyValuePair<string, string>(x.Key, x.Value?.ToString())), namingType);

            public IRequestable ToForm(object param, NamingType namingType = NamingType.Normal)
            {
                if (param is null) return this;

                var typeStore = RuntimeTypeCache.Instance.GetCache(param.GetType());

                return ToForm(typeStore.PropertyStores
                    .Where(x => x.CanRead)
                    .Select(info =>
                    {
                        var item = info.Member.GetValue(param, null);

                        if (item is null) return new KeyValuePair<string, string>(info.Name, null);

                        if (item is DateTime date)
                        {
                            return new KeyValuePair<string, string>(info.Name, date.ToString("yyyy-MM-dd HH:mm:ss.FFFFFFFK"));
                        }

                        return new KeyValuePair<string, string>(info.Name, item.ToString());

                    }), namingType);
            }

            public IRequestable ToJson(string param)
            {
                __data = param;

                return AppendHeader("Content-Type", "application/json");
            }

            public IRequestable ToJson<T>(T param, NamingType namingType = NamingType.CamelCase) where T : class
                => ToJson(JsonHelper.ToJson(param, namingType));

            public IRequestable ToQueryString(string param)
            {
                if (string.IsNullOrEmpty(param)) return this;

                string uriString = __uri.ToString();

                string query = param.TrimStart('?', '&');

                __uri = new Uri(uriString + (string.IsNullOrEmpty(__uri.Query) ? "?" : "&") + query);

                return this;
            }

            public IRequestable ToQueryString(IEnumerable<string> param)
              => ToQueryString(string.Join("&", param));

            public IRequestable ToQueryString<T>(T param) where T : IEnumerable<KeyValuePair<string, string>>
                 => ToQueryString(string.Join("&", param.Select(kv => string.Concat(kv.Key, "=", kv.Value))));

            public IRequestable ToQueryString(IEnumerable<KeyValuePair<string, DateTime>> param)
                  => ToQueryString(string.Join("&", param.Select(x => string.Concat(x.Key, "=", x.Value.ToString("yyyy-MM-dd HH:mm:ss.FFFFFFFK")))));

            public IRequestable ToQueryString<T>(IEnumerable<KeyValuePair<string, T>> param)
                  => ToQueryString(string.Join("&", param.Select(x => string.Concat(x.Key, "=", x.Value?.ToString() ?? string.Empty))));

            public IRequestable ToQueryString(object param, NamingType namingType = NamingType.UrlCase)
            {
                if (param is null) return this;

                var typeStore = RuntimeTypeCache.Instance.GetCache(param.GetType());

                return ToQueryString(string.Join("&", typeStore.PropertyStores
                    .Where(x => x.CanRead)
                    .Select(info =>
                    {
                        var item = info.Member.GetValue(param, null);

                        if (item is null) return string.Concat(info.Name.ToNamingCase(namingType), "=", string.Empty);

                        if (item is DateTime date)
                        {
                            return string.Concat(info.Name.ToNamingCase(namingType), "=", date.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        return string.Concat(info.Name.ToNamingCase(namingType), "=", item.ToString());
                    })));
            }

            public override string Request(string method, int timeout = 5)
            {
                using (var client = new SkyWebClient
                {
                    Timeout = timeout,
                    Encoding = Encoding.UTF8
                })
                {
                    foreach (var kv in __headers)
                    {
                        client.Headers.Add(kv.Key, kv.Value);
                    }

                    if (method.ToUpper() == "GET")
                    {
                        return Encoding.UTF8.GetString(client.DownloadData(__uri));
                    }

                    if (__form is null)
                    {
                        client.UploadString(__uri, method.ToUpper(), __data ?? string.Empty);
                    }

                    return Encoding.UTF8.GetString(client.UploadValues(__uri, method.ToUpper(), __form));
                }
            }

#if !NET40
            public override async Task<string> RequestAsync(string method, int timeout = 5)
            {
                using (var client = new SkyWebClient
                {
                    Timeout = timeout,
                    Encoding = Encoding.UTF8
                })
                {
                    foreach (var kv in __headers)
                    {
                        client.Headers.Add(kv.Key, kv.Value);
                    }

                    if (method.ToUpper() == "GET")
                    {
                        return Encoding.UTF8.GetString(await client.DownloadDataTaskAsync(__uri));
                    }

                    if (__form is null)
                    {
                        return await client.UploadStringTaskAsync(__uri, method.ToUpper(), __data ?? string.Empty);
                    }

                    return Encoding.UTF8.GetString(await client.UploadValuesTaskAsync(__uri, method.ToUpper(), __form));
                }
            }
#endif
            public IRequestable ToXml(string param)
            {
                __data = param;

                return AppendHeader("Content-Type", "application/xml");
            }

            public IRequestable ToXml<T>(T param) where T : class => ToXml(XmlHelper.XmlSerialize(param));

            public IJsonRequestable<T> Json<T>(NamingType namingType = NamingType.CamelCase) where T : class => new JsonRequestable<T>(this, namingType);

            public IXmlRequestable<T> Xml<T>() where T : class => new XmlRequestable<T>(this);

            public IJsonRequestable<T> Json<T>(T _, NamingType namingType = NamingType.CamelCase) where T : class => new JsonRequestable<T>(this, namingType);

            public IXmlRequestable<T> Xml<T>(T _) where T : class => new XmlRequestable<T>(this);
        }

        private class JsonRequestable<T> : Requestable<T>, IJsonRequestable<T>
        {
            private readonly IRequestable requestable;

            public JsonRequestable(IRequestable requestable, NamingType namingType)
            {
                this.requestable = requestable;
                NamingType = namingType;
            }

            public NamingType NamingType { get; }

            public override T Request(string method, int timeout = 5000)
            {
                string value = requestable.Request(method, timeout);

                if (string.IsNullOrEmpty(value))
                    return default;
                try
                {
                    return JsonHelper.Json<T>(value, NamingType);
                }
                catch
                {
                    return default;
                }
            }

#if !NET40

            public override async Task<T> RequestAsync(string method, int timeout = 5000)
            {
                string value = await requestable.RequestAsync(method, timeout);

                if (string.IsNullOrEmpty(value))
                    return default;

                try
                {
                    return JsonHelper.Json<T>(value, NamingType);
                }
                catch
                {
                    return default;
                }
            }
#endif
        }

        private class XmlRequestable<T> : Requestable<T>, IXmlRequestable<T>
        {
            private readonly IRequestable requestable;

            public XmlRequestable(IRequestable requestable)
            {
                this.requestable = requestable;
            }

            public override T Request(string method, int timeout = 5000)
            {
                string value = requestable.Request(method, timeout);

                if (string.IsNullOrEmpty(value))
                    return default;
                try
                {
                    return XmlHelper.XmlDeserialize<T>(value);
                }
                catch
                {
                    return default;
                }
            }

#if !NET40
            public override async Task<T> RequestAsync(string method, int timeout = 5000)
            {
                string value = await requestable.RequestAsync(method, timeout);

                if (string.IsNullOrEmpty(value))
                    return default;

                try
                {
                    return XmlHelper.XmlDeserialize<T>(value);
                }
                catch
                {
                    return default;
                }
            }
#endif
        }

        /// <summary>
        /// 字符串是否为有效链接
        /// </summary>
        /// <param name="uriString">链接地址</param>
        /// <returns></returns>
        public static bool IsUrl(this string uriString) => UriPattern.IsMatch(uriString);

        /// <summary>
        /// 提供远程请求能力
        /// </summary>
        /// <param name="uriString">请求地址</param>
        /// <returns></returns>
        public static IRequestable AsRequestable(this string uriString)
            => new Requestable(uriString);

        /// <summary>
        /// 提供远程请求能力
        /// </summary>
        /// <param name="uri">请求地址</param>
        /// <returns></returns>
        public static IRequestable AsRequestable(this Uri uri)
            => new Requestable(uri);
    }
}