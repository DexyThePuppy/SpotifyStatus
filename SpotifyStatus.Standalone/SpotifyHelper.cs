using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyStatus.Standalone;

namespace SpotifyStatus
{
    internal static class SpotifyHelper
    {
        private static readonly HttpClient _httpClient = new();

        private static readonly JsonSerializer _jsonSerializer = JsonSerializer.CreateDefault();

        private static readonly Dictionary<string, PlayerSetRepeatRequest.State> _states = new()
        {
            { "track", PlayerSetRepeatRequest.State.Track },
            { "context", PlayerSetRepeatRequest.State.Context },
            { "off", PlayerSetRepeatRequest.State.Off }
        };

        static SpotifyHelper()
        {
            //_httpClient.head
        }

        public static string GetCover(this IPlayableItem playableItem)
        {
            return playableItem switch
            {
                FullTrack track => track.Album.Images[0].Url,
                FullEpisode episode => episode.Images[0].Url,
                _ => null
            };
        }

        public static IEnumerable<SpotifyResource> GetCreators(this IPlayableItem playableItem)
        {
            return playableItem switch
            {
                FullTrack track => track.Artists.Select(artist => new SpotifyResource(artist.Name, artist.ExternalUrls["spotify"])).ToArray(),
                FullEpisode episode => new[] { new SpotifyResource(episode.Show.Name, episode.Show.ExternalUrls["spotify"]) },
                _ => Enumerable.Empty<SpotifyResource>()
            };
        }

        public static int GetDuration(this IPlayableItem playableItem)
        {
            return playableItem switch
            {
                FullTrack track => track.DurationMs,
                FullEpisode episode => episode.DurationMs,
                _ => 100,
            };
        }

        public static SpotifyResource GetGrouping(this IPlayableItem playableItem)
        {
            return playableItem switch
            {
                FullTrack track => new SpotifyResource(track.Album.Name, track.Album.ExternalUrls["spotify"]),
                FullEpisode episode => new SpotifyResource(episode.Show.Name, episode.Show.ExternalUrls["spotify"]),
                _ => null
            };
        }

        public static string GetId(this IPlayableItem playableItem)
        {
            return playableItem switch
            {
                FullTrack track => track.Id,
                FullEpisode episode => episode.Id,
                _ => null
            };
        }

        public static SpotifyResource GetResource(this IPlayableItem playableItem)
        {
            return playableItem switch
            {
                FullTrack track => new SpotifyResource(track.Name, track.ExternalUrls["spotify"]),
                FullEpisode episode => new SpotifyResource(episode.Name, episode.Show.ExternalUrls["spotify"]),
                _ => null,
            };
        }

        public static PlayerSetRepeatRequest.State GetState(string name) => _states[name];

        public static PlayerSetRepeatRequest.State Next(this PlayerSetRepeatRequest.State state)
            => (PlayerSetRepeatRequest.State)(((int)state + 1) % 3);

        public static async void SendCanvasAsync(this IPlayableItem playableItem, Action<SpotifyInfo, string> sendMessage)
        {
            var trackId = playableItem.GetId();
            if (string.IsNullOrEmpty(trackId))
                return;

            try
            {
                string canvasUrl = await GetSpotifyTrackDownloadUrl(trackId);

                if (string.IsNullOrWhiteSpace(canvasUrl))
                {
                    sendMessage(SpotifyInfo.Canvas, "");
                    Console.WriteLine("No canvas for playable found.");
                    return;
                }

                sendMessage(SpotifyInfo.Canvas, canvasUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while getting canvas url");
                Console.WriteLine(ex.ToString());
            }
        }

        private static async Task<string> GetSpotifyTrackDownloadUrl(string trackId)
        {
            string url = $"https://www.canvasdownloader.com/canvas?link=https://open.spotify.com/track/{trackId}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string htmlContent = await response.Content.ReadAsStringAsync();

                // Search for an MP4 link in the HTML content
                string mp4Pattern = @"https?://[^\s""']+\.mp4";
                Match mp4Match = Regex.Match(htmlContent, mp4Pattern, RegexOptions.IgnoreCase);

                if (mp4Match.Success)
                {
                    return mp4Match.Value; // Return the found MP4 URL
                }
                else
                {
                    Console.WriteLine("No MP4 link found in the HTML content.");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return null;
            }
        }


        public static async void SendLyricsAsync(this IPlayableItem playableItem, Action<SpotifyInfo, string> sendMessage)
        {
            sendMessage(SpotifyInfo.ClearLyrics, "");

            var id = playableItem.GetId();
            if (string.IsNullOrEmpty(id))
                return;

            try
            {
                using var response = await _httpClient.GetAsync($"https://spotify-lyrics-api-umber.vercel.app/?trackid={id}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                // Check if the content is HTML
                if (content.TrimStart().StartsWith("<"))
                {
                    Console.WriteLine("Received HTML instead of JSON. Attempting to extract lyrics.");
                    ExtractLyricsFromHtml(content, sendMessage);
                    return;
                }

                // If it's not HTML, assume it's JSON and try to parse it
                using var jsonTextReader = new JsonTextReader(new StringReader(content));
                var lyrics = _jsonSerializer.Deserialize<SongLyrics>(jsonTextReader);

                if (lyrics is null || lyrics.Error)
                {
                    Console.WriteLine("No lyrics for playable found.");
                    return;
                }

                foreach (var line in lyrics.Lines)
                    sendMessage(SpotifyInfo.LyricsLine, line.ToString());
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request error while getting lyrics: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error while getting lyrics: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while getting lyrics");
                Console.WriteLine(ex.ToString());
            }
        }

        private static void ExtractLyricsFromHtml(string htmlContent, Action<SpotifyInfo, string> sendMessage)
        {
            // Simple regex to extract text between <p> tags
            var matches = Regex.Matches(htmlContent, @"<p>(.*?)</p>", RegexOptions.Singleline);
            
            if (matches.Count == 0)
            {
                Console.WriteLine("No lyrics found in HTML content.");
                return;
            }

            foreach (Match match in matches)
            {
                string line = System.Web.HttpUtility.HtmlDecode(match.Groups[1].Value.Trim());
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sendMessage(SpotifyInfo.LyricsLine, line);
                }
            }
        }

        public static int ToUpdateInt(this SpotifyInfo info)
            => info == SpotifyInfo.Clear ? 0 : ((int)Math.Log2((int)info) + 1);
    }
}
