﻿using Microsoft.AspNetCore.Components.Authorization;
using Performance.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Performance.Client.Core
{
    public class AuthProvider : AuthenticationStateProvider
    {
        private readonly HttpClient HttpClient;

        public string UserName { get; set; }

        public AuthProvider(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            //这里获得用户登录状态
            var result = await HttpClient.GetFromJsonAsync<ResultData<UserDto>>($"api/Auth/GetUser");

            if (result.IsSuccess == false)
            {
                MarkUserAsLoggedOut();
                return new AuthenticationState(new ClaimsPrincipal());
            }
            else
            {
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, result.Data.Name));
                var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "apiauth"));
                return new AuthenticationState(authenticatedUser);
            }
        }

        /// <summary>
        /// 标记授权
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        public void MarkUserAsAuthenticated(UserDto userDto)
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", userDto.Token);
            UserName = userDto.Name;

            //根据服务器返回的数据进行配置本地的策略
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, userDto.Name));
            claims.Add(new Claim("Admin", "Admin"));
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "apiauth"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);

            //可以将Token存储在本地存储中，实现页面刷新无需登录
        }

        /// <summary>
        /// 标记注销
        /// </summary>
        public void MarkUserAsLoggedOut()
        {
            HttpClient.DefaultRequestHeaders.Authorization = null;
            UserName = null;

            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }
    }
}
