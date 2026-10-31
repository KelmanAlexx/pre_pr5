using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CampusDormitory.Tests;

public class DormitorySanityTest
{
    [Fact]
    public async Task CreateDormitory_ThenDelete_TestRecordIsCleanedUp()
    {
        var baseUrl = Environment.GetEnvironmentVariable("SANITY_BASE_URL");

        // Защита от случайного запуска против неизвестной системы.
        Assert.False(string.IsNullOrWhiteSpace(baseUrl),
            "Перед запуском укажите SANITY_BASE_URL тестового стенда.");

        using var client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl!)
        };

        long? createdId = null;

        try
        {
            var uniqueName = "SANITY-" + Guid.NewGuid().ToString("N");

            var response = await client.PostAsJsonAsync(
                "/api/dormitories",
                new
                {
                    name = uniqueName,
                    address = "ТЕСТОВЫЙ АДРЕС, НЕ ДЛЯ РЕАЛЬНОГО ЗАСЕЛЕНИЯ"
                });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<CreateResult>();
            Assert.NotNull(result);
            Assert.True(result!.Id > 0);
            createdId = result.Id;

            var getResponse = await client.GetAsync("/api/dormitories");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        }
        finally
        {
            // Очистка выполняется даже при ошибке проверки после создания.
            if (createdId.HasValue)
            {
                var deleteResponse = await client.DeleteAsync(
                    "/api/dormitories/" + createdId.Value);

                Assert.True(
                    deleteResponse.IsSuccessStatusCode ||
                    deleteResponse.StatusCode == HttpStatusCode.NotFound,
                    "Не удалось удалить тестовую запись. Требуется ручная проверка.");
            }
        }
    }

    private class CreateResult
    {
        public long Id { get; set; }
    }
}
