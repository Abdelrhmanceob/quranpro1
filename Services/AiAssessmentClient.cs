using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Maeen1_New.Services
{
    public class AiProviderConfigurationStatus
    {
        public bool IsEnabled { get; set; }
        public string Message { get; set; }
        public string BaseUrl { get; set; }
        public string Model { get; set; }
    }

    public class AiAssessmentClient
    {
        public AiProviderConfigurationStatus GetConfigurationStatus()
        {
            var baseUrl = ConfigurationManager.AppSettings["AiProviderBaseUrl"];
            var apiKey = ConfigurationManager.AppSettings["AiProviderApiKey"];
            var model = ConfigurationManager.AppSettings["AiProviderModel"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return new AiProviderConfigurationStatus
                {
                    IsEnabled = false,
                    Message = "الذكاء الاصطناعي غير مفعل: قيمة AiProviderBaseUrl غير موجودة."
                };
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new AiProviderConfigurationStatus
                {
                    IsEnabled = false,
                    Message = "الذكاء الاصطناعي غير مفعل: أضف AiProviderApiKey في Web.config.",
                    BaseUrl = baseUrl
                };
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                return new AiProviderConfigurationStatus
                {
                    IsEnabled = false,
                    Message = "الذكاء الاصطناعي غير مفعل: أضف AiProviderModel في Web.config.",
                    BaseUrl = baseUrl
                };
            }

            return new AiProviderConfigurationStatus
            {
                IsEnabled = true,
                Message = "الذكاء الاصطناعي مفعل ومربوط مع مزود OpenAI-compatible.",
                BaseUrl = baseUrl,
                Model = model
            };
        }

        public AssessmentResult TryAssess(WizardAnswers answers)
        {
            var config = GetConfigurationStatus();
            if (!config.IsEnabled)
            {
                return null;
            }

            var baseUrl = ConfigurationManager.AppSettings["AiProviderBaseUrl"];
            var apiKey = ConfigurationManager.AppSettings["AiProviderApiKey"];
            var model = ConfigurationManager.AppSettings["AiProviderModel"];
            var timeoutSetting = ConfigurationManager.AppSettings["AiProviderTimeoutSeconds"];
            var customPath = ConfigurationManager.AppSettings["AiProviderChatCompletionsPath"];

            var endpointUrl = BuildChatCompletionsEndpoint(baseUrl, customPath);
            if (string.IsNullOrWhiteSpace(endpointUrl))
            {
                return null;
            }

            int timeoutSeconds;
            if (!int.TryParse(timeoutSetting, out timeoutSeconds))
            {
                timeoutSeconds = 20;
            }

            var prompt = BuildPrompt(answers);
            var requestBody = new
            {
                model = model,
                temperature = 0.2,
                messages = new[]
                {
                    new { role = "system", content = "You are an educational planner for Quran memorization students." },
                    new { role = "user", content = prompt }
                }
            };

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                    var jsonPayload = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = client.PostAsync(endpointUrl, content).GetAwaiter().GetResult();

                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    return ParseAssessmentResult(body);
                }
            }
            catch
            {
                return null;
            }
        }

        private static string BuildChatCompletionsEndpoint(string baseUrl, string customPath)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return null;
            }

            var cleanBase = baseUrl.Trim().TrimEnd('/');
            var hasV1Suffix = cleanBase.EndsWith("/v1", StringComparison.OrdinalIgnoreCase);

            string endpointPath;
            if (string.IsNullOrWhiteSpace(customPath))
            {
                endpointPath = hasV1Suffix ? "chat/completions" : "v1/chat/completions";
            }
            else
            {
                endpointPath = customPath.Trim().TrimStart('/');
            }

            return cleanBase + "/" + endpointPath;
        }

        private static string BuildPrompt(WizardAnswers answers)
        {
            return
                "Return only valid JSON with these keys: level, plan, smartSchedule, suggestedTeacherName. " +
                "No markdown, no extra text. " +
                "The smartSchedule must be an Arabic practical weekly plan with day-by-day tasks and realistic memorization/revision loads. " +
                "Student answers are: " +
                "dailyMemorizationHours=" + answers.DailyMemorizationHours +
                ", targetJuzCount=" + answers.TargetJuzCount +
                ", targetJuzNames=" + (answers.TargetJuzNames ?? string.Empty) +
                ", completedJuzCount=" + answers.CompletedJuzCount +
                ", completedJuzNames=" + (answers.CompletedJuzNames ?? string.Empty) +
                ", tajweedLevel=" + answers.TajweedLevel +
                ", targetDuration=" + answers.TargetDuration + ". " +
                "Take into account the relative difficulty of listed Juz/Surahs when deciding level and smartSchedule.";
        }

        private static AssessmentResult ParseAssessmentResult(string rawJson)
        {
            try
            {
                var root = JObject.Parse(rawJson);
                var content = (string)root["choices"]?[0]?["message"]?["content"];
                if (string.IsNullOrWhiteSpace(content))
                {
                    return null;
                }

                var trimmed = content.Trim();
                var firstCurly = trimmed.IndexOf('{');
                var lastCurly = trimmed.LastIndexOf('}');

                if (firstCurly < 0 || lastCurly <= firstCurly)
                {
                    return null;
                }

                var innerJson = trimmed.Substring(firstCurly, lastCurly - firstCurly + 1);
                var payload = JObject.Parse(innerJson);

                var result = new AssessmentResult
                {
                    Level = (string)payload["level"],
                    Plan = (string)payload["plan"],
                    SmartSchedule = (string)payload["smartSchedule"],
                    SuggestedTeacherName = (string)payload["suggestedTeacherName"],
                    RecommendationSource = "AiProvider"
                };

                if (string.IsNullOrWhiteSpace(result.Level) || string.IsNullOrWhiteSpace(result.Plan))
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(result.SmartSchedule))
                {
                    result.SmartSchedule = "جدول ذكي غير متوفر من المزود الحالي، سيتم الاعتماد على الخطة الأساسية.";
                }

                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
