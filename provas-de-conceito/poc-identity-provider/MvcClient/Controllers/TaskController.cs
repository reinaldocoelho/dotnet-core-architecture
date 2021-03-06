﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MvcClient.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MvcClient.Services;

namespace MvcClient.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly IResourcesHttpClient _resourcesHttpClient;

        public TaskController(IResourcesHttpClient resourcesHttpClient)
        {
            _resourcesHttpClient = resourcesHttpClient;
        }

        public async Task<IActionResult> MyTasks()
        {
            var client = await _resourcesHttpClient.GetClient();
            var response = await client.GetAsync("http://localhost:3000/tasks").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var tasksList = JsonConvert.DeserializeObject<List<TaskViewModel>>(content);
                return View(tasksList);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Denied", "Account");
            }

            throw new Exception($"Problema ao acessar a API: {response.ReasonPhrase}");
        }

        [Authorize(Roles = "Subscriber")]
        public async Task<IActionResult> Finish(Guid id)
        {
            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var client = new HttpClient();
            client.SetBearerToken(accessToken);
            var response = await client.PostAsync($"http://localhost:3000/tasks/finish/{id}", null).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("MyTasks");
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("Denied", "Account");
            }

            throw new Exception($"Problema ao acessar a API: {response.ReasonPhrase}");
        }
    }
}