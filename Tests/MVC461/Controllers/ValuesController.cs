﻿using System;
using SkyBuilding.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SkyBuilding;
using SkyBuilding.Exceptions;

namespace MVC461.Controllers
{
    /// <inheritdoc />
    public enum TestEnum
    {
        Normal
    }
    /// <inheritdoc />
    public interface IDependency
    {

    }
    /// <inheritdoc />
    public class Dependency : IDependency
    {

    }

    /// <inheritdoc />
    public class ValuesController : BaseController
    {
        /// <inheritdoc />
        public ValuesController(IDependency dependency)
        {

        }
        /// <inheritdoc />
        // GET api/values
        [HttpGet]
        public IEnumerable<object> Get()
        {
            return new object[] { TestEnum.Normal, 1000000uL, 10000000000000000000uL };
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET api/values/5
        [HttpGet]
        public ulong Get(ulong id)
        {
            return id;
        }
        /// <inheritdoc />
        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        /// <inheritdoc />
        // PUT api/values/5
        [HttpPut]
        public void Put(int id, [FromBody]string value)
        {
        }
        /// <inheritdoc />
        // DELETE api/values/5
        [HttpDelete]
        [JwtAuthorize]
        public void Delete(int id)
        {
            throw new BusiException("认证测试成功");
        }
        /// <inheritdoc />
        [HttpGet]
        [ActionName("login")]
        public DResult Login(string account, string password)
        {
            return DResult.Ok(new
            {
                id = 100000,
                name = account
            });
        }
    }
}
