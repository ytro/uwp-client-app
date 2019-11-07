﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using PCApplication.Configuration;
using PCApplication.JsonSchemas;
using PCApplication.UserControls;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PCApplication.Services {
    /// <summary>
    /// The singleton Rest service.
    /// This class contains all Rest API calls and manages communication errors.
    /// However, it doesn't handle the response, which is returned to the caller.
    /// </summary>
    public class RestService : IRestService {
        // Singleton HttpClient
        private HttpClient _client = new HttpClient();
        private string _token = "";

        public RestService() { }

        public async Task<bool> Login(string username, string password) {
            string requestUri = ConfigManager.GetBaseServerUri() + "/usager/login";
            try {
                //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://httpbin.org/post");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);

                string json = new JObject
                {
                   { "usager", "admin" },
                   { "mot_de_passe", password}
                }.ToString();

                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode) { // Represents a code from 200 to 299
                    string responseContent = await response.Content.ReadAsStringAsync();

                    // Check JSON reponse against schema
                    JsonSchema schema = JsonSchema.FromType<TokenResponse>();
                    var errors = schema.Validate(responseContent);
                    if (errors.Count > 0)
                    {
                        return false;
                    }

                    // Return deserialized JSON object
                    TokenResponse token = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
                    this._token = token.AccessToken;
                    return true;
                } else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) { // 400
                    CustomContentDialog.ShowAsync("Erreur 400:\n" + response.ToString(), title: "Erreur", primary: "OK");
                    return false;
                } else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) { // 403
                    CustomContentDialog.ShowAsync("Erreur 403:\n" + response.ToString(), title: "Erreur", primary: "OK");
                    return false;
                } else if (response.StatusCode == System.Net.HttpStatusCode.NotFound) { // 404
                    CustomContentDialog.ShowAsync("Erreur 404: Non trouvé", title: "Erreur", primary: "OK");
                    return false;
                } else {
                    CustomContentDialog.ShowAsync("Erreur de connection", title: "Erreur", primary: "OK");
                    return false;
                }
            } catch { }

            return false;
        }

        public async Task<bool> Logout() {
            string requestUri = ConfigManager.GetBaseServerUri() + "/admin/logout";
            try {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://httpbin.org/post");
                HttpResponseMessage response = await _client.SendAsync(request);
                if (response.IsSuccessStatusCode) {
                    return true;
                }
            } catch { }
            return false;
        }

        public async Task<bool> ChangePassword(string oldPassword, string newPassword) {
            string requestUri = ConfigManager.GetBaseServerUri() + "/admin/motdepasse";

            string json = new JObject
            {
                { "ancien", oldPassword },
                { "nouveau", newPassword }
            }.ToString();

            throw new System.NotImplementedException();
        }

        public async Task<bool> CreateAccount(string username, string password, bool isEditor) {
            string requestUri = ConfigManager.GetBaseServerUri() + "/admin/creationcompte";

            string json = new JObject
            {
                { "usager", username },
                { "mot_de_passe", password },
                { "edition", isEditor }
            }.ToString();

            throw new System.NotImplementedException();
        }

        public async Task<bool> DeleteAccount(string username) {
            string requestUri = ConfigManager.GetBaseServerUri() + "/admin/suppressioncompte";

            string json = new JObject
            {
                { "usager", username }
            }.ToString();

            throw new System.NotImplementedException();
        }

        public async Task<object> GetBlockchain(HostEnum source) {
            string requestUri = ConfigManager.GetBaseServerUri() + "/admin/chaine";
            switch (source) {
                case HostEnum.Miner1: requestUri += "/1"; break;
                case HostEnum.Miner2: requestUri += "/2"; break;
                case HostEnum.Miner3: requestUri += "/3"; break;
            }

            throw new System.NotImplementedException();
        }


        public async Task<LogsResponse> GetLogs(HostEnum source, int lastReceived) {
            string requestUri = ConfigManager.GetBaseServerUri() + "/admin/logs";

            switch (source) {
                case HostEnum.Miner1: requestUri += "/1"; break;
                case HostEnum.Miner2: requestUri += "/2"; break;
                case HostEnum.Miner3: requestUri += "/3"; break;
                case HostEnum.WebServer: requestUri += "/serveurweb"; break;
            }

            // Prepare request
            string json = new JObject {
                { "dernier", lastReceived }
            }.ToString();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send request
            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode) { // 200-299
                // Get response
                string responseContent = await response.Content.ReadAsStringAsync();

                // Check JSON reponse against schema
                JsonSchema schema = JsonSchema.FromType<LogsResponse>();
                var errors = schema.Validate(responseContent);
                if (errors.Count > 0) {
                    // Debug.WriteLine(error.Path + ": " + error.Kind);
                    return null;
                }

                // Mocked response
                // responseContent = StringResources.GetString("mockValidLogsJson");

                // Return deserialized JSON object
                return JsonConvert.DeserializeObject<LogsResponse>(responseContent);

            } else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) { // 400 Bad request
                CustomContentDialog.ShowAsync("Erreur 400: Mauvaise requête", title: "Erreur", primary: "OK");
                return null;
            } else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) { // 401 Unauthorized
                CustomContentDialog.ShowAsync("Erreur 403: Non authorisé", title: "Erreur", primary: "OK");
                return null;
            } else if (response.StatusCode == System.Net.HttpStatusCode.NotFound) { // 404
                CustomContentDialog.ShowAsync("Erreur 404: Non trouvé", title: "Erreur", primary: "OK");
                return null;
            } else {
                CustomContentDialog.ShowAsync("Erreur de connection", title: "Erreur", primary: "OK");
                return null;
            }
        }

    }

    public interface IRestService {
        // Requêtes POST
        Task<bool> Login(string username, string password);                         // POST /admin/login
        Task<bool> Logout();                                                        // POST /admin/logout
        Task<bool> ChangePassword(string oldPassword, string newPassword);          // POST /admin/motdepasse
        Task<bool> CreateAccount(string username, string password, bool isEditor);  // POST /admin/creationcompte
        Task<bool> DeleteAccount(string username);                                  // POST /admin/suppressioncompte

        // Requêtes GET
        Task<object> GetBlockchain(HostEnum source);                                // GET /admin/chaine/[1-3]
        Task<LogsResponse> GetLogs(HostEnum source, int lastReceived);     // GET /admin/logs/[1-3] et GET /admin/logs/serveurweb
    }
}