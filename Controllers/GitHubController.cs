using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Testing.Models; // Ensure this matches your project namespace for models
using Newtonsoft.Json;

namespace Testing.Controllers
{
    [Route("GitHub")]
    public class GitHubController : Controller
    {
        private readonly HttpClient _httpClient;

        public GitHubController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: /GitHub/Branches
        [HttpGet("Branches")]
        public async Task<IActionResult> GetBranches()
        {
            var repoOwner = "Sharnitha";  // Replace with your GitHub repo owner
            var repoName = "newrepo-dotnet";  // Replace with your GitHub repo name
            var url = $"https://api.github.com/repos/{repoOwner}/{repoName}/branches";

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Add("User-Agent", "DotNet-App");

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error fetching branches from GitHub.");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            // Deserialize the JSON response into a list of GitBranch objects
            var branches = JArray.Parse(jsonResponse).ToObject<List<GitBranch>>();

            // Create the view model and pass the branches to the view
            var model = new BranchViewModel
            {
                Branches = branches.Select(b => b.Name).ToList()  // Extract branch names
            };

            return View("Branches", model);  // Return the 'Branches' view
        }

        // POST: /GitHub/RunPipeline
        [HttpPost("RunPipeline")]
        public async Task<IActionResult> RunPipeline(string selectedBranch)
        {
            if (string.IsNullOrEmpty(selectedBranch))
            {
                return BadRequest("No branch selected.");
            }

            var repoOwner = "Sharnitha";  // Replace with your GitHub repo owner
            var repoName = "newrepo-dotnet";  // Replace with your GitHub repo name
            var workflowId = "self-ci.yml";  // Replace with your workflow file name or ID
            var url = $"https://api.github.com/repos/{repoOwner}/{repoName}/actions/workflows/{workflowId}/dispatches";

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

            // Set necessary headers
            requestMessage.Headers.Add("User-Agent", "DotNet-App");

            // Hardcoded GitHub Personal Access Token (PAT)
            var pat = "Your_Pat";  // Replace with your actual PAT
            requestMessage.Headers.Add("Authorization", $"Bearer {pat}");

            // Create JSON body to trigger the workflow with the selected branch
            var jsonBody = new
            {
                @ref = selectedBranch  // Use @ref to escape the reserved keyword
            };

            requestMessage.Content = new StringContent(JObject.FromObject(jsonBody).ToString(), System.Text.Encoding.UTF8, "application/json");

            // Send the request to trigger the workflow
            var response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                return Ok("Pipeline triggered successfully.");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Error triggering pipeline: {errorMessage}");
            }
        }
    }
}

