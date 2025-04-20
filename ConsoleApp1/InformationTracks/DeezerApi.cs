using System.Net.Http.Json;
using ConsoleApp1.InformationTracks.Dto;

namespace ConsoleApp1.InformationTracks
{
    public class DeezerApi
    {
        public async Task<InformationDeezerTrackDto> GetInformationTrack(string artist, string song) 
        {

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var url = $"https://api.deezer.com/search?q=artist:\"{artist.Trim()}\"track:\"{song.Trim()}\"";
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadFromJsonAsync<DeezerDto>();

                        if (responseData != null || responseData.Data != null && responseData.Data.Any())
                        {
                            response = await httpClient.GetAsync($"https://api.deezer.com/track/{responseData.Data.First().Id}");

                            if (response.IsSuccessStatusCode)
                            {
                                var responseInformation = await response.Content.ReadFromJsonAsync<InformationDeezerTrackDto>();
                                return responseInformation;
                            }
                        }

                        return null;
                    }
                    else
                    {
                        Console.WriteLine($"Error al consultar la API {response.ReasonPhrase}");
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
