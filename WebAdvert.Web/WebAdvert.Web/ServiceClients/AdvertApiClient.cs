using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AdvertApi.Models;
using Amazon.ServiceDiscovery;
using Amazon.ServiceDiscovery.Model;
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
            _client = client;
            _mapper = mapper;


            var discoveryClient = new AmazonServiceDiscoveryClient();
            var discoveryTask =  discoveryClient.DiscoverInstancesAsync(new DiscoverInstancesRequest()
            {
                ServiceName = "advertapi",
                NamespaceName = "WebAdvertisement"
            });
            discoveryTask.Wait();

            var instances = discoveryTask.Result.Instances; //randomize
            var ipv4 = instances[0].Attributes["AWS_INSTANCE_IPV4"];
            var port = instances[0].Attributes["AWS_INSTANCE_PORT"];

            _baseAddress = configuration.GetSection("AdvertApi").GetValue<string>("BaseUrl");

            //var createUrl = _configuration.GetSection("AdvertApi").GetValue<string>("BaseUrl");
            //_client.BaseAddress = new Uri(createUrl);
        }

        public async Task<bool> ConfirmAsync(ConfirmAdvertRequest model)
        {
            var advertModel = _mapper.Map<ConfirmAdvertModel>(model);
            var jsonModel = JsonConvert.SerializeObject(advertModel);
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
            var response = await _client.PostAsync(new Uri($"{_baseAddress}/create"),
                new StringContent(jsonModel, Encoding.UTF8, "application/json")).ConfigureAwait(false);

            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var createAdvertResponse = JsonConvert.DeserializeObject<CreateAdvertResponse>(responseJson);
            var advertResponse = _mapper.Map<AdvertResponse>(createAdvertResponse); 
            return advertResponse;
        }

        public async Task<List<Advertisement>> GetAllAsync()
        {
            var apiCallResponse = await _client.GetAsync(new Uri($"{_baseAddress}/all")).ConfigureAwait(false);
            var allAdvertModels = await apiCallResponse.Content.ReadAsAsync<List<AdvertModel>>().ConfigureAwait(false);
            return allAdvertModels.Select(x => _mapper.Map<Advertisement>(x)).ToList();
        }

        public async Task<Advertisement> GetAsync(string advertId)
        {
            var apiCallResponse = await _client.GetAsync(new Uri($"{_baseAddress}/{advertId}")).ConfigureAwait(false);
            var fullAdvert = await apiCallResponse.Content.ReadAsAsync<AdvertModel>().ConfigureAwait(false);
            return _mapper.Map<Advertisement>(fullAdvert);
        }
    }
}
