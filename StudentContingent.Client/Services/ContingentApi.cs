using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using StudentContingent.Client.Models;
using StudentContingent.Domain.Model;

namespace StudentContingent.Client.Services;

/// <summary>Ошибка, которую вернул API (текст берётся из ProblemDetails).</summary>
public class ApiException : Exception
{
    public ApiException(string message) : base(message)
    {
    }
}

/// <summary>Обращения клиента к REST API модуля.</summary>
public class ContingentApi
{
    private static readonly JsonSerializerOptions Options = CreateOptions();
    private readonly HttpClient _http;

    public ContingentApi(HttpClient http)
    {
        _http = http;
    }

    // ---- Студенты ----

    public Task<List<Student>> GetStudentsAsync(string? lastName, Guid? studyGroupId, StudentStatus? status)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(lastName))
        {
            query.Add($"lastName={Uri.EscapeDataString(lastName.Trim())}");
        }
        if (studyGroupId is not null)
        {
            query.Add($"studyGroupId={studyGroupId}");
        }
        if (status is not null)
        {
            query.Add($"status={status}");
        }
        var url = query.Count == 0 ? "api/students" : "api/students?" + string.Join("&", query);
        return GetAsync<List<Student>>(url);
    }

    public Task<Student> GetStudentAsync(Guid id) => GetAsync<Student>($"api/students/{id}");

    public Task<List<MovementEvent>> GetHistoryAsync(Guid id) => GetAsync<List<MovementEvent>>($"api/students/{id}/history");

    public Task<List<SummaryRow>> GetSummaryAsync() => GetAsync<List<SummaryRow>>("api/students/summary");

    public Task<Student> CreateStudentAsync(Student student) => PostAsync<Student, Student>("api/students", student);

    public Task UpdateStudentAsync(Student student) => SendAsync(HttpMethod.Put, $"api/students/{student.Id}", student);

    public Task DeleteStudentAsync(Guid id) => SendAsync(HttpMethod.Delete, $"api/students/{id}", null);

    public Task ApplyMovementAsync(Guid studentId, MovementRequest request) =>
        SendAsync(HttpMethod.Post, $"api/students/{studentId}/movements", request);

    // ---- Группы ----

    public Task<List<StudyGroup>> GetGroupsAsync() => GetAsync<List<StudyGroup>>("api/groups");

    public Task<StudyGroup> GetGroupAsync(Guid id) => GetAsync<StudyGroup>($"api/groups/{id}");

    public Task<List<Student>> GetGroupStudentsAsync(Guid id) => GetAsync<List<Student>>($"api/groups/{id}/students");

    public Task<StudyGroup> CreateGroupAsync(StudyGroup group) => PostAsync<StudyGroup, StudyGroup>("api/groups", group);

    public Task DeleteGroupAsync(Guid id) => SendAsync(HttpMethod.Delete, $"api/groups/{id}", null);

    // ---- Приказы ----

    public Task<List<Order>> GetOrdersAsync() => GetAsync<List<Order>>("api/orders");

    public Task<Order> CreateOrderAsync(Order order) => PostAsync<Order, Order>("api/orders", order);

    // ---- Справочники ----

    public Task<List<Department>> GetDepartmentsAsync() => GetAsync<List<Department>>("api/departments");

    public Task<Department> CreateDepartmentAsync(Department department) =>
        PostAsync<Department, Department>("api/departments", department);

    public Task<List<StudyProgram>> GetProgramsAsync() => GetAsync<List<StudyProgram>>("api/programs");

    public Task<StudyProgram> CreateProgramAsync(StudyProgram program) =>
        PostAsync<StudyProgram, StudyProgram>("api/programs", program);

    // ---- Общая часть ----

    private async Task<T> GetAsync<T>(string url)
    {
        var response = await _http.GetAsync(url);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<T>(Options))!;
    }

    private async Task<TResult> PostAsync<TBody, TResult>(string url, TBody body)
    {
        var response = await _http.PostAsJsonAsync(url, body, Options);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<TResult>(Options))!;
    }

    private async Task SendAsync(HttpMethod method, string url, object? body)
    {
        using var request = new HttpRequestMessage(method, url);
        if (body != null)
        {
            request.Content = JsonContent.Create(body, body.GetType(), options: Options);
        }
        var response = await _http.SendAsync(request);
        await EnsureSuccessAsync(response);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = $"Ошибка {(int)response.StatusCode}";
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemInfo>(Options);
            if (!string.IsNullOrWhiteSpace(problem?.Detail))
            {
                message = problem.Detail;
            }
            else if (!string.IsNullOrWhiteSpace(problem?.Title))
            {
                message = problem.Title;
            }
        }
        catch (JsonException)
        {
        }
        catch (NotSupportedException)
        {
        }
        throw new ApiException(message);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
