using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AdvertApi.Models;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace WebAdvert.Web.ServiceClients
{
    public class AdvertApiClient : IAdvertApiClient
    {
        //private readonly IConfiguration _configuration;
        private readonly string _baseAddress;
        private readonly HttpClient _client;
        private readonly IMapper _mapper;
        public AdvertApiClient(IConfiguration configuration, HttpClient client, IMapper mapper)
        {
            //_configuration = configuration;
            _client = client;
            _mapper = mapper;

            _baseAddress = configuration.GetSection("AdvertApi").GetValue<string>("BaseUrl");

            //var createUrl = _configuration.GetSection("AdvertApi").GetValue<string>("BaseUrl");
            //_client.BaseAddress = new Uri(createUrl);
            //_client.DefaultRequestHeaders.Add("Content-type", "application/json");
        }

        public async Task<bool> ConfirmAsync(ConfirmAdvertRequest model)
        {
            var advertModel = _mapper.Map<ConfirmAdvertModel>(model);
            var jsonModel = JsonConvert.SerializeObject(advertModel);
            //var response = await _client
            //    .PutAsync(new Uri($"{_baseAddress}/confirm"), new StringContent(jsonModel))
            //    .ConfigureAwait(false);
            var response = await _client
                .PutAsync(new Uri($"{_baseAddress}/confirm"),
                    new StringContent(jsonModel, Encoding.UTF8, "application/json"))
                .ConfigureAwait(false);
            return response.StatusCode == HttpStatusCode.OK;

        }

        public async Task<AdvertResponse> CreateAsync(CreateAdvertModel model)
        {
            var advertApiModel =  _mapper.Map<AdvertModel>(model); 
            var jsonModel = JsonConvert.SerializeObject(advertApiModel);
            //var response = await _client.PostAsync(_client.BaseAddress, new StringContent(jsonModel)).ConfigureAwait(false);
            //var response = await _client
            //    .PostAsync(new Uri($"{_baseAddress}/create"), new StringContent(jsonModel))
            //    .ConfigureAwait(false);
            var response = await _client.PostAsync(new Uri($"{_baseAddress}/create"),
                new StringContent(jsonModel, Encoding.UTF8, "application/json")).ConfigureAwait(false);

            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var createAdvertResponse = JsonConvert.DeserializeObject<CreateAdvertResponse>(responseJson);
            var advertResponse = _mapper.Map<AdvertResponse>(createAdvertResponse); 
            return advertResponse;
        }
    }
}
