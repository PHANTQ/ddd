using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CategoriesService.DTO;
using Microsoft.Extensions.Options;

namespace CategoriesService.Services
{
    public class BadWordService : IBadWordService
    {
        private readonly HttpClient _httpClient;
        private readonly ProfanityApiOptions _options;

        // Danh sách từ nhạy cảm nội bộ (có thể mở rộng hoặc lấy từ file/config)
        private readonly HashSet<string> _localBadWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
           "đĩ", "đĩ điếm", "lồn", "cặc", "cu", "cặc", "chim", "buồi", "mẹ kiếp", "đồ chó", "chó", "lợn", "khốn", "đĩ mẹ", "chó chết", "chết mẹ"
            , "đéo", "đụ", "địt", "mày", "mày chết", "con mẹ mày", "con đĩ", "con lợn", "con chó", "mịa mày", "đồ khốn", "thằng chó", "địt mẹ"
            , "đĩ mợ", "địt con mẹ mày", "đồ điếm", "chết tiệt", "súc vật", "đần độn", "ngu", "dốt", "ngu ngốc", "điên", "khùng", "bựa", "bẩn"
            , "loz", "lồn", "lộn", "cu", "mồm", "mồm chó", "mặt chó", "mặt lồn", "mặt đĩ", "dâm", "đĩ", "hư hỏng", "buồi", "bú", "bú cu", "bú liếm"
            , "lồn mẹ", "vãi đái", "vãi cả lồn", "vãi cả", "vãi", "thối", "bựa", "bẩn thỉu", "chó đẻ", "đẻ con chó", "chết tiệt", "chết tiệt mày"
            , "đĩ con", "đĩ cái", "con đĩ cái", "đái", "đái dầm", "đụ mẹ", "đụ con", "đụ cái", "đụ đĩ", "đụ đéo", "địt mẹ", "địt mợ", "địt con"
            , "địt con mẹ mày", "phò", "phò đĩ", "phò cái", "đĩ cái", "con đĩ", "lồn", "lồn mẹ", "lồn con", "lồn chó", "lồn lợn", "buồi", "buồi chó"
            , "buồi lợn", "cặc", "cặc chó", "cặc lợn", "lồn chó", "lồn lợn", "chó", "chó chết", "chó nhà", "đéo", "đéo mày", "đéo con mẹ", "đéo cái"
            , "đồ đĩ", "đồ chó", "đồ đần", "đồ khốn", "đồ chết tiệt", "đồ chó chết", "ngu", "ngu si", "ngu ngốc", "ngu như lợn", "ngu như chó", "khốn"
            , "khốn nạn", "khốn kiếp", "khốn mịa", "khốn bố", "đần", "đần độn", "đần đốn", "đần dốt", "đần chọc", "dốt", "dốt nát", "dốt nát vô cùng"
            , "điên", "điên khùng", "điên loạn", "điên rồ", "điên dại", "lồn", "lồn mẹ", "lồn chó", "lồn lợn", "lồn lều", "lồn đĩ", "bựa", "bựa bựa"
            , "bựa hết chỗ nói", "dâm", "dâm đãng", "dâm dục", "dâm ô", "đĩ", "đĩ điếm", "đĩ mẹ", "đĩ con", "đĩ cái", "đĩ chó", "đĩ lợn", "địt", "địt mẹ"
            , "địt con", "địt đĩ", "phò", "phò đĩ", "phò cái", "súc vật", "súc sinh", "vật ngu", "vãi", "vãi đái", "vãi cả đái", "vãi cả lồn", "vãi cả chó"
            , "vãi cả lợn", "mày", "mày chết", "mày đéo", "mày khốn", "mày chó", "con mẹ", "con chó", "con lợn", "con đĩ", "chó", "chó chết", "chó nhà", "lợn"
            , "lợn chết", "lợn nhà", "chửi", "chửi thề", "chửi tục"

        };

        public BadWordService(HttpClient httpClient, IOptions<ProfanityApiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<(bool IsBad, List<string> BadWords)> CheckProfanityAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return (false, new List<string>());

            // Kiểm tra từ nhạy cảm trong danh sách nội bộ
            var badWordsFound = new List<string>();

            // Tách text thành các từ (có thể tùy chỉnh regex hoặc cách tách)
            var words = text.Split(new char[] { ' ', ',', '.', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                if (_localBadWords.Contains(word))
                {
                    badWordsFound.Add(word);
                }
            }

            if (badWordsFound.Any())
            {
                // Nếu đã phát hiện từ nhạy cảm nội bộ thì không cần gọi API nữa
                return (true, badWordsFound);
            }

            // Nếu chưa phát hiện từ nhạy cảm trong danh sách nội bộ thì gọi API để kiểm tra tiếp
            var form = new Dictionary<string, string>
            {
                ["content"] = text,
                ["censor-character"] = "*",
                ["user-id"] = _options.UserId,
                ["api-key"] = _options.ApiKey
            };

            var response = await _httpClient.PostAsync(_options.BaseUrl, new FormUrlEncodedContent(form));

            if (!response.IsSuccessStatusCode)
                return (false, new List<string>());

            var result = await response.Content.ReadAsStringAsync();

            using var jsonDoc = JsonDocument.Parse(result);
            var root = jsonDoc.RootElement;

            bool isBad = root.GetProperty("is-bad").GetBoolean();

            if (isBad)
            {
                if (root.TryGetProperty("bad-words-list", out var badList))
                {
                    foreach (var word in badList.EnumerateArray())
                    {
                        badWordsFound.Add(word.GetString());
                    }
                }
            }

            return (isBad, badWordsFound);
        }
    }
}
